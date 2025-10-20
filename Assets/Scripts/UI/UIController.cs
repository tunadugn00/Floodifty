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

    public void AddMoveButton()
    {
        boardManager.AddMove();
        SoundManager.Instance.PlayClick();
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
            SceneTransitionManager.Instance.LoadSceneWithAni("LevelSelect");
        }
        else
        {
            PlayerPrefs.SetInt("SelectedLevel", nextLevel);
            SceneTransitionManager.Instance.LoadSceneWithAni("GameScene");
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
        SceneTransitionManager.Instance.LoadSceneWithAni("MainMenu");
        SoundManager.Instance.PlayClick();
    }

    public void LevelMenuButton()
    {
        SceneTransitionManager.Instance.LoadSceneWithAni("LevelSelect");
        SoundManager.Instance.PlayClick();
    }
}
