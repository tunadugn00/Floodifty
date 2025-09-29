using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public PopupController popup;
    public GameObject settingPanel;
 
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
        GameManager.Instance.SetState(GameManager.GameState.Pause);
        settingPanel.SetActive(true);
        SoundManager.Instance.PlayClick();
    }
    public void CloseButton()
    {
        settingPanel.SetActive(false);
        SoundManager.Instance.PlayClick();
    }

}
