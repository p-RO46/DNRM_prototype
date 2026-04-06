using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RenderTextureRaycaster : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Camera targetCamera; // 렌더 텍스처를 찍고 있는 카메라
    private RawImage rawImage;

    private Transform selectedObject = null;

    [SerializeField] private ChattingUI chattingUI;
    [SerializeField] private Canvas messageBox;
    [SerializeField] private Canvas selectionButton;
    [SerializeField] private GameObject playerManager;

    [SerializeField] private FadeController fadeController;

    private bool isWaiting = true;

    List<string> chair = new List<string> { "빈 방에 의자가 덩그러니 놓여 있다.", "여기에 앉아서 대화하라는 걸까?" };
    string intercom = "인터폰이 있다. 대화를 시작할까?";

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        playerManager.SetActive(false);
    }
    public async void OnPointerClick(PointerEventData eventData)
    {
        if (selectedObject != null)
        {
            if (selectedObject.name.Equals("Chair"))
            {
                await PrintList(chair);
            }
            else if (selectedObject.name.Equals("Intercom"))
            {
                await WaitForSelect(IntercomClicked);
            }
            else
            {
                selectedObject = null;
                return;
            }

            MeshRenderer outline = selectedObject.Find("Outline").GetComponent<MeshRenderer>();
            outline.enabled = false;

            selectedObject = null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
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
            // Debug.Log("선택된 오브젝트: " + hit.collider.name);

            selectedObject = hit.transform;
            MeshRenderer outline = selectedObject.Find("Outline").GetComponent<MeshRenderer>();
            outline.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selectedObject != null)
        {
            MeshRenderer outline = selectedObject.Find("Outline").GetComponent<MeshRenderer>();
            outline.enabled = false;

            selectedObject = null;
        }
    }

    public void WaitingEnd()
    {
        isWaiting = false;
    }

    private async Awaitable PrintList(List<string> list)
    {
        playerManager.SetActive(true);
        messageBox.enabled = true;
        for(int i = 0; i<list.Count; i++)
        {
            isWaiting = true;
            chattingUI.PrintText(list[i]);
            while (isWaiting)
            {
                await Awaitable.NextFrameAsync();
            }
            if(i == list.Count - 1)
            {
                playerManager.SetActive(false);
                messageBox.enabled = false;
                break;
            }
        }
    }

    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    private bool select = false; // 1:true 2:false

    private async Awaitable WaitForSelect(Selected action)
    {
        isWaiting = true;
        messageBox.enabled = true;
        chattingUI.PrintText(intercom);
        selectionButton.enabled = true;
        while (isWaiting)
        {
            await Awaitable.NextFrameAsync();
        }
        if (action != null)
        {
            action();
        }
    }

    private delegate void Selected();

    private void IntercomClicked()
    {
        if (select)
        {
            StartCoroutine(NextDialogue());
        }
        else
        {
            messageBox.enabled = false;
            selectionButton.enabled = false;
            playerManager.SetActive(false);
        }
    }

    IEnumerator NextDialogue()
    {
        yield return StartCoroutine(fadeController.FadeOutAnim());
        SceneManager.LoadScene("ScreenScene");
    }

    public void Select01()
    {
        select = true;
        isWaiting = false;
    }

    public void Select02()
    {
        select = false;
        isWaiting = false;
    }
}