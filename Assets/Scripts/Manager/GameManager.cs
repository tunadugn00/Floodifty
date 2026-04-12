using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Playing, Pause, Won, Lost }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [HideInInspector]
    public bool isEndlessMode = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        isEndlessMode = PlayerPrefs.GetInt("GameMode", 0) == 1;

    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        if (newState == GameState.Pause) Time.timeScale = 0f;
        else Time.timeScale = 1f;
    }

    public bool IsGameActive()
    {
        return CurrentState == GameState.Playing;
    }
}