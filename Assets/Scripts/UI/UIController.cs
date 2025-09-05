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
    }

    public void UIWin()
    {
        GameManager.Instance.SetState(GameManager.GameState.Won);
        popup.ShowPopup(levelCompletePanel, levelCompleteWindow);
 
    }
    public void UILose()
    {
        GameManager.Instance.SetState(GameManager.GameState.Lost);
        popup.ShowPopup(levelFailedPanel, levelFailedWindow);
     
    }
    public void UIPause()
    {
        GameManager.Instance.SetState(GameManager.GameState.Pause);
        pausePanel.SetActive(true);

    }
    public void Resume()
    {
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        pausePanel.SetActive(false);

    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        DOTween.KillAll();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
