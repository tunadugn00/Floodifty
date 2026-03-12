using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public PopupController popup;
    public CanvasGroup settingPanel;
    public RectTransform settingWindow;
 
   public void PlayGameButton()
    {
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.Save();

        SoundManager.Instance.PlayClick();
        SceneTransitionManager.Instance.LoadSceneWithAni("LevelSelect");
    }

    public void EndlessButton()
    {
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerPrefs.SetInt("EndlessStage", 1); // Reset lại màn 1 của Endless
        PlayerPrefs.Save();

        SoundManager.Instance.PlayClick();
        SceneTransitionManager.Instance.LoadSceneWithAni("GameScene");
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
