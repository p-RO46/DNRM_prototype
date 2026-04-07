using UnityEngine;

public class PersistentLLM : MonoBehaviour
{
    private static PersistentLLM instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 인스턴스가 존재하면 새로 생성된 이 객체를 파괴
            Destroy(gameObject);
        }
    }
}