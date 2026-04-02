using UnityEngine;
using UnityEngine.Networking;
using System;

public class OpenAINpcChatClient : INpcChatClient
{
    // 로컬 서버 주소 (실제 배포 시에는 서버 IP로 변경)
    private string serverUrl = "http://127.0.0.1:8000/chat";

    public async Awaitable<ChatResponse> AskNpc(string jsonData)
    {
        // ***이 작업을 요청하는 쪽에서 하도록 변경***
        // ChatRequest requestData = new ChatRequest { user_input = input, persona = persona };
        // string jsonData = JsonUtility.ToJson(requestData);

        // HTTP POST 요청 생성
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                // 유니티 6에서는 SendWebRequest()를 바로 await 할 수 있습니다.
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"서버 응답 오류: {request.error}");
                    return new ChatResponse { reply = "서버 통신에 실패했습니다.", filtered = true };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"비동기 요청 중 예외 발생: {e.Message}");
                return new ChatResponse { reply = "알 수 없는 오류가 발생했습니다.", filtered = true };
            }
        }

    }
}
