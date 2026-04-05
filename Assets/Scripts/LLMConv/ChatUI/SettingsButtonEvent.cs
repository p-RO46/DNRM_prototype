using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsButtonEvent : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private GameObject buttons;
    private bool isShowed = false;

    [Header("UI Contents")]
    [SerializeField] private Canvas settingUI;
    [SerializeField] private Canvas manualUI;
    [SerializeField] private Canvas logUI;

    [Header("Player Manager")]
    [SerializeField] private GameObject playerManager;

    #region Button Event
    public void MainButtonClicked()
    {
        isShowed = !isShowed;
        buttons.SetActive(isShowed);
    }

    public void SettingButtonClicked()
    {
        playerManager.SetActive(false);
        settingUI.enabled = true;
    }

    public void ManualButtonClicked()
    {
        playerManager.SetActive(false);
        manualUI.enabled = true;
    }

    public void LogButtonClicked()
    {
        playerManager.SetActive(false);
        logUI.enabled = true;
    }
    #endregion

    #region Close Button
    public void SettingCloseClicked()
    {
        settingUI.enabled = false;
        playerManager.SetActive(true);
    }

    public void ManualCloseClicked()
    {
        manualUI.enabled = false;
        playerManager.SetActive(true);
    }

    public void LogCloseClicked()
    {
        logUI.enabled = false;
        playerManager.SetActive(true);
    }
    #endregion
}
