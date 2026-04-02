using LLMUnity;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public struct DialogueGroup
{
    public DialogueGroup(Player p1, Player p2)
    {
        mem1 = p1;
        mem2 = p2;
    }

    public Player mem1;
    public Player mem2;
}

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private FadeController fadeController;
    [SerializeField] private MessageSystem messageSystem; // LLM에게 Request를 보낼 수 있는 객체. 싱글톤으로 만들어야 하는데...

    [SerializeField] private string initialMessage = "안녕하세요.";

    [SerializeField] private LLMAgent localLlmAgent;

    private List<DialogueGroup> groups = new List<DialogueGroup>();
    private Player restMem;

    [SerializeField] private bool isDialogueFinished = true;
    [SerializeField] private bool isLoading = false;

    private async void Start()
    {
        List<string> users = new List<string> { "USER", "AFURE", "CHENYI", "SIGRID", "VIREO" };
        await GenerateSchedule(users);
    }

    public async Awaitable GenerateSchedule(List<string> originalUsers)
    {
        List<string> participants = new List<string>(originalUsers);

        // 1. 인원이 홀수면 "휴식" 유저를 추가하여 짝수로 맞춤
        if (participants.Count % 2 != 0)
        {
            participants.Add("REST");
        }

        int numParticipants = participants.Count;
        int numRounds = numParticipants - 1; // 라운드 수
        int halfSize = numParticipants / 2;

        for (int round = 0; round < numRounds; round++)
        {
            groups.Clear();
            // Debug.Log($"--- {round + 1}회차 대화 ---");

            for (int i = 0; i < halfSize; i++)
            {
                string u1 = participants[i];
                string u2 = participants[numParticipants - 1 - i];

                if (u1 == "REST" || u2 == "REST")
                {
                    string restUser = (u1 == "REST") ? u2 : u1;
                    if(!Enum.TryParse(restUser, false, out restMem))
                    {
                        Debug.LogError("DialogueManager 오류: Player에 없는 이름.");
                        return;
                    }
                    // Debug.Log($" - [{restUser}] 님은 이번에 쉽니다.");
                }
                else
                {
                    Player mem1, mem2;
                    if (!Enum.TryParse(u1, false, out mem1))
                    {
                        Debug.LogError("DialogueManager 오류: Player에 없는 이름.");
                        return;
                    }

                    if (!Enum.TryParse(u2, false, out mem2))
                    {
                        Debug.LogError("DialogueManager 오류: Player에 없는 이름.");
                        return;
                    }

                    groups.Add(new DialogueGroup(mem1, mem2));
                    //  Simulate(u1, u2);
                    // Debug.Log($" - {u1} <-> {u2}");
                }
            }

            int userindex = -1;
            // groups를 보고 Simulate 혹은 Dialogue 결정.
            // 1. Loading 화면 띄우기
            if (!isLoading)
            {
                fadeController.FadeIn();
                isLoading = true;
            }

            for (int k = 0; k < groups.Count; k++)
            {
                if (groups[k].mem1 == Player.USER || groups[k].mem2 == Player.USER)
                {
                    // 이러면 유예시키죠
                    userindex = k;
                }
                else
                {
                    // 여기선 바로 돌려주죠
                    // 반환값 없이 바로 파일에 Input 하는 메서드.
                    isDialogueFinished = true;
                    await messageSystem.RequestSimulateNpc(groups[k].mem1, groups[k].mem2);
                }
            }

            if(userindex > -1) // user가 index에 기록되었다면 대화 시작
            {
                Player npc = (groups[userindex].mem1 == Player.USER) ? groups[userindex].mem2 : groups[userindex].mem1;
                await messageSystem.RequestNpcResponse(initialMessage, npc);

                isDialogueFinished = false;
                if (isLoading)
                {
                    fadeController.FadeOut();
                    isLoading = false;
                }
            }

            /// 이제 대화를 Event로 지속해야 하는데,
            /// 조건이 false인 동안 계속 다음 프레임으로 넘김
            while (!isDialogueFinished)
            {
                /*
                string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "log.txt");
                string savedLog = File.ReadAllText(path);
                List<int> tokens = localLlmAgent.llm.llmService.Tokenize(savedLog);
                Debug.Log($"텍스트 토큰 수: {tokens.Count}");

                if (tokens.Count > 5000) break;
                */
                await Awaitable.NextFrameAsync();
            }

            // 2. 고정된 첫 번째 요소(0번 인덱스)를 제외하고 나머지 회전
            // 리스트의 마지막 요소를 가져와서 1번 인덱스에 삽입 (시계방향 회전)
            string lastElement = participants[numParticipants - 1];
            participants.RemoveAt(numParticipants - 1);
            participants.Insert(1, lastElement);
        }
    }
}
