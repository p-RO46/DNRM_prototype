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
    [Header("Sound")]
    [SerializeField] private SoundManager BGMManager;

    [Header("UI Controller")]
    [SerializeField] private FadeController fadeController;
    [SerializeField] private VoteInteraction voteInteraction;
    [SerializeField] private Canvas dialogueUI;

    [Header("LLM")]
    [SerializeField] private MessageSystem messageSystem; // LLM에게 Request를 보낼 수 있는 객체. 싱글톤으로 만들어야 하는데...

    [SerializeField] private string initialMessage = "안녕하세요.";

    [SerializeField] private LLMAgent localLlmAgent;

    private List<DialogueGroup> groups = new List<DialogueGroup>();
    private Player restMem;

    public List<Player> users = new List<Player>();

    private async void Start()
    {
        // users = new List<string> { "USER", "CHENYI", "SIGRID", "VIREO", "AFURE" };
        dialogueUI.enabled = false;

        BGMManager.ChangeClip(AC.BGM_EMPTY);
        await GenerateSchedule(users);
    }

    public async Awaitable GenerateSchedule(List<Player> originalUsers)
    {
        List<Player> participants = new List<Player>(originalUsers);

        // 1. 인원이 홀수면 "휴식" 유저를 추가하여 짝수로 맞춤
        if (participants.Count % 2 != 0)
        {
            participants.Add(Player.REST);
        }

        int numParticipants = participants.Count;
        int numRounds = numParticipants - 1; // 라운드 수
        int halfSize = numParticipants / 2;

        for (int round = 0; round < numRounds; round++)
        {
            groups.Clear();
            // Debug.Log($"--- {round + 1}회차 대화 ---");

            // 1. Loading 화면 띄우기
            fadeController.FadeIn();

            for (int i = 0; i < halfSize; i++)
            {
                Player u1 = participants[i];
                Player u2 = participants[numParticipants - 1 - i];

                if (u1 == Player.REST || u2 == Player.REST)
                {
                    restMem = (u1 == Player.REST) ? u2 : u1;
                    Debug.Log($" - [{restMem.ToString()}] 님은 이번에 쉽니다.");
                }
                else
                {
                    groups.Add(new DialogueGroup(u1, u2));
                    //  Simulate(u1, u2);
                    Debug.Log($" - {u1.ToString()} <-> {u2.ToString()}");
                }
            }

            int userindex = -1;
            // groups를 보고 Simulate 혹은 Dialogue 결정.

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
                    messageSystem.isDialogueFinished = true;
                    await messageSystem.RequestSimulateNpc(groups[k].mem1, groups[k].mem2);
                }
            }

            if(userindex >= 0) // user가 index에 기록되었다면 대화 시작
            {
                Player npc = (groups[userindex].mem1 == Player.USER) ? groups[userindex].mem2 : groups[userindex].mem1;

                await localLlmAgent.ClearHistory();
                await messageSystem.RequestNpcResponse(initialMessage, npc);

                fadeController.FadeOut();
                /// 이제 대화를 Event로 지속해야 하는데,
                /// 조건이 false인 동안 계속 다음 프레임으로 넘김
                messageSystem.isDialogueFinished = false;
                dialogueUI.enabled = true;
                BGMManager.ChangeClip(AC.BGM_LONG);
                while (!messageSystem.isDialogueFinished)
                {
                    await Awaitable.NextFrameAsync();
                }
                dialogueUI.enabled = false;

                /// 탈출 이후에 NPC가 끝내는 말 해야하는데 이건 그냥 고정으로 할까
                /// 요청을 쓸데없이 한 번 더 보내는 느낌이라. 그냥 고정멘트 달아두는게 좋겠다
                /// 끝난 다음에는 (1)입력창 비활성화 (2)끝내기버튼 활성화 이런식인가

                fadeController.FadeIn();
                BGMManager.ChangeClip(AC.BGM_EMPTY);
                messageSystem.tokenUsage.text = "0";
                await messageSystem.DialogueSave();
            }

            // 2. 고정된 첫 번째 요소(0번 인덱스)를 제외하고 나머지 회전
            // 리스트의 마지막 요소를 가져와서 1번 인덱스에 삽입 (시계방향 회전)
            Player lastElement = participants[numParticipants - 1];
            participants.RemoveAt(numParticipants - 1);
            participants.Insert(1, lastElement);
        }

        // 최종 뺑이가 끝나면 vote 시작.
        fadeController.FadeOut();
        voteInteraction.StartVote();
    }

    public void EndDialogue()
    {
        messageSystem.isDialogueFinished = true;
    }
}
