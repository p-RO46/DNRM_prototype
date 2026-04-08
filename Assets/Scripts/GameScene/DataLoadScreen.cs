using UnityEngine;
using System.Collections.Generic;

public class DataLoadScreen : DataLoad
{
    [Header("Dialogue Manager")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Canvas dialogueUI;
    [SerializeField] private SoundManager BGMManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        ReadProgress();
        LampOn();

        dialogueUI.enabled = false;

        BGMManager.ChangeClip(AC.BGM_EMPTY);

        List<Player> users = new List<Player>();
        foreach (NpcInformation info in npcInfo)
        {
            if (info.isAlive)
            {
                users.Add(info.player);
            }
        }

        int n = users.Count;

        while (n > 1) // 랜덤한 대화 순서. 이래도 되나 싶긴하다
        {
            n--;
            int k = Random.Range(0, n + 1); // 0 <= k <= n

            // 요소 교환 (Swap)
            Player value = users[k];
            users[k] = users[n];
            users[n] = value;
        }

        await dialogueManager.GenerateSchedule(users);
    }

}
