using JetBrains.Annotations;
using LLMUnity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class VoteState
{
    public MyState my_state;
    public string prev_round_administered;
    public string prev_round_I_voted;
}
[Serializable]
public class MyState
{
    public int administered_count;
    public string physical_state;
}

// 투표 시 사망자 처리 필요
[Serializable]
public class VoteResult
{
    public string name;
    public int count;
}

[Serializable]
public class ProgressJson
{
    public List<VoteResult> number_of_drug_administered;
    public string current_conversation_partner;
    public int current_round;
}

[Serializable]
public class PlayerLamp
{
    public Player player = Player.USER;
    public int lampCount = 0;
    public TextMeshProUGUI nameText;
    public List<Image> lampImages;
}

public class VoteInteraction : MonoBehaviour
{
    [Header("Data Loader")]
    [SerializeField] private DataLoad dataLoader;

    [Header("UI")] 
    [SerializeField] private Canvas voteMainUI;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private Canvas electorUI;
    [SerializeField] private TextMeshProUGUI electorText;

    [Header("Vote Result UI")]
    [SerializeField] private Canvas voteResultUI;
    [SerializeField] private List<TextMeshProUGUI> voteCounts = new List<TextMeshProUGUI>();
    [SerializeField] private List<PlayerLamp> playerLamp = new List<PlayerLamp>();
    [SerializeField] private Material lampOn;
    [SerializeField] private Material lampOff;
    [SerializeField] private Material lampDown;

    [Header("Vote Toggles")]
    [SerializeField] private Canvas voteToggleUI; // Parent
    [SerializeField] private List<Toggle> toggles = new List<Toggle>();
    [SerializeField] private Button OkButton;

    [Header("Message System")]
    [SerializeField] private MessageSystem messageSystem;

    private string playerVote = "USER";
    [SerializeField] private bool isRevote = false;
    [SerializeField] private bool isTie = false;

    [SerializeField] private LLMAgent llmAgent;

    public Player elector;
    public List<string> candidates = new List<string>();
    private List<string> revoteCands = new List<string>();

    private Dictionary<int, string> state_per_injection = new Dictionary<int, string>
    {
        {0,  "Healthy. There's nothing physically wrong."},
        {1,  "Feel dizzy. I feel sick and feel like throwing up."},
        {2, "It's hard to stand. It's hard to breathe. My head hurts like it's breaking."}
    };

    public void OnToggleClicked()
    {
        foreach(Toggle t in toggles)
        {
            if (t.isOn)
            {
                playerVote = t.gameObject.name;
                OkButton.interactable = true;
                return;
            }
        }
        OkButton.interactable = false;
    }

    // 투표를 시작하는 메서드
    public void StartVote()
    {
        voteMainUI.enabled = true;
        voteToggleUI.enabled = true;
        voteResultUI.enabled = false;
        foreach(Toggle t in toggles)
        {
            // 1. 일단 전부 interactable을 비활성화
            t.interactable = false;
            foreach(string candidate in candidates)
            {
                if (t.gameObject.name.Equals(candidate))
                {
                    // Debug.Log("ToggleName: "+ t.gameObject.name + " CandName: " + candidate);
                    // 2. 후보 라인업에 있으면 interactable 활성화
                    t.interactable = true;
                    break;
                }
            }
        }
    }

    // 투표 결과를 계산하는 메서드
    public async void CountVotes()
    {
        // 0. 투표 토글 닫기
        voteToggleUI.enabled = false;
        voteResultUI.enabled = false;
        fadeController.FadeIn();

        Debug.Log(string.Join(", ", candidates));

        // 1. NPC들의 투표 결과를 받아온다.
        await messageSystem.RequestVote(candidates);

        // 2. USER의 표를 더한다.
        foreach (VoteResult result in messageSystem.voteResult)
        {
            if (result.name.Equals(playerVote))
            {
                result.count++;
                break;
            }
        }

        // 2-1. 투표 결과 화면을 띄운다.
        fadeController.FadeOut();
        voteResultUI.enabled = true;

        foreach (TextMeshProUGUI count in voteCounts)
        {
            count.text = "0";
        }

        // 2-1-1. 숫자를 업데이트 해 준다.
        foreach (VoteResult result in messageSystem.voteResult)
        {
            foreach (TextMeshProUGUI count in voteCounts)
            {
                string str = count.gameObject.name;
                str = str.Substring(0, str.Length - 5).ToUpper();
                // Debug.Log("UpdateCount: " + str);

                if (result.name.Equals(str))
                {
                    count.text = result.count.ToString();
                    break;
                }
            }
        }

        isTie = false;
        // 3. 최고 득표자를 찾는다.
        string maximum = "USER";
        int max = 0;
        for (int i = 0; i < messageSystem.voteResult.Count; i++)
        {
            // 0표면 득표 검사 xxx
            if (messageSystem.voteResult[i].count == 0) continue;

            if (messageSystem.voteResult[i].count > max)
            {
                maximum = messageSystem.voteResult[i].name;
                max = messageSystem.voteResult[i].count;
            }
        }

        // 4. 동률을 검사한다.
        revoteCands.Clear();
        for(int i = 0; i < candidates.Count; i++)
        {
            // 0표면 동률 검사 xxx
            if (messageSystem.voteResult[i].count == 0) continue;

            for(int j = i + 1; j < candidates.Count; j++)
            {
                if (messageSystem.voteResult[i].count == messageSystem.voteResult[j].count)
                {
                    revoteCands.Add(messageSystem.voteResult[i].name);
                    revoteCands.Add(messageSystem.voteResult[j].name);
                }
            }
        }

        foreach (string cand in revoteCands)
        {
            if (cand.Equals(maximum)) 
            {
                isTie = true;
                return;
            }
        }
        elector = (Player)Enum.Parse(typeof(Player), maximum);
    }

    public void NextButtonClicked()
    {
        if(!isTie)
        {
            // 4-1. 동률이 아니라면, 최고 득표자를 선출자로 결정한다.
            voteToggleUI.enabled = false;
            voteResultUI.enabled = false;
            electorUI.enabled = true;
            electorText.text = elector.ToString();

            // 실제 저장 데이터에도 선출자의 표를 추가한다.
            dataLoader.currentProgress.number_of_drug_administered[(int)elector].count ++;

            return;
        }

        // 4-2. 동률일 시, 재투표 여부를 검사한다.
        // 후보 라인업을 재투표 버전으로 바꿔준다.
        candidates = new List<string>(revoteCands).Distinct().ToList();
        if (isRevote)
        {
            // 4-2-1. 재투표라면, 동률 리스트에서 한 명을 랜덤하게 선출한다.
            isRevote = false;
            int index = UnityEngine.Random.Range(0, candidates.Count);
            elector = (Player)Enum.Parse(typeof(Player), candidates[index]);

            voteToggleUI.enabled = false;
            voteResultUI.enabled = false;
            electorUI.enabled = true;
            electorText.text = elector.ToString();

            // 실제 저장 데이터에도 선출자의 표를 추가한다.
            dataLoader.currentProgress.number_of_drug_administered[(int)elector].count++;
        }
        else
        {
            // 4-2-2. 재투표가 아니라면, 동률 리스트를 대상으로 재투표를 진행한다.
            isRevote = true;
            StartVote();
        }
    }

    public void ElectorOKClicked()
    {
        voteResultUI.enabled = false;
        voteToggleUI.enabled = false;
        voteMainUI.enabled = false;
        UpdateProgress();
        StartCoroutine(LampOnRoutine());
    }

    private void UpdateProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "progress.json");

        string oldProgressJson = PromptBuilder.ReadFileToText(path);
        ProgressJson oldProgress = JsonConvert.DeserializeObject<ProgressJson>(oldProgressJson);

        ProgressJson progress = new ProgressJson
        {
            number_of_drug_administered = dataLoader.currentProgress.number_of_drug_administered,
            current_conversation_partner = "$DEFAULT$",
            current_round = oldProgress.current_round + 1
        };

        string jsonData = JsonUtility.ToJson(progress);
        // Debug.Log(jsonData);
        messageSystem.SaveTextToFile(path, jsonData);
        UpdateVoteState();
    }

    private void UpdateVoteState()
    {
        // Screen Scene이기에 가능.
        foreach(Player player in dataLoader.users)
        {
            if (player == Player.USER) continue;

            string player_name = player.ToString();
            foreach(VoteResult result in dataLoader.currentProgress.number_of_drug_administered)
            {
                if (result.name.Equals(player_name) && result.count < 3)
                {
                    VoteState voteState = new VoteState { my_state = new MyState() };
                    if(result == null)
                    {
                        Debug.LogError("result가 Null입니다.");
                    }
                    voteState.my_state.administered_count = result.count;
                    voteState.my_state.physical_state = state_per_injection[result.count];
                    voteState.prev_round_administered = elector.ToString();

                    foreach(Vote vote in messageSystem.vote_reply_state.result)
                    {
                        if (vote.voter.Equals(player_name))
                        {
                            voteState.prev_round_I_voted = vote.vote;
                        }
                    }
                    
                    string content = JsonConvert.SerializeObject(voteState);
                    string path = Path.Combine(Application.persistentDataPath, "Data", player_name, "vote_state.json");
                    messageSystem.SaveTextToFile(path, content);
                }
            }
        }
    }

    private IEnumerator LampOnRoutine()
    {
        PlayerLamp electedLamp = new PlayerLamp();
        foreach(PlayerLamp p in playerLamp)
        {
            if(p.player == elector)
            {
                electedLamp = p;
                break;
            }
        }

        int lampNum = 0;
        foreach (VoteResult result in dataLoader.currentProgress.number_of_drug_administered)
        {
            Debug.Log(result.name);
            if (result.name.Equals(elector.ToString()))
            {
                Debug.Log(result.count);
                lampNum = result.count - 1;
                break;
            }
        }

        Image lampImage = electedLamp.lampImages[lampNum];
        yield return StartCoroutine(FadeAlpha(1, 0, 1f, lampImage));
        lampImage.material = lampOn;
        yield return StartCoroutine(FadeAlpha(0, 0.5f, 1f, lampImage));
        yield return StartCoroutine(FadeAlpha(0.5f, 0.2f, 0.3f, lampImage));
        yield return StartCoroutine(FadeAlpha(0.2f, 1, 0.2f, lampImage));

        // 화면 전환
        yield return StartCoroutine(fadeController.FadeOutAnim());
        ChangeScene();
    }

    private IEnumerator FadeAlpha(float start, float end, float duration, Image img)
    {
        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, elapsed / duration);
            img.color = c;
            yield return null;
        }
    }

    private void ChangeScene()
    {
        if(dataLoader.currentProgress.number_of_drug_administered[0].count >= 3)
        {
            SceneManager.LoadScene("EndingScene");
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
