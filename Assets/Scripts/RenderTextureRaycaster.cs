using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RenderTextureRaycaster : MonoBehaviour
{
    public Camera targetCamera; // 렌더 텍스처를 찍고 있는 카메라
    private RawImage rawImage;

    public Transform selectedObject = null;

    [SerializeField] private ChattingUI chattingUI;
    [SerializeField] private Canvas messageBox;
    [SerializeField] private Canvas selectionButton;
    [SerializeField] private GameObject playerManager;

    [SerializeField] private GraphicRaycaster screenTouch;

    [SerializeField] private FadeController fadeController;

    private bool isWaiting = true;

    List<string> chair = new List<string> { "빈 방에 의자가 덩그러니 놓여 있다.", "여기에 앉아서 대화하라는 걸까?" };
    string intercom = "인터폰이 있다. 대화를 시작할까?";

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        playerManager.SetActive(false);
    }

    public async void OnButtonClick()
    {
        screenTouch.enabled = false;
        if (selectedObject != null && !selectedObject.IsUnityNull())
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
                screenTouch.enabled = true;
                return;
            }

            if (selectedObject != null && !selectedObject.IsUnityNull())
            {
                MeshRenderer outline = selectedObject.Find("Outline").GetComponent<MeshRenderer>();
                outline.enabled = false;

                selectedObject = null;
            }
        }
        screenTouch.enabled = true;
    }

    public void WaitingEnd()
    {
        isWaiting = false;
    }

    private async Awaitable PrintList(List<string> list)
    {
        playerManager.SetActive(true);
        messageBox.enabled = true;
        for(int i = 0; i < list.Count; i++)
        {
            isWaiting = true;
            chattingUI.PrintText(list[i]);
            while (isWaiting)
            {
                await Awaitable.NextFrameAsync();
            }
        }

        isWaiting = true;
        while (isWaiting)
        {
            await Awaitable.NextFrameAsync();
        }
        playerManager.SetActive(false);
        messageBox.enabled = false;
        return;
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
        fadeController.GetComponent<Canvas>().enabled = true;
        yield return StartCoroutine(fadeController.FadeToBlack());
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