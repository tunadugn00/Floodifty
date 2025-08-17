using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI movesValue;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI goalValue;

    public GameObject winPanel;
    public GameObject losePanel;

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
    public void UIWin()
    {
        winPanel.SetActive(true);
    }
    public void UILose()
    {
        losePanel.SetActive(true);
    }
}
