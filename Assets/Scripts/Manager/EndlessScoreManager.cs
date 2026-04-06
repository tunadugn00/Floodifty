using UnityEngine;
// BaseScore × StarMultiplier × StageMultiplier + BonusMoves

public class EndlessScoreManager : MonoBehaviour
{
    public static EndlessScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int baseScore = 1000;
    [SerializeField] private int bonusPerRemainingMove = 50;

    private int sessionScore = 0;
    private const string HIGHSCORE_KEY = "EndlessHighScore";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int AddScore(int stars, int stage, int movesLeft)
    {
        float starMultiplier = stars == 3 ? 1.5f : stars == 2 ? 1.0f : 0.5f;
        float stageMultiplier = 1f + (stage * 0.1f);
        int bonus = movesLeft * bonusPerRemainingMove;

        int earned = Mathf.RoundToInt(baseScore * starMultiplier * stageMultiplier) + bonus;
        sessionScore += earned;

        if (sessionScore > GetHighScore())
        {
            PlayerPrefs.SetInt(HIGHSCORE_KEY, sessionScore);
            PlayerPrefs.Save();
        }

        if (LeaderboardManager.Instance != null)
            _ = LeaderboardManager.Instance.SubmitScoreAsync(sessionScore);
        return earned;
    }

    public void ResetSession()
    {
        sessionScore = 0;
    }

    public int GetSessionScore() => sessionScore;
    public int GetHighScore() => PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
}