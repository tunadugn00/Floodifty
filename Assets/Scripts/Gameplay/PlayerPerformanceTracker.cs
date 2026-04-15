using UnityEngine;

public class PlayerPerformanceTracker : MonoBehaviour
{
    public static PlayerPerformanceTracker Instance { get; private set; }

    const string KEY_TOTAL_STAGES = "PT_TotalStages";
    const string KEY_WIN_STAGES = "PT_WinStages";
    const string KEY_TOTAL_MOVES_LEFT = "PT_TotalMovesLeft";

    private int totalStages;
    private int winStages;
    private int totalMovesLeft;


    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        totalStages = PlayerPrefs.GetInt(KEY_TOTAL_STAGES, 0);
        winStages = PlayerPrefs.GetInt(KEY_WIN_STAGES, 0);
        totalMovesLeft = PlayerPrefs.GetInt(KEY_TOTAL_MOVES_LEFT, 0);
    }

    void Save()
    {
        PlayerPrefs.SetInt(KEY_TOTAL_STAGES, totalStages);
        PlayerPrefs.SetInt(KEY_WIN_STAGES, winStages);
        PlayerPrefs.SetInt(KEY_TOTAL_MOVES_LEFT, totalMovesLeft);
        PlayerPrefs.Save();
    }

    public void RecordStageResult(bool won, int movesLeft)
    {
        totalStages++;
        if (won) winStages++;
        if (won) totalMovesLeft += movesLeft;
        Save();
    }

    public float GetWinRate()
    {
        if (totalStages == 0) return 0.5f;
        return (float)winStages / totalStages;
    }

    public float GetAvgMovesLeft()
    {
        if (winStages == 0) return 0f;
        return (float)totalMovesLeft / winStages;
    }


    public enum PlayerTier { Beginner, Normal, Expert }

    public PlayerTier GetTier()
    {
        if (totalStages < 3) return PlayerTier.Normal;

        float winRate = GetWinRate();
        float avgMovesLeft = GetAvgMovesLeft();

        if (winRate >= 0.75f && avgMovesLeft >= 1.5f)
            return PlayerTier.Expert;

        if (winRate >= 0.4f)
            return PlayerTier.Normal;

        return PlayerTier.Beginner;
    }

    public void ResetStats()
    {
        totalStages = winStages = totalMovesLeft = 0;
        Save();
    }
    
}