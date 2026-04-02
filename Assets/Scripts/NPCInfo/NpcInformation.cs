using UnityEngine;

[CreateAssetMenu(fileName = "NpcInformation", menuName = "Scriptable Objects/NpcInformation")]
public class NpcInformation : ScriptableObject
{
    [Header("Basic Info")]
    public Player player;
    public string npcName = "default";
    public int deathCount = 3;
    public bool isAlive = true;
    public TextAsset persona;

    [Header("Graphic")]
    public Material image;
}
