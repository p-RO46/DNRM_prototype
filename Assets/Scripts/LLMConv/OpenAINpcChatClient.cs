using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAINpcChatClient : INpcChatClient
{
    // 로컬 서버 주소 (실제 배포 시에는 서버 IP로 변경)
    private string serverUrl = "https://dnrm-server.onrender.com/chat";
    private bool isAsking = false; // 요청은 한 번에 하나씩만
    private float retryDelay = 3f;

    public async Awaitable<ChatResponse> AskNpc(string jsonData)
    {
        
        if (isAsking)
        {
            Debug.LogWarning("[Wait] 이미 답변을 기다리는 중입니다.");
            return new ChatResponse { reply = "이미 처리 중인 요청이 있습니다.", filtered = true };
        }

        isAsking = true;

        try
        {
            int maxRetries = 3; // 최대 시도 횟수
            for (int i = 0; i < maxRetries; i++)
            {
                // 매 시도마다 'using'을 통해 새 WebRequest 객체를 생성해야 함
                using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
                {
                    Debug.Log($"[Attempt {i + 1}] 서버 요청 중...");

                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = 120;

                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"[Success] 서버 응답 도착: {request.downloadHandler.text}");
                        return JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                    }

                    long code = request.responseCode;
                    if (code == 521 || code == 502 || code == 503 || code == 0)
                    {
                        Debug.LogWarning($"[Retry {i + 1}/{maxRetries}] Server not ready (code: {code}), waiting {retryDelay}s...");
                        await Awaitable.WaitForSecondsAsync(retryDelay);
                        retryDelay *= 2; // 지수 백오프
                        continue;
                    }

                    // 실패했을 경우
                    Debug.LogWarning($"[Error] 시도 {i + 1} 실패: {request.error}");
                    // 상세한 에러 로그 출력
                    Debug.LogError($"[Detailed Error] URL: {request.url}");
                    Debug.LogError($"[Detailed Error] Result: {request.result}");
                    Debug.LogError($"[Detailed Error] Response Code: {request.responseCode}");
                    Debug.LogError($"[Detailed Error] Error Message: {request.error}");

                    // 마지막 시도였다면 루프 종료 후 에러 처리로 이동
                    if (i == maxRetries - 1) break;
                }
            }

            return new ChatResponse { reply = "서버 통신에 최종 실패했습니다.", filtered = true };
        }
        catch (Exception e)
        {
            Debug.LogError($"비동기 요청 중 예외 발생: {e.Message}");
            return new ChatResponse { reply = "알 수 없는 오류가 발생했습니다.", filtered = true };
        }
        finally
        {
            isAsking = false;
            Debug.Log("[Status] 요청 프로세스 종료.");
        }
    }

    /*
    public async Awaitable<ChatResponse> AskNpc(string jsonData)
    {
        if (isAsking)
        {
            Debug.LogWarning("[Wait] 이미 답변을 기다리는 중입니다. 잠시만 기다려 주세요.");
            return new ChatResponse { reply = "이미 처리 중인 요청이 있습니다.", filtered = true };
        }

        isAsking = true; // 요청 시작 상태로 변경

        try
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
        }
        catch (Exception e)
        {
            Debug.LogError($"비동기 요청 중 예외 발생: {e.Message}");
            return new ChatResponse { reply = "알 수 없는 오류가 발생했습니다.", filtered = true };
        }
        finally
        {
            // 2. 어떤 상황에서도(성공, 실패, 에러) 마지막엔 플래그를 해제
            isAsking = false;
            Debug.Log("[Status] 요청 처리가 완료되어 다음 요청이 가능합니다.");
        }
    }
    */
}

