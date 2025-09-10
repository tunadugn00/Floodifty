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

    public CanvasGroup levelCompletePanel;
    public RectTransform levelCompleteWindow;
    public CanvasGroup levelFailedPanel;
    public RectTransform levelFailedWindow;


    public void OnResetButtonClicked()
    {
        boardManager.ResetBoard();
        SoundManager.Instance.PlayClick();
    }

    public void UIWin()
    {
        GameManager.Instance.SetState(GameManager.GameState.Won);
        popup.ShowPopup(levelCompletePanel, levelCompleteWindow);
        SoundManager.Instance.PlayWin();
 
    }
    public void UILose()
    {
        GameManager.Instance.SetState(GameManager.GameState.Lost);
        popup.ShowPopup(levelFailedPanel, levelFailedWindow);
        SoundManager.Instance.PlayLose();

    }
    public void UIPause()
    {
        GameManager.Instance.SetState(GameManager.GameState.Pause);
        pausePanel.SetActive(true);
        SoundManager.Instance.PlayClick();

    }
    public void Resume()
    {
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        pausePanel.SetActive(false);
        SoundManager.Instance.PlayClick();

    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        DOTween.KillAll();
        SoundManager.Instance.PlayClick();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SoundManager.Instance.PlayClick();
    }
}
