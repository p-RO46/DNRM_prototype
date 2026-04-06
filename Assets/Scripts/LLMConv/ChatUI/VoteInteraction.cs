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
                    Debug.Log("ToggleName: "+ t.gameObject.name + " CandName: " + candidate);
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
        // 2-1-1. 숫자를 업데이트 해 준다.
        foreach (VoteResult result in messageSystem.voteResult)
        {
            foreach (TextMeshProUGUI count in voteCounts)
            {
                string str = count.gameObject.name;
                str = str.Substring(0, str.Length - 5).ToUpper();
                Debug.Log("UpdateCount: " + str);

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

        // 4. 동률을 검사한다.
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
        StartCoroutine(LampOnRoutine());
        UpdateProgress();
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

        if(electedLamp.lampCount < 2)
        {
            // 살아있음
            Image lampImage = electedLamp.lampImages[electedLamp.lampCount++];
            yield return StartCoroutine(FadeAlpha(1, 0, 1f, lampImage));
            lampImage.material = lampOn;
            yield return StartCoroutine(FadeAlpha(0, 0.5f, 1f, lampImage));
            yield return StartCoroutine(FadeAlpha(0.5f, 0.2f, 0.5f, lampImage));
            yield return StartCoroutine(FadeAlpha(0.2f, 1, 0.5f, lampImage));
        }
        else
        {
            // 죽음
            electedLamp.nameText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            for(int i = electedLamp.lampImages.Count - 1; i >= 0; i++)
            {
                Image lampImage = electedLamp.lampImages[i];
                yield return StartCoroutine(FadeAlpha(1, 0, 0.7f, lampImage));
                lampImage.material = lampDown;
                yield return StartCoroutine(FadeAlpha(0, 1f, 0.3f, lampImage));
            }
        }

        // 화면 전환
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(fadeController.FadeOutAnim());
        SceneManager.LoadScene("GameScene");
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
}
