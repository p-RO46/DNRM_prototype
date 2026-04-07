using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private Canvas warning;
    [SerializeField] private TextMeshProUGUI playTimeText;
    private void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "playTime.txt");
        if (File.Exists(path))
        {
            PlayTimer.TotalPlayTime = float.Parse(File.ReadAllText(path));
        }
        else
        {
            PlayTimer.TotalPlayTime = 0;
        }

        playTimeText.text = $"{PlayTimer.TotalPlayTime} s";
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
