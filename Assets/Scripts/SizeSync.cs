using UnityEngine;
using UnityEngine.EventSystems;

// UIBehaviour를 상속받아야 크기 변경 이벤트를 수신할 수 있습니다.
public class SizeSync : UIBehaviour
{
    public RectTransform target; // 크기를 복사해올 대상 (부모가 아니어도 됨)
    private RectTransform myRect;

    protected override void Awake()
    {
        base.Awake();
        myRect = GetComponent<RectTransform>();
    }

    // 이 함수는 유니티 시스템에 의해 크기가 변할 때만 자동으로 호출됩니다.
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SyncSize();
    }

#if UNITY_EDITOR
    // 에디터에서 타겟을 할당하거나 바꿨을 때도 즉시 반영되도록 합니다.
    protected override void OnValidate()
    {
        base.OnValidate();
        SyncSize();
    }
#endif

    private void SyncSize()
    {
        if (target != null && myRect != null)
        {
            // 대상의 크기를 그대로 복사합니다.
            // myRect.sizeDelta = target.sizeDelta;
            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target.rect.height);

            // 만약 앵커 설정까지 다르다면 rect 전체를 맞출 수도 있습니다.
            // myRect.anchorMin = target.anchorMin;
            // myRect.anchorMax = target.anchorMax;
        }
    }
}