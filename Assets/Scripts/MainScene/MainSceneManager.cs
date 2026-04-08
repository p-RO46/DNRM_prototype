using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private Canvas warning;

    public bool useOpenAI;
    private async void Start()
    {
        if (useOpenAI)
        {
            await WarmUpServer();
        }
    }

    private async Awaitable WarmUpServer()
    {
        using var request = UnityWebRequest.Get("https://dnrm-server.onrender.com/health");
        request.timeout = 60;
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("서버 준비 완료");
        else
            Debug.LogWarning("서버 워밍업 실패 — 첫 NPC 대화가 느릴 수 있음");
    }

    public void NewGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "Data");

        if (Directory.Exists(path))
        {
            warning.enabled = true;
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void NewGameYes()
    {
        DeleteFolder();
        SceneManager.LoadScene("GameScene");
    }

    public void NewGameNo()
    {
        warning.enabled = false;
    }

    void DeleteFolder()
    {
        string path = Path.Combine(Application.persistentDataPath, "Data");

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);

            // Debug.Log("디렉토리 삭제 완료!");
        }
    }
}
