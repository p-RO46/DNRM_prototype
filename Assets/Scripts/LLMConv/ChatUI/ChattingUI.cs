using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TextSpeed
{
    FAST = 1,
    MEDIUM = 2,
    SLOW = 3,
}

public class ChattingUI : MonoBehaviour
{
    [Header("Text Speed")]
    [SerializeField] private TextSpeed textSpeed = TextSpeed.MEDIUM;
    [SerializeField] private float speedArg = 0.05f;

    [SerializeField] private SoundManager SEManager;

    [SerializeField] private Toggle FastToggle;
    [SerializeField] private Toggle MediumToggle;
    [SerializeField] private Toggle SlowToggle;

    [SerializeField] private float indicatorSpeed = 0.5f;

    private TextMeshProUGUI textBox;
    private StringBuilder sb = new StringBuilder();

    private void Start()
    {
        textBox = GetComponent<TextMeshProUGUI>();
    }

    public void PrintText(string message)
    {
        StopAllCoroutines();
        StartCoroutine(PrintTextRoutine(message));
    }

    public void PrintStop()
    {
        StopAllCoroutines();
    }

    // 문자열 받아서 UI에 순차적으로 띄우는거
    private IEnumerator PrintTextRoutine(string message)
    {
        textBox = GetComponent<TextMeshProUGUI>();
        if (textBox == null)
        {
            Debug.LogError("ChatText 컴포넌트가 할당되지 않았습니다!");
            yield break;
        }

        // 2. 출력할 문자열이 비어있는지 확인
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("출력할 텍스트가 비어있습니다.");
            yield break;
        }
        // string fullText = message.Replace("\r\n", "\n");
        sb.Clear();
        for(int i = 0; i < message.Length; i++)
        {
            sb.Append(message[i]);
            textBox.text = sb.ToString();
            SEManager.PlayKeyOneShot();
            yield return new WaitForSeconds((float)textSpeed * speedArg);
        }
    }

    public void ShowLoadingIndicator()
    {
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        int count = 0;
        while (true)
        {
            if (count == 0)
            {
                sb.Clear();
                count = 4;
            }

            textBox.text = sb.ToString();
            sb.Append(".");
            count--;

            yield return new WaitForSeconds(indicatorSpeed);
        }
    }

    public void SetSpeed()
    {
        if (FastToggle.isOn)
        {
            textSpeed = TextSpeed.FAST;
        }
        else if (MediumToggle.isOn)
        {
            textSpeed = TextSpeed.MEDIUM;
        }
        else if (SlowToggle.isOn)
        {
            textSpeed = TextSpeed.SLOW;
        }
    }
}
