using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcInformation", menuName = "Scriptable Objects/NpcInformation")]

public class NpcInformation : ScriptableObject
{
    [Header("Basic Info")]
    public Player player;
    public string npcName = "default";
    private int _deathCount = 3;
    public int deathCount
    {
        get => _deathCount;
        set
        {
            _deathCount = value;
            // 값이 설정될 때 조건 체크
            isAlive = (_deathCount > 0);
        }
    }
    public bool isAlive = true;

    [Header("Graphic")]
    public List<Sprite> expressions = new List<Sprite>();
}
