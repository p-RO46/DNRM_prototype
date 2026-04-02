using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RenderTextureRaycaster : MonoBehaviour, IPointerClickHandler
{
    public Camera targetCamera; // 렌더 텍스처를 찍고 있는 카메라
    private RawImage rawImage;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Raw Image 내에서의 클릭 좌표를 0~1 사이의 값(UV)으로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Rect 좌표를 (0,0)~(1,1) 범위의 Viewport 좌표로 정규화
        Rect rect = rawImage.rectTransform.rect;
        float x = (localPoint.x - rect.x) / rect.width;
        float y = (localPoint.y - rect.y) / rect.height;

        Vector3 viewportPoint = new Vector3(x, y, 0);

        // 2. 변환된 좌표를 사용하여 해당 카메라에서 레이 발사
        Ray ray = targetCamera.ViewportPointToRay(viewportPoint);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("선택된 오브젝트: " + hit.collider.name);

            // 여기서 오브젝트 선택 로직 수행 (예: 색상 변경, 정보창 띄우기 등)
            // hit.collider.GetComponent<YourScript>()?.OnSelected();
        }
    }
}