using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum AC
{
    BGM_LONG,
    BGM_SHORT,
    BGM_EMPTY,
    SE_KEY01,
    SE_KEY02,
    SE_NOISE01_LONG,
    SE_NOISE02_LONG,
    SE_NOISE01_SHORT,
    SE_NOISE02_SHORT,
}

public class SoundManager : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private Slider slider;
    [SerializeField] private List<SerializableData<AC, AudioClip>> sounds = new List<SerializableData<AC, AudioClip>>();
    private Dictionary<AC, AudioClip> data = new Dictionary<AC, AudioClip>();

    public void ChangeVolume()
    {
        if (slider != null)
        {
            GetComponent<AudioSource>().volume = slider.value;
        }
    }

    public void ChangeClip(AC name)
    {
        GetComponent<AudioSource>().clip = data[name];
        GetComponent<AudioSource>().Play();
    }

    public void PlayClipOneShot(AC name)
    {
        GetComponent<AudioSource>().PlayOneShot(data[name]);
    }

    public void PlayKeyOneShot()
    {
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            GetComponent<AudioSource>().PlayOneShot(data[AC.SE_KEY01]);
        }
        else
        {
            GetComponent<AudioSource>().PlayOneShot(data[AC.SE_KEY02]);
        }
    }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        data.Clear();
        foreach (var sound in sounds)
        {
            // 중복 키 방지 로직
            if (!data.ContainsKey(sound.key))
            {
                data.Add(sound.key, sound.value);
            }
            else
            {
                Debug.LogWarning($"중복된 키가 발견되었습니다: {sound.key}");
            }
        }
    }
}
