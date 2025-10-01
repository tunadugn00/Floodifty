using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public PopupController popup;
    public CanvasGroup settingPanel;
    public RectTransform settingWindow;
 
   public void PlayGameButton()
    {
        SceneManager.LoadScene("LevelSelect");
        SoundManager.Instance.PlayClick();
    }

    public void EndlessButton()
    {
        SoundManager.Instance.PlayClick();
    }

    public void SettingButton()
    {
        SoundManager.Instance.PlayClick();
        popup.ShowPopup(settingPanel, settingWindow);
        
    }
    public void CloseButton()
    {
        SoundManager.Instance.PlayClick();
        popup.HidePopup(settingPanel, settingWindow);
    }

}
