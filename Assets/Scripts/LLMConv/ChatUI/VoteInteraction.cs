using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VoteResult
{
    public string name;
    public int count;
}
public class ProgressJson
{
    public List<VoteResult> number_of_drug_administered;
    public string current_conversation_partner;
    public int current_round;
}

public class VoteInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private FadeController fadeController;
    [SerializeField] private Canvas electorUI;
    [SerializeField] private TextMeshProUGUI electorText;

    [Header("Vote Result UI")]
    [SerializeField] private Canvas voteResultUI;
    [SerializeField] private List<TextMeshProUGUI> voteCounts = new List<TextMeshProUGUI>();

    [Header("Vote Toggles")]
    [SerializeField] private Canvas voteToggleUI; // Parent
    [SerializeField] private List<Toggle> toggles = new List<Toggle>();
    [SerializeField] private Button OkButton;

    [Header("Message System")]
    [SerializeField] private MessageSystem messageSystem;

    private string playerVote = "USER";
    private bool isRevote = false;

    public Player elector;
    [SerializeField]
    private List<string> candidates = new List<string>
    {
        "USER",
        "AFURE",
        "CHENYI",
        "SIGRID",
        "VIREO"
    };

    private List<string> revoteCands = new List<string>();

    public void OnToggleClicked()
    {
        foreach(Toggle t in toggles)
        {
            if (t.isOn)
            {
                playerVote = t.gameObject.name;
                OkButton.interactable = true;
                break;
            }
        }
        OkButton.interactable = false;
    }

    // 투표를 시작하는 메서드
    public void StartVote()
    {
        voteToggleUI.enabled = true;
        foreach(Toggle t in toggles)
        {
            // 1. 일단 전부 interactable을 비활성화
            t.interactable = false;
            foreach(string candidate in candidates)
            {
                if (t.gameObject.name.Equals(candidate))
                {
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
        fadeController.FadeIn();

        // 1. NPC들의 투표 결과를 받아온다.
        await messageSystem.RequestVote();

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
        // 2-1-1. 숫자를 업데이트 해 준다.
        foreach (VoteResult result in messageSystem.voteResult)
        {
            foreach (TextMeshProUGUI count in voteCounts)
            {
                string str = count.gameObject.name;
                str = str.Substring(str.Length - 5);

                if (result.name.Equals(str))
                {
                    count.text = result.count.ToString();
                    break;
                }
            }
        }

        // 3. 동률을 검사한다.
        revoteCands.Clear();
        for(int i = 0; i < 5; i++)
        {
            // 0표면 동률 검사 xxx
            if (messageSystem.voteResult[i].count == 0) continue;

            for(int j = i + 1; j < 5; j++)
            {
                if (messageSystem.voteResult[i].count == messageSystem.voteResult[j].count)
                {
                    revoteCands.Add(messageSystem.voteResult[i].name);
                    revoteCands.Add(messageSystem.voteResult[j].name);
                }
            }
        }
    }

    public void NextButtonClicked()
    {
        // 3-1. 동률 여부를 검사한다.
        if(revoteCands.Count == 0)
        {
            // 3-1-1. 동률이 아니라면, 최고 득표자를 선출자로 결정한다.
            string maximum = "USER";
            for (int i = 0; i < 5; i++)
            {
                // 0표면 득표 검사 xxx
                if (messageSystem.voteResult[i].count == 0) continue;

                for (int j = i + 1; j < 5; j++)
                {
                    if (messageSystem.voteResult[i].count > messageSystem.voteResult[j].count)
                    {
                        maximum = messageSystem.voteResult[i].name;
                    }
                    else
                    {
                        maximum = messageSystem.voteResult[j].name;
                    }
                }
            }

            elector = (Player)Enum.Parse(typeof(Player), maximum);
            voteResultUI.enabled = false;
            electorText.text = elector.ToString();
            electorUI.enabled = true;
            return;
        }

        // 3-2. 동률일 시, 재투표 여부를 검사한다.
        // 후보 라인업을 재투표 버전으로 바꿔준다.
        candidates = new List<string>(revoteCands);
        if (isRevote)
        {
            // 3-2-1. 재투표라면, 동률 리스트에서 한 명을 랜덤하게 선출한다.
            isRevote = false;
            int index = UnityEngine.Random.Range(0, candidates.Count);
            elector = (Player)Enum.Parse(typeof(Player), candidates[index]);
        }
        else
        {
            // 3-2-2. 재투표가 아니라면, 동률 리스트를 대상으로 재투표를 진행한다.
            isRevote = true;
            StartVote();
        }
    }

    public void ElectorOKClicked()
    {
        UpdateProgress();
        SceneManager.LoadScene("GameScene");
    }

    private void UpdateProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "progress.json");

        string oldProgressJson = PromptBuilder.ReadFileToText(path);
        ProgressJson oldProgress = JsonConvert.DeserializeObject<ProgressJson>(oldProgressJson);

        ProgressJson progress = new ProgressJson
        {
            number_of_drug_administered = messageSystem.voteResult,
            current_conversation_partner = "$DEFAULT$",
            current_round = oldProgress.current_round++
        };

        string jsonData = JsonUtility.ToJson(progress);
        messageSystem.SaveTextToFile(path, jsonData);
    }
}
