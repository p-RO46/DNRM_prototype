using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DataLoad : MonoBehaviour
{
    [Header("Saved Data")]
    public ProgressJson currentProgress = new ProgressJson();
    public int currentRound;

    [Header("Npc Information")]
    [SerializeField] private List<NpcInformation> npcInfo = new List<NpcInformation>();

    [Header("Lamp UI")]
    [SerializeField] private List<PlayerLamp> playerLamp = new List<PlayerLamp>();
    [SerializeField] private Material lampOn;
    [SerializeField] private Material lampOff;
    [SerializeField] private Material lampDown;

    [Header("Fade Controller")]
    [SerializeField] private FadeController fadeController;

    private void Start()
    {
        ReadProgress();
        LampOn();
    }

    public void ReadProgress()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, "Data", "Common", "progress.json");
        string progressJson = "";
        try
        {
            // 파일이 저장될 폴더 경로만 추출
            string directoryPath = Path.GetDirectoryName(fullPath);

            // 폴더가 없으면 생성
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"폴더 생성됨: {directoryPath}");
            }

            // 파일 쓰기
            if (!File.Exists(fullPath))
            {
                string initialProgress = Resources.Load<TextAsset>("Common/progress").text;
                File.WriteAllText(fullPath, initialProgress);
            }

            // 파일 읽기
            progressJson = File.ReadAllText(fullPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"파일 읽기 실패: {e.Message}");
            return;
        }

        Debug.Log(progressJson);
        currentProgress = JsonConvert.DeserializeObject<ProgressJson>(progressJson);
        currentRound = currentProgress.current_round;

        List<VoteResult> result = new List<VoteResult>(currentProgress.number_of_drug_administered);
        for (int i = 0; i < npcInfo.Count; i++)
        {
            npcInfo[i].deathCount = 3 - result[i].count;
        }
    }

    private void LampOn()
    {
        for(int i = 0; i< npcInfo.Count; i++)
        {
            playerLamp[i].lampCount = 3 - npcInfo[i].deathCount;
        }

        foreach(PlayerLamp lamp in playerLamp)
        {
            for(int i = 0; i < lamp.lampCount; i++)
            {
                lamp.lampImages[i].material = lampOn;
            }
        }
    }
}
