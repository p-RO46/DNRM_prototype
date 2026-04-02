using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject text;
    private Image uiImage;
    private Material uiMaterial;
    [SerializeField] private float duration = 1.0f;

    public bool isFadeOut = false;
    public bool isFadeIn = false;

    void Awake()
    {
        uiImage = panel.GetComponent<Image>();
        // .material을 호출하면 해당 오브젝트만을 위한 머티리얼 복제본(instanced)이 생성됩니다.
        uiMaterial = uiImage.material;
    }

    public void FadeOut()
    {
        // 0에서 1까지 Shader의 _Alpha 값을 변화시키는 로직 (예시)
        StartCoroutine(FadeOutAnim());
    }

    public void FadeIn()
    {
        // 0에서 1까지 Shader의 _Alpha 값을 변화시키는 로직 (예시)
        StartCoroutine(FadeInAnim());
    }

    private void Update()
    {
        if (isFadeOut)
        {
            FadeOut();
            isFadeOut = false;
        }

        if (isFadeIn)
        {
            FadeIn();
            isFadeIn = false;
        }
    }

    private IEnumerator FadeOutAnim()
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(0, 3, elapsed / duration);
            float value_text = Mathf.Lerp(1, 0, elapsed / duration);

            uiMaterial.SetFloat("_Scroll", value);
            text.GetComponent<CanvasGroup>().alpha = value_text;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(1, 0, elapsed / duration);
            uiMaterial.SetFloat("_Alpha", value);
            yield return null;
        }

        panel.SetActive(false);
    }

    private IEnumerator FadeInAnim()
    {
        panel.SetActive(true);

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(0, 1, elapsed / duration);
            uiMaterial.SetFloat("_Alpha", value);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(3, 0, elapsed / duration);
            float value_text = Mathf.Lerp(0, 1, elapsed / duration);

            uiMaterial.SetFloat("_Scroll", value);
            text.GetComponent<CanvasGroup>().alpha = value_text;

            yield return null;
        }
    }
}
