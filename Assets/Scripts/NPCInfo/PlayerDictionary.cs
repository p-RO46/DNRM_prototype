using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableData<T1, T2>
{
    public T1 key;
    public T2 value;
}

[CreateAssetMenu(fileName = "PlayerDictionary", menuName = "Scriptable Objects/PlayerDictionary")]
public class PlayerDictionary : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private List<SerializableData<Player, NpcInformation>> dataView = new List<SerializableData<Player, NpcInformation>> ();
    private Dictionary<Player, NpcInformation> data = new Dictionary<Player, NpcInformation>();

    // 딕셔너리 접근용 프로퍼티
    public Dictionary<Player, NpcInformation> dictionary => data;

    // --- ISerializationCallbackReceiver 구현 ---

    // 데이터를 저장하기 직전에 호출 (보통 비워둡니다)
    public void OnBeforeSerialize() { }

    // 데이터를 불러온 직후(인스펙터 수정 포함) 호출
    public void OnAfterDeserialize()
    {
        data.Clear();
        foreach (var entry in dataView)
        {
            // 중복 키 방지 로직
            if (!data.ContainsKey(entry.key))
            {
                data.Add(entry.key, entry.value);
            }
            else
            {
                Debug.LogWarning($"중복된 키가 발견되었습니다: {entry.key}");
            }
        }
    }
}
