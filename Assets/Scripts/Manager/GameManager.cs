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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // DỜI LÊN ĐÂY! Đọc Mode ngay lập tức khi Scene vừa load lên
        // 1 = Endless, 0 = Normal
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