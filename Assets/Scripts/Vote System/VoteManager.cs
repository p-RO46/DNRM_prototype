using UnityEngine;
using System;
using LLMUnity;
using Newtonsoft.Json;

[Serializable] // 유니티 인스펙터 확인용 (필수는 아님)
public class VoteData
{
    public Player voter; // 투표자
    public Player subject; // 투표 대상
}

public class VoteManager : MonoBehaviour
{
    // 투표 정보는 텍스트 파일로 누적되어 저장? 사실 바로 직전 투표수만 전달해도 괜찮지 않나 싶긴함
    // 누적하려면 외부파일을 만들어야 하지만, 아니라면 scriptable object로 충분하지
    // 불러오기에 대해서는 나중가서...
    [SerializeField] private VoteInformation voteInfo;
    // Npc Information
    [SerializeField] 
    private NpcInformation[] npcInfo = new NpcInformation[4];

    [Header("LLM Option")]
    public bool useOpenAI = false; // false: local, true: OpenAI
    public LLMAgent localLlmAgent;
    public string prompt = "";
     
    private INpcChatClient client;
    private bool isProcessing = false; // 중복 요청 방지 플래그

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

        /*
        VoteData vote = new VoteData
        {
            voter = Player.AFURE,
            subject = Player.USER
        };

        // -----------------------------------------
        // 2. 객체 -> JSON 문자열 (직렬화)
        // -----------------------------------------
        string json = JsonConvert.SerializeObject(vote, Formatting.Indented);
        Debug.Log("생성된 JSON: \n" + json);

        // -----------------------------------------
        // 3. JSON 문자열 -> 객체 (역직렬화)
        // -----------------------------------------
        string receivedJson = "{\"voter\":\"AFURE\",\"subject\":\"CHENYI\"}";
        VoteData loadedData = JsonConvert.DeserializeObject<VoteData>(receivedJson);

        Debug.Log($"투표자: {loadedData.voter}, 대상: {loadedData.subject}");
        */
    }

    /// 순차적으로 돌면서 투표 정보를 받아온다.
    /// User 꺼를 마지막으로 받는다.
    private void Voting()
    {
        for(int i = 0; i < 4; i++)
        {

        }
    }

    // User -> LLM Server
    private async void RequestNpcVote()
    {
        // 1. 이미 처리 중이면 무시
        if (isProcessing) return;

        try
        {
            isProcessing = true;

            // 2. 서버 요청
            ChatResponse response = await client.AskNpc(prompt);

            // 3. 결과 처리
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"투표 중 오류 발생: {e.Message}");
        }
        finally
        {
            // 4. 성공하든 실패하든 입력창 다시 활성화
            isProcessing = false;
        }
    }
}
