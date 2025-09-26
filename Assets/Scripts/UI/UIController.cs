using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject pausePanel;
    public BoardManager boardManager;
    public PopupController popup;
    public StarDisplay starDisplay;

    public CanvasGroup levelCompletePanel;
    public RectTransform levelCompleteWindow;
    public CanvasGroup levelFailedPanel;
    public RectTransform levelFailedWindow;


    public void OnResetButtonClicked()
    {
        boardManager.ResetBoard();
        SoundManager.Instance.PlayClick();
    }

    public void UIWin(int stars)
    {
        GameManager.Instance.SetState(GameManager.GameState.Won);
        popup.ShowPopup(levelCompletePanel, levelCompleteWindow);
        SoundManager.Instance.PlayWin();

        starDisplay.Show(stars);

        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);

        SaveSystem.SetLevelStars(currentLevel, stars);// lưu *
        SaveSystem.SetUnlockedLevel(currentLevel + 1); // unlock level tiếp theo
 
    }
    public void UILose()
    {
        GameManager.Instance.SetState(GameManager.GameState.Lost);
        popup.ShowPopup(levelFailedPanel, levelFailedWindow);
        SoundManager.Instance.PlayLose();

    }
    public void PauseButton()
    {
        GameManager.Instance.SetState(GameManager.GameState.Pause);
        pausePanel.SetActive(true);
        SoundManager.Instance.PlayClick();

    }
    public void ResumeButton()
    {
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        pausePanel.SetActive(false);
        SoundManager.Instance.PlayClick();

    }
    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        DOTween.KillAll();
        SoundManager.Instance.PlayClick();
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
        SoundManager.Instance.PlayClick();
    }

    public void LevelMenuButton()
    {
        SceneManager.LoadScene("LevelSelect");
        SoundManager.Instance.PlayClick();
    }
}
