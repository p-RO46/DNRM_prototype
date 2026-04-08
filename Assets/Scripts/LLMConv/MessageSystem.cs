using LLMUnity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static UnityEngine.Audio.ProcessorInstance;

[Serializable]
public class DialogueReply
{
    public string expression;
    public string dialogue;
}

// 서버로 보낼 데이터 구조 (JSON 변환용)
[Serializable]
public class ChatRequest
{
    public string user_id;
    public string system;
    public string user_input;
}

// 서버에서 받을 데이터 구조
[Serializable]
public class ChatResponse
{
    public string reply;
    public bool filtered;
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}

[Serializable]
public class SimulateReply
{
    public string summary;
    public string opinionAB; // A가 B에 대해 가진 생각
    public string opinionBA; // B가 A에 대해 가진 생각
}

[Serializable]
public class SummarizeReply
{
    public string summary;
    public string opinion;
}

[Serializable]
public class Vote
{
    public string voter;
    public string vote;
}
public class VoteReply
{
    public List<Vote> result;
}

public class MessageSystem : MonoBehaviour
{
    [Header("DataBase")]
    [SerializeField] private PlayerDictionary playerDB;
    public DataLoad dataLoader;

    [Header("Current NPC")]
    public NpcInformation currentNpc;
    private string currentNpcName = "";
    [SerializeField] private SpriteRenderer npcImage;

    [Header("UI Contents")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI nameField;
    public ChattingUI chattingUI;
    [SerializeField] private PoolManager poolManager; // 프리팹 꺼내다 쓰는 곳
    [SerializeField] private GameObject logBoxPrefab;
    [SerializeField] private Transform logContent;
    [SerializeField] public TextMeshProUGUI tokenUsage;
    // NPC들의 투표 결과를 반환
    public List<VoteResult> voteResult = new List<VoteResult>();

    [Header("LLM Option")]
    [SerializeField] private bool useOpenAI = false; // false: local, true: OpenAI
    [SerializeField] private LLMAgent localLlmAgent;
    [SerializeField] private int maximumTokens = 5000;
    public int totalUsage = 0;

    private INpcChatClient client;
    private bool isProcessing = false; // 중복 요청 방지 플래그

    public bool isDialogueFinished = true;

    private List<string> tempLog = new List<string>();
    private Vector3 logPos = new Vector3(0, 0, 0);
    private Vector3 logScale = new Vector3(1, 1, 1);

    private void Start()
    {
        if (useOpenAI)
        {
            client = new OpenAINpcChatClient();
        }
        else
        {
            client = new LocalNpcChatClient(localLlmAgent);
        }

        inputField.enabled = true;
    }

    private void DialogueFinished(int total_tokens)
    {
        if (useOpenAI)
        {
            totalUsage += total_tokens;
            tokenUsage.text = totalUsage.ToString();
            Debug.Log($"텍스트 토큰 수: {totalUsage}");

            if (totalUsage > maximumTokens)
            {
                // isDialogueFinished = true;
                inputField.enabled = false;
                tokenUsage.color = Color.red;
            }
        }
        else
        {
            string result = string.Join("\n", tempLog);
            List<int> tokens = localLlmAgent.llm.llmService.Tokenize(result);
            tokenUsage.text = tokens.Count.ToString();
            Debug.Log($"텍스트 토큰 수: {tokens.Count}");

            if (tokens.Count > maximumTokens)
            {
                // isDialogueFinished = true;
                inputField.enabled = false;
                tokenUsage.color = Color.red;

            }
        }
    }

    #region Dialogue Request
    public async void RequestNpcResponse()
    {
        // 1. 이미 처리 중이면 무시
        if (isProcessing) return;

        // 사용자가 Enter 키를 눌렀을 때 호출
        string input = inputField.text;
        if (string.IsNullOrWhiteSpace(input)) return; // 빈 입력 방지

        //string persona = currentNpc.persona.text;
        string jsonData = "";

        if (useOpenAI)
        {
            // json 변환은 항상 요청하는 곳에서
            string systemPrompt = PromptBuilder.BuildDialoguePrompt(currentNpc, dataLoader.currentRound, tempLog);
            ChatRequest requestData = new ChatRequest { user_id = SystemInfo.deviceUniqueIdentifier, system = systemPrompt, user_input = input };
            jsonData = JsonUtility.ToJson(requestData);
        }
        else
        {
            jsonData = input;
        }

        try
        {
            isProcessing = true;
            inputField.interactable = false; // 입력창 잠금

            // "..." 표시 등으로 NPC가 생각 중임을 표현하면 좋다
            chattingUI.ShowLoadingIndicator();

            // 2. 서버 요청
            ChatResponse response = await client.AskNpc(jsonData);

            // 3. 결과 처리
            if (response != null && !string.IsNullOrEmpty(response.reply))
            {
                string reply_json = response.reply;

                // 1. 정규표현식을 사용하여 { } 구조만 추출
                // { 로 시작해서 } 로 끝나는 가장 큰 덩어리를 찾습니다.
                var match = Regex.Match(reply_json, @"\{.*\}", RegexOptions.Singleline);

                if (match.Success)
                {
                    reply_json = match.Value;
                }
                else
                {
                    // Debug.LogError("응답에서 JSON 형태를 찾을 수 없습니다: " + reply_json);
                    return;
                }

                try
                {
                    reply_json = FixJsonTypos(reply_json);
                    DialogueReply replyData = JsonConvert.DeserializeObject<DialogueReply>(reply_json);

                    if (Enum.TryParse(typeof(Expression), replyData.expression, true, out object currentExpression))
                    {
                        npcImage.sprite = currentNpc.expressions[(int)currentExpression];
                    }
                    else
                    {
                        npcImage.sprite = currentNpc.expressions[(int)Expression.DEFAULT];
                    }

                    chattingUI.PrintText(replyData.dialogue);
                    Debug.Log(reply_json);

                    // GameObject logBox1 = Instantiate(logBoxPrefab);
                    GameObject logBox1 = poolManager.Get(logBoxPrefab);
                    logBox1.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = $"[{playerDB.dictionary[Player.USER].npcName}]"; 
                    logBox1.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = input;
                    logBox1.transform.SetParent(logContent);
                    logBox1.transform.localPosition = logPos;
                    logBox1.transform.localScale = logScale;

                    // GameObject logBox2 = Instantiate(logBoxPrefab);
                    GameObject logBox2 = poolManager.Get(logBoxPrefab);
                    logBox2.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = $"[{currentNpcName}]";
                    logBox2.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = replyData.dialogue;
                    logBox2.transform.SetParent(logContent);
                    logBox2.transform.localPosition = logPos;
                    logBox2.transform.localScale = logScale;

                    string content1 = $"USER: {jsonData}";
                    string content2 = $"{currentNpcName}: {replyData.dialogue}";

                    tempLog.Add(content1);
                    tempLog.Add(content2);

                    DialogueFinished(response.total_tokens); // 토큰 사용량 체크하기.
                }
                catch (System.Exception e)
                {
                    // Debug.LogError($"파싱 실패! 원문: {reply_json} / 에러: {e.Message}");
                }
            }
            else
            {
                chattingUI.PrintText("대답을 듣지 못했습니다. 다시 시도해주세요.");
            }
        }
        catch(System.Exception e)
        {
            // Debug.LogError($"대화 중 오류 발생: {e.Message}");
            chattingUI.PrintText("네트워크 상태가 불안정합니다.");
        }
        finally
        {
            // 4. 성공하든 실패하든 입력창 다시 활성화
            isProcessing = false;
            inputField.interactable = true;
            inputField.text = ""; // 입력창 비우기
            inputField.ActivateInputField(); // 다시 포커스 주기
        }
    }

    // 최초 호출. 내용 "안녕하세요"로 고정
    public async Awaitable RequestNpcResponse(string initial, Player current)
    {
        // 1. 이미 처리 중이면 무시
        if (isProcessing) return;

        await localLlmAgent.ClearHistory();

        // System Prompt 설정은 한 번만 하기
        currentNpc = playerDB.dictionary[current];
        currentNpcName = currentNpc.player.ToString();

        npcImage.sprite = currentNpc.expressions[(int)Expression.DEFAULT];
        nameField.text = $"[{currentNpc.npcName}]";

        // 사용자가 Enter 키를 눌렀을 때 호출
        string input = initial;
        if (string.IsNullOrWhiteSpace(input)) return; // 빈 입력 방지

        //string persona = currentNpc.persona.text;
        string jsonData = "";

        if (useOpenAI)
        {
            // json 변환은 항상 요청하는 곳에서
            string systemPrompt = PromptBuilder.BuildDialoguePrompt(currentNpc, dataLoader.currentRound, tempLog);
            ChatRequest requestData = new ChatRequest { user_id = SystemInfo.deviceUniqueIdentifier, system = systemPrompt, user_input = input };
            jsonData = JsonUtility.ToJson(requestData);
        }
        else
        {
            localLlmAgent.systemPrompt = PromptBuilder.BuildDialoguePrompt(currentNpc, dataLoader.currentRound, tempLog);
            jsonData = input;
        }


        try
        {
            isProcessing = true;

            // "..." 표시 등으로 NPC가 생각 중임을 표현하면 좋다
            chattingUI.ShowLoadingIndicator();

            // 2. 서버 요청
            ChatResponse response = await client.AskNpc(jsonData);

            // 3. 결과 처리
            if (response != null && !string.IsNullOrEmpty(response.reply))
            {
                string reply_json = response.reply;

                var match = Regex.Match(reply_json, @"\{.*\}", RegexOptions.Singleline);

                if (match.Success)
                {
                    reply_json = match.Value;
                }
                else
                {
                    // Debug.LogError("응답에서 JSON 형태를 찾을 수 없습니다: " + reply_json);
                    return;
                }

                try
                {
                    reply_json = FixJsonTypos(reply_json);
                    DialogueReply replyData = JsonConvert.DeserializeObject<DialogueReply>(reply_json);

                    if (Enum.TryParse(typeof(Expression), replyData.expression, true, out object currentExpression))
                    {
                        npcImage.sprite = currentNpc.expressions[(int)currentExpression];
                    }
                    else
                    {
                        npcImage.sprite = currentNpc.expressions[(int)Expression.DEFAULT];
                    }

                    chattingUI.PrintText(replyData.dialogue);
                    GameObject logBox = Instantiate(logBoxPrefab);
                    logBox.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = $"[{currentNpcName}]";
                    logBox.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = replyData.dialogue;
                    logBox.transform.SetParent(logContent);

                    string content2 = $"{currentNpcName}: {replyData.dialogue}";

                    tempLog.Add(content2);

                }
                catch (System.Exception e)
                {
                    // Debug.LogError($"파싱 실패! 원문: {reply_json} / 에러: {e.Message}");
                }
            }
            else
            {
                chattingUI.PrintText("대답을 듣지 못했습니다. 다시 시도해주세요.");
            }
        }
        catch (System.Exception e)
        {
            // Debug.LogError($"대화 중 오류 발생: {e.Message}");
            chattingUI.PrintText("네트워크 상태가 불안정합니다.");
        }
        finally
        {
            // 4. 성공하든 실패하든 입력창 다시 활성화
            isProcessing = false;
            inputField.interactable = true;
            inputField.text = ""; // 입력창 비우기
            inputField.ActivateInputField(); // 다시 포커스 주기
        }
    }
    #endregion

    #region Simulate Request
    public async Awaitable RequestSimulateNpc(Player A, Player B)
    {
        // 1. 이미 처리 중이면 무시
        // 근데 이게 다른거 처리중일때도 무시해야하니까? MessageSystem에 묶어야 할 것 같긴 함
        if (isProcessing) return;

        string jsonData = "";

        if (useOpenAI)
        {
            // json 변환은 항상 요청하는 곳에서
            string systemPrompt = PromptBuilder.BuildSimulatePrompt(A, B, dataLoader.currentRound);
            ChatRequest requestData = new ChatRequest { user_id = SystemInfo.deviceUniqueIdentifier, system = systemPrompt, user_input = "Please provide the simulation result in JSON format." };
            jsonData = JsonUtility.ToJson(requestData);
        }
        else
        {
            await localLlmAgent.ClearHistory();
            localLlmAgent.systemPrompt = PromptBuilder.BuildSimulatePrompt(A, B, dataLoader.currentRound);
            jsonData = "Please provide the simulation result in JSON format.";
        }

        try
        {
            isProcessing = true;

            if (client == null)
            {
                await Awaitable.NextFrameAsync();
            }

            // 2. 서버 요청
            ChatResponse response = await client.AskNpc(jsonData);

            // 3. 결과 처리
            if (response != null && !string.IsNullOrEmpty(response.reply))
            {
                string reply_json = response.reply;

                // 1. 정규표현식을 사용하여 { } 구조만 추출
                // { 로 시작해서 } 로 끝나는 가장 큰 덩어리를 찾습니다.
                var match = Regex.Match(reply_json, @"\{.*\}", RegexOptions.Singleline);

                if (match.Success)
                {
                    reply_json = match.Value;
                }
                else
                {
                    // Debug.LogError("응답에서 JSON 형태를 찾을 수 없습니다: " + reply_json);
                    return;
                }

                try
                {
                    SimulateReply replyData = JsonConvert.DeserializeObject<SimulateReply>(reply_json);

                    string playerA = A.ToString().ToUpper(); // 경로 일관성을 위해 대문자 변환 권장
                    string playerB = B.ToString().ToUpper();

                    string summary = replyData.summary;
                    string opinionAB = replyData.opinionAB;
                    string opinionBA = replyData.opinionBA;

                    // 1. 경로 설정 (Application.persistentDataPath 사용 시 Resources는 생략하는 것이 일반적입니다)
                    // 만약 꼭 폴더를 구분하고 싶다면 아래와 같이 설정하세요.
                    string summaryA = Path.Combine(Application.persistentDataPath, "Data", playerA, "summary", $"{playerB}.txt");
                    string summaryB = Path.Combine(Application.persistentDataPath, "Data", playerB, "summary", $"{playerA}.txt");
                    string pathA = Path.Combine(Application.persistentDataPath, "Data", playerA, "opinion", $"{playerB}.txt");
                    string pathB = Path.Combine(Application.persistentDataPath, "Data", playerB, "opinion", $"{playerA}.txt");

                    // 2. 파일 저장 실행 (저장용 헬퍼 함수 호출)
                    SaveTextToFile(summaryA, summary);
                    SaveTextToFile(summaryB, summary);
                    SaveTextToFile(pathA, opinionAB);
                    SaveTextToFile(pathB, opinionBA);

                    // Debug.Log($"저장 완료: {pathA}");
                }
                catch (System.Exception e)
                {
                    // Debug.LogError($"파싱 실패! 원문: {reply_json} / 에러: {e.Message}");
                }
            }
            else
            {
                // Debug.LogError("Simulate: 대답을 받지 못했습니다.");
                // chattingUI.PrintText("대답을 듣지 못했습니다. 다시 시도해주세요.");
            }
        }
        catch (System.Exception e)
        {
            // Debug.LogError($"Simulate 중 오류 발생: {e.Message}");
            // chattingUI.PrintText("네트워크 상태가 불안정합니다.");
        }
        finally
        {
            isProcessing = false;
        }
    }
    #endregion

    #region Summarize Request
    public async Awaitable RequestSummarize()
    {
        // User의 방금 대화 Log를 summarize 하고
        // USER/summary/NPC이름.txt 에 저장하면 될 것 같습니다!
        
        // 1. 이미 처리 중이면 무시
        if (isProcessing) return;

        string jsonData = "";
        if (useOpenAI)
        {
            // json 변환은 항상 요청하는 곳에서
            string systemPrompt = PromptBuilder.BuildSummarizePrompt(currentNpc, dataLoader.currentRound);
            ChatRequest requestData = new ChatRequest { user_id = SystemInfo.deviceUniqueIdentifier, system = systemPrompt, user_input = "Please provide the summarization result in JSON format." };
            jsonData = JsonUtility.ToJson(requestData);
        }
        else
        {
            await localLlmAgent.ClearHistory();
            localLlmAgent.systemPrompt = PromptBuilder.BuildSummarizePrompt(currentNpc, dataLoader.currentRound);
            jsonData = "Please provide the summarization result in JSON format.";
        }

        try
        {
            isProcessing = true;

            // 2. 서버 요청
            ChatResponse response = await client.AskNpc(jsonData);

            // 3. 결과 처리
            if (response != null && !string.IsNullOrEmpty(response.reply))
            {
                string reply_json = response.reply;

                // 1. 정규표현식을 사용하여 { } 구조만 추출
                // { 로 시작해서 } 로 끝나는 가장 큰 덩어리를 찾습니다.
                var match = Regex.Match(reply_json, @"\{.*\}", RegexOptions.Singleline);

                if (match.Success)
                {
                    reply_json = match.Value;
                }
                else
                {
                    // Debug.LogError("응답에서 JSON 형태를 찾을 수 없습니다: " + reply_json);
                    return;
                }

                try
                {
                    SummarizeReply replyData = JsonConvert.DeserializeObject<SummarizeReply>(reply_json);

                    // 저장할 파일 경로 설정 (Application.persistentDataPath는 플랫폼별 저장 경로)
                    string sPath = Path.Combine(Application.persistentDataPath, "Data", $"{currentNpcName}", "summary", $"USER.txt");
                    string oPath = Path.Combine(Application.persistentDataPath, "Data", $"{currentNpcName}", "opinion", $"USER.txt");

                    string summary = replyData.summary;
                    string opinion = replyData.opinion;
                    // 파일 쓰기
                    File.WriteAllText(sPath, summary);
                    File.WriteAllText(oPath, opinion);

                    // Debug.Log("Summarize: 파일 저장 완료!");
                }
                catch (System.Exception e)
                {
                    // Debug.LogError($"파싱 실패! 원문: {reply_json} / 에러: {e.Message}");
                }
            }
            else
            {
                // Debug.LogError("Summarize: 대답을 듣지 못했습니다.");
                // chattingUI.PrintText("대답을 듣지 못했습니다. 다시 시도해주세요.");
            }
        }
        catch (System.Exception e)
        {
            // Debug.LogError($"Summarize 중 오류 발생: {e.Message}");
        }
        finally
        {
            isProcessing = false;

        }
    }

    public async Awaitable DialogueSave()
    {
        string result = string.Join("\n", tempLog);
        string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "log.txt");
        SaveTextToFile(path, result);

        /*
        foreach(Transform child in logContent)
        {
            // child.SetParent(null);
            child.GetComponent<PooledObject>().ReturnToPool();
        }
        */

        for (int i = logContent.childCount - 1; i >= 0; i--)
        {
            // 1. 일반 삭제일 경우
            // Destroy(logContent.GetChild(i).gameObject);

            // 2. 오브젝트 풀링을 사용 중일 경우 (PooledObject 스크립트가 있다고 가정)
            var pooled = logContent.GetChild(i).GetComponent<PooledObject>();
            if (pooled != null) pooled.ReturnToPool();
        }

        tempLog.Clear();
        await RequestSummarize();
    }
    #endregion

    #region Vote Request
    public async Awaitable RequestVote(List<string> candidates)
    {
        // 1. 이미 처리 중이면 무시
        if (isProcessing) return;

        string jsonData = "";
        if (useOpenAI)
        {
            // json 변환은 항상 요청하는 곳에서
            string systemPrompt = PromptBuilder.BuildVotePrompt(candidates);
            ChatRequest requestData = new ChatRequest { user_id = SystemInfo.deviceUniqueIdentifier, system = systemPrompt, user_input = "Please provide the vote result in JSON format." };
            jsonData = JsonUtility.ToJson(requestData);
        }
        else
        {
            await localLlmAgent.ClearHistory();
            localLlmAgent.systemPrompt = PromptBuilder.BuildVotePrompt(candidates);
            jsonData = "Please provide the vote result in JSON format.";
        }

        try
        {
            isProcessing = true;

            // 2. 서버 요청
            ChatResponse response = await client.AskNpc(jsonData);

            // 3. 결과 처리
            if (response != null && !string.IsNullOrEmpty(response.reply))
            {
                string reply_json = response.reply;
                Debug.Log(reply_json);

                // 1. 정규표현식을 사용하여 { } 구조만 추출
                // { 로 시작해서 } 로 끝나는 가장 큰 덩어리를 찾습니다.
                var match = Regex.Match(reply_json, @"\{.*\}", RegexOptions.Singleline);

                if (match.Success)
                {
                    reply_json = match.Value;
                }
                else
                {
                    // Debug.LogError("응답에서 JSON 형태를 찾을 수 없습니다: " + reply_json);
                    return;
                }

                try
                {
                    VoteReply replyData = JsonConvert.DeserializeObject<VoteReply>(reply_json);
                    // string[] keys = { "USER", "AFURE", "CHENYI", "SIGRID", "VIREO" };
                    // LINQ의 ToDictionary 사용
                    Dictionary<string, int> voteData = candidates.ToDictionary(key => key, key => 0);
                    foreach(Vote v in replyData.result)
                    {
                        if (voteData.ContainsKey(v.vote))
                        {
                            // 이미 있으면 1표 증가
                            voteData[v.vote]++;
                        }
                        else
                        {
                            // 랜덤으로 돌려야만
                            List<string> randKey = voteData.Keys.ToList(); // 값을 리스트로 변환
                            int _index = UnityEngine.Random.Range(0, randKey.Count);
                            voteData[randKey[_index]]++;
                        }
                    }

                    voteResult.Clear();
                    foreach (KeyValuePair<string, int> entry in voteData)
                    {
                        voteResult.Add(new VoteResult { name = entry.Key, count = entry.Value });
                    }
                }
                catch (System.Exception e)
                {
                    // Debug.LogError($"Vote 파싱 실패! 원문: {reply_json} / 에러: {e.Message}");
                }
            }
            else
            {
                // Debug.LogError("Vote: 대답을 듣지 못했습니다.");
                // chattingUI.PrintText("대답을 듣지 못했습니다. 다시 시도해주세요.");
            }
        }
        catch (System.Exception e)
        {
            // Debug.LogError($"vote 중 오류 발생: {e.Message}");
        }
        finally
        {
            isProcessing = false;
        }
    }
    #endregion

    // --- 헬퍼 함수 ---
    public void SaveTextToFile(string fullPath, string content)
    {
        try
        {
            // 파일이 저장될 폴더 경로만 추출
            string directoryPath = Path.GetDirectoryName(fullPath);

            // 폴더가 없으면 생성
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                // Debug.Log($"폴더 생성됨: {directoryPath}");
            }

            // 파일 쓰기
            File.WriteAllText(fullPath, content);
        }
        catch (System.Exception e)
        {
            // Debug.LogError($"파일 저장 실패: {e.Message}");
        }
    }

    public string FixJsonTypos(string rawJson)
    {
        // 자주 발생하는 오타들을 정상적인 키로 치환
        return rawJson
            .Replace("\"dialogage\":", "\"dialogue\":");
    }
}
