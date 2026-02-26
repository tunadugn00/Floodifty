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

    // ADS : đăng kí sự kiên trả thưởng
    private void  OnEnable()
    {
        //khi AdsManager gọi trả thưởng, gọi hàm GiveReward
        AdsManager.OnUserEarnedReward += GiveReward;
    }
    private void OnDisable()
    {
        //hủy đăng kí khi tắt object
        AdsManager.OnUserEarnedReward -= GiveReward;
    }

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
 
        //hiển thị ADS xen kẽ, 50% tỉ lệ
        if(Random.Range(0,2) == 0)
        {
            AdsManager.Instance.ShowInterstitialAd();
        }
    }
    public void UILose()
    {
        GameManager.Instance.SetState(GameManager.GameState.Lost);
        popup.ShowPopup(levelFailedPanel, levelFailedWindow);
        SoundManager.Instance.PlayLose();

    }

    public void AddMoveButton()
    {
        //Hiển thị ADS có thưởng
        AdsManager.Instance.ShowRewardedAd();
        SoundManager.Instance.PlayClick();
    }
    //gọi tự động bới ADsManager khi xem QC thành công
    private void GiveReward()
    {
        boardManager.AddMove();
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
        Time.timeScale = 1f;
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        DOTween.KillAll();
        SoundManager.Instance.PlayClick();
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        SceneTransitionManager.Instance.LoadSceneWithAni("MainMenu");
        SoundManager.Instance.PlayClick();
    }

    public void LevelMenuButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance.SetState(GameManager.GameState.Playing);

        SceneTransitionManager.Instance.LoadSceneWithAni("LevelSelect");
        SoundManager.Instance.PlayClick();
    }
}
