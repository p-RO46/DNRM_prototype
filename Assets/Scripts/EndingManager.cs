using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;

[Serializable]
public class Ending
{
    public int round;
    public List<string> contents;
}

[Serializable]
public class Endings
{
    public List<Ending> endings;
}
public class EndingManager : MonoBehaviour
{
    [SerializeField] ChattingUI chatting;
    [SerializeField] TextMeshProUGUI textBox;
    private Endings dnrm_endings = new Endings();
    private bool isWaiting = false;
    private async void Start()
    {
        TextAsset endingAsset = Resources.Load<TextAsset>("Common/Ending");
        dnrm_endings = JsonConvert.DeserializeObject<Endings>(endingAsset.text);

        string path = Path.Combine(Application.persistentDataPath, "Data", "Common", "progress.json");
        string progress_json = File.ReadAllText(path);
        ProgressJson progress = JsonConvert.DeserializeObject<ProgressJson>(progress_json);

        await PrintEnding(progress.current_round);
    }

    public void SetWaiting()
    {
        isWaiting = false;
    }

    private async Awaitable PrintEnding(int round)
    {
        foreach(Ending e in dnrm_endings.endings)
        {
            if (e.round >= round)
            {
                foreach(string str in e.contents)
                {
                    isWaiting = true;
                    chatting.PrintText(str);
                    while (isWaiting)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                    textBox.text = str;
                    isWaiting = true;
                    while (isWaiting)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                }

                // 세이브 파일 지움
                string path = Path.Combine(Application.persistentDataPath, "Data");

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);

                    // Debug.Log("디렉토리 삭제 완료!");
                }
                SceneManager.LoadScene("MainScene");
            }
        }
    }
}
