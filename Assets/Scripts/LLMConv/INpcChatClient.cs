using UnityEngine;

public interface INpcChatClient
{
    Awaitable<ChatResponse> AskNpc(string jsonData);
}