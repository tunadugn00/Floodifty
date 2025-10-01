using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public BoardManager boardManager;
    public PopupController popup;
    public StarDisplay starDisplay;

    public CanvasGroup levelCompletePanel;
    public RectTransform levelCompleteWindow;
    public CanvasGroup levelFailedPanel;
    public RectTransform levelFailedWindow;
    public CanvasGroup pausedPanel;
    public RectTransform pausedWindow;


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
        popup.ShowPopup(pausedPanel, pausedWindow);
        SoundManager.Instance.PlayClick();

    }

    public void NextLevelButton()
    {
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        int nextLevel = currentLevel + 1;

        if(nextLevel > FindAnyObjectByType<BoardManager>().database.TotalLevels)
        {
            SceneManager.LoadScene("LevelSelect");
        }
        else
        {
            PlayerPrefs.SetInt("SelectedLevel", nextLevel);
            SceneManager.LoadScene("GameScene");
        }   
        SoundManager.Instance.PlayClick();
    }
    public void ResumeButton()
    {
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        popup.HidePopup(pausedPanel, pausedWindow);
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
