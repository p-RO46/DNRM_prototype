using System.IO;
using UnityEngine;

public static class PlayTimer
{
    public static float TotalPlayTime = 0f; // static으로 선언

    public static void SavePlayTime()
    {
        // 저장 로직 예시
        string path = Path.Combine(Application.persistentDataPath, "playTime.txt");
        File.WriteAllText(path, $"{TotalPlayTime}");
    }
}