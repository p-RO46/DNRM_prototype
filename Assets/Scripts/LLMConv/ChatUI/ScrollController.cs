using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Image handle;

    void Update()
    {
        if(GetComponent<RectTransform>().rect.height <= 100f)
        {
            scrollRect.vertical = false;
            handle.enabled = false;
        }
        else
        {
            scrollRect.vertical = true;
            handle.enabled = true;
        }
    }
}
