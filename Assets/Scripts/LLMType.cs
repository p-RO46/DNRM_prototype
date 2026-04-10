using UnityEngine;

public class LLMType : MonoBehaviour
{
    private static LLMType instance;

    public bool useOpenAI = false;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLLMType()
    {

    }
}
