using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Playing, Pause, Won, Lost}
    public GameState CurrentState {  get; private set; } = GameState.Playing;
    public GameState state;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        state = CurrentState;
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;

        if(newState == GameState.Pause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public bool IsGameActive()
    {
        return CurrentState == GameState.Playing;
    }
}
