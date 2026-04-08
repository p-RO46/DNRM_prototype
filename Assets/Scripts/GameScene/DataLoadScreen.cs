using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class DataLoadScreen : DataLoad
{
    [Header("Dialogue Manager")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Canvas dialogueUI;
    [SerializeField] private SoundManager BGMManager;
    [SerializeField] private VoteInteraction voteInteraction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        ReadProgress();
        LampOn();

        dialogueUI.enabled = false;

        BGMManager.ChangeClip(AC.BGM_EMPTY);
        voteInteraction.candidates.Clear();

        foreach (VoteResult result in currentProgress.number_of_drug_administered)
        {
            Debug.Log($"{result.name}: {result.count}");
            if (result.count < 3)
            {
                if(Enum.TryParse(typeof(Player), result.name, false, out object p))
                {
                    users.Add((Player)p);
                    voteInteraction.candidates.Add(result.name);
                }
            }
        }

        int n = users.Count;

        while (n > 1) // 랜덤한 대화 순서. 이래도 되나 싶긴하다
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1); // 0 <= k <= n

            // 요소 교환 (Swap)
            Player value = users[k];
            users[k] = users[n];
            users[n] = value;
        }

        await dialogueManager.GenerateSchedule(users);
    }

}
