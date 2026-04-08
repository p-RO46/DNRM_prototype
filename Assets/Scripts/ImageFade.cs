using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageFade : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    void Start()
    {
        GetComponent<Image>().color = Color.black;

        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(1, 0, elapsed / duration);
            GetComponent<Image>().color = new Color(0, 0, 0, value);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        GetComponent<Image>().enabled = false;
    }
}
