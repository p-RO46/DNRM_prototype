using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public enum TextSpeed
{
    FAST = 1,
    MEDIUM = 2,
    SLOW = 3,
}

public class ChattingUI : MonoBehaviour
{
    [Header("Text Speed")]
    public TextSpeed textSpeed = TextSpeed.MEDIUM;
    public float speedArg = 0.05f;
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

    // 문자열 받아서 UI에 순차적으로 띄우는거
    private IEnumerator PrintTextRoutine(string message)
    {
        string fullText = message.Replace("\r\n", "\n");
        sb.Clear();
        for(int i = 0; i < fullText.Length; i++)
        {
            textBox.text = sb.ToString();
            sb.Append(fullText[i]);
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
}
