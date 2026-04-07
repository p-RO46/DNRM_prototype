using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RenderTextureRaycaster rtr;
    [SerializeField] private FadeController fadeController;

    [Header("Selectable Object")]
    [SerializeField] private Transform selectable;

    private void Start()
    {
        if (fadeController != null && !fadeController.IsUnityNull())
        {
            fadeController.SetAlpha(0);
        }
    }

    // 마우스를 버튼 위에 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rtr.selectedObject != null) return;

        rtr.selectedObject = selectable;
        MeshRenderer outline = rtr.selectedObject.Find("Outline").GetComponent<MeshRenderer>();
        outline.enabled = true;
    }

    // 마우스가 버튼 밖으로 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (rtr.selectedObject != null)
        {
            MeshRenderer outline = rtr.selectedObject.Find("Outline").GetComponent<MeshRenderer>();
            outline.enabled = false;

            rtr.selectedObject = null;
        }
    }
}
