using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("Win Panel - Coins (Normal mode)")]
    public GameObject coinGroup;             
    public TextMeshProUGUI coinMainText;
    public TextMeshProUGUI coinBonusText;

    [Header("Win Panel - Score (Endless mode)")]
    public GameObject scoreGroup;             
    public TextMeshProUGUI scoreMainText;
    public TextMeshProUGUI scoreBonusText;

    [Header("Lose Panel - Endless")]
    public GameObject finalScoreGroup;
    public TextMeshProUGUI finalScoreText;    

    [Header("Counter Settings")]
    [SerializeField] private float countDuration = 1.2f;
    [SerializeField] private float bonusDelay = 0.4f;    
    [SerializeField] private float bonusFadeDelay = 0.8f; 

    private void OnEnable()
    {
        AdsManager.OnUserEarnedReward += GiveReward;
    }
    private void OnDisable()
    {
        AdsManager.OnUserEarnedReward -= GiveReward;
    }

    public void OnResetButtonClicked()
    {
        boardManager.ResetBoard();
        SoundManager.Instance.PlayClick();
    }

    public void UIWin(int stars, int movesLeft = 0)
    {
        GameManager.Instance.SetState(GameManager.GameState.Won);
        popup.ShowPopup(levelCompletePanel, levelCompleteWindow);
        SoundManager.Instance.PlayWin();
        starDisplay.Show(stars);

        if (GameManager.Instance.isEndlessMode)
        {
            coinGroup?.SetActive(false);
            scoreGroup?.SetActive(true);

            int stage = PlayerPrefs.GetInt("EndlessStage", 1);
            int earned = EndlessScoreManager.Instance.AddScore(stars, stage, movesLeft);
            int totalBefore = EndlessScoreManager.Instance.GetSessionScore() - earned;
            int totalAfter = EndlessScoreManager.Instance.GetSessionScore();

            StartCoroutine(AnimateCounter(scoreMainText, totalBefore, totalAfter, scoreBonusText, earned));
        }
        else
        {
            scoreGroup?.SetActive(false);
            coinGroup?.SetActive(true);

            int coins = RewardManager.Instance.GiveReward(stars);
            int coinsBefore = CurrencyManager.Instance.GetCoins() - coins;
            StartCoroutine(AnimateCounter(coinMainText, coinsBefore, coinsBefore + coins, coinBonusText, coins));

            int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
            SaveSystem.SetLevelStars(currentLevel, stars);
            SaveSystem.SetUnlockedLevel(currentLevel + 1);
        }

        if (Random.Range(0, 2) == 0)
            AdsManager.Instance.ShowInterstitialAd();
    }

    private IEnumerator AnimateCounter(TextMeshProUGUI mainText, int from, int to, TextMeshProUGUI bonusText, int bonus)
    {
        if (mainText == null) yield break;
        mainText.text = from.ToString("N0");

        if (bonusText != null)
        {
            bonusText.text = $"+{bonus}";
            bonusText.alpha = 1f;
            bonusText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(bonusDelay);

        float elapsed = 0f;
        while (elapsed < countDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / countDuration);
            float eased = DOVirtual.EasedValue(0f, 1f, t, Ease.OutQuad);
            int current = Mathf.RoundToInt(Mathf.Lerp(from, to, eased));
            mainText.text = current.ToString("N0");
            yield return null;
        }
        mainText.text = to.ToString("N0");

        yield return new WaitForSecondsRealtime(bonusFadeDelay);
        if (bonusText != null)
        {
            DOTween.To(() => bonusText.alpha, x => bonusText.alpha = x, 0f, 0.4f)
                .SetUpdate(true)
                .OnComplete(() => bonusText.gameObject.SetActive(false));
        }
    }
    public void UILose()
    {
        GameManager.Instance.SetState(GameManager.GameState.Lost);
        popup.ShowPopup(levelFailedPanel, levelFailedWindow);
        SoundManager.Instance.PlayLose();

        if (GameManager.Instance.isEndlessMode && EndlessScoreManager.Instance != null)
        {
            int final = EndlessScoreManager.Instance.GetSessionScore();
            finalScoreGroup?.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = $"Score: {final:N0}";
            EndlessScoreManager.Instance.ResetSession();
        }
        else
        {
            finalScoreGroup?.SetActive(false);
        }
    }

    public void AddMoveButton()
    {
        AdsManager.Instance.ShowRewardedAd();
        SoundManager.Instance.PlayClick();
    }
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
        if (GameManager.Instance.isEndlessMode)
        {
            // 1. Tăng stage hiện tại
            int currentStage = PlayerPrefs.GetInt("EndlessStage", 1);
            PlayerPrefs.SetInt("EndlessStage", currentStage + 1);

            // 2. Load lại Scene (Trong hàm Start() của BoardManager, ông gọi EndlessLevelGenerator.GenerateLevel để vẽ bảng)
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
            int nextLevel = currentLevel + 1;

            if (nextLevel > FindAnyObjectByType<BoardManager>().database.TotalLevels)
            {
                SceneTransitionManager.Instance.LoadSceneWithAni("LevelSelect");
            }
            else
            {
                PlayerPrefs.SetInt("SelectedLevel", nextLevel);
                SceneTransitionManager.Instance.LoadSceneWithAni("GameScene");
            }
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