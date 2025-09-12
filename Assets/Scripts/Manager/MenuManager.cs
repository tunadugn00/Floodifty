using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
   public void PlayGameButton()
    {
        SceneManager.LoadScene("LevelSelect");
        SoundManager.Instance.PlayClick();
    }

    public void SettingButton()
    {
        SoundManager.Instance.PlayClick();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
