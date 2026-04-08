using UnityEngine;
using UnityEngine.Networking;
using System;

public class OpenAINpcChatClient : INpcChatClient
{
    // 로컬 서버 주소 (실제 배포 시에는 서버 IP로 변경)
    private string serverUrl = "https://dnrm-server.onrender.com/chat";

    public async Awaitable<ChatResponse> AskNpc(string jsonData)
    {
        // HTTP POST 요청 생성
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            Debug.Log($"[Request] 서버로 보내는 데이터: {jsonData}");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.timeout = 60;
            Debug.Log("[Status] 서버 응답 대기 중... (무료 서버는 최대 1분 소요될 수 있음)");

            try
            {
                // 유니티 6에서는 SendWebRequest()를 바로 await 할 수 있습니다.
                // await request.SendWebRequest();
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    await Awaitable.WaitForSecondsAsync(2);
                    await request.SendWebRequest();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[Success] 서버 응답 도착: {request.downloadHandler.text}");
                    return JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                }
                else
                {
                    // Debug.LogError($"서버 응답 오류: {request.error}");
                    Debug.LogError($"서버 응답 오류: {request.responseCode} / {request.error}");
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
