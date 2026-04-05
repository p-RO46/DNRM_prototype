using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Image uiImage;
    [SerializeField] private GameObject text;
    private Material uiMaterial;
    [SerializeField] private float duration = 1.0f;

    public bool isLoading = false;

    void Awake()
    {
        uiMaterial = uiImage.material;
    }

    public void FadeOut()
    {
        if (isLoading)
        {
            isLoading = false;
            StopAllCoroutines();
            StartCoroutine(FadeOutAnim());
        }
    }

    public void FadeIn()
    {
        if (!isLoading)
        {
            isLoading = true;
            StopAllCoroutines();
            StartCoroutine(FadeInAnim());
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

        GetComponent<Canvas>().enabled = false;
    }

    private IEnumerator FadeInAnim()
    {
        GetComponent<Canvas>().enabled = true;

        float elapsed = 0;
        while (elapsed < duration)
        {
            if (uiMaterial.GetFloat("_Alpha") >= 1) break;

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
