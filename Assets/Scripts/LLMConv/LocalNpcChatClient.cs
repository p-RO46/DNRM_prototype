using UnityEngine;
using LLMUnity;

public class LocalNpcChatClient : INpcChatClient
{
    private LLMAgent llmAgent;

    public LocalNpcChatClient(LLMAgent llmAgent)
    {
        this.llmAgent = llmAgent;
    }

    public async Awaitable<ChatResponse> AskNpc(string jsonData)
    {
        string _reply = await llmAgent.Chat(jsonData);

        // 필터링이 애매해서 일단 이렇게...
        return new ChatResponse { reply = _reply, filtered = false };
    }
}
