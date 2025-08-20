using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI movesValue;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI goalValue;

    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject pausePanel;
    public BoardManager boardManager;

    public void SetMove(int moves)
    {
        movesText.text = "Moves Left:";
        movesValue.text = moves.ToString();
    }
    public void SetGoal(Tile.TileColor color)
    {
        goalText.text = "Fill Everything With";
        goalValue.text = color.ToString();
        goalValue.color = GetColorForTile(color);
    }
    private Color GetColorForTile(Tile.TileColor tileColor)
    {
        switch(tileColor)
        {
            case Tile.TileColor.Red: return Color.red;
            case Tile.TileColor.Green: return Color.green;
            case Tile.TileColor.Blue: return Color.blue;
            case Tile.TileColor.Yellow: return Color.yellow;
        }
        return Color.white;
    }
    public void OnResetButtonClicked()
    {
        boardManager.ResetBoard();
    }

    public void UIWin()
    {
        GameManager.Instance.SetState(GameManager.GameState.Won);
        winPanel.SetActive(true);
    }
    public void UILose()
    {
        GameManager.Instance.SetState(GameManager.GameState.Lost);
        losePanel.SetActive(true);
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
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
