using TMPro;
using UnityEngine;
using DG.Tweening;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI movesValue;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI goalValue;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI levelValue;

    [Header("ITEM")]
    public TextMeshProUGUI hintCountText;
    public TextMeshProUGUI hammerCountText;
    public TextMeshProUGUI bombCountText;
    private void Start()
    {
        UpdateItemCounts();
    }
    public void UpdateItemCounts()
    {
        if (ItemManager.Instance == null) return;
        if (hintCountText != null)
        {
            int count = ItemManager.Instance.GetHintCount();
            hintCountText.text = count > 0 ? count.ToString() : "+";
        }
        if (hammerCountText != null)
        {
            int count = ItemManager.Instance.GetHammerCount();
            hammerCountText.text = count > 0 ? count.ToString() : "+";
        }
        if (bombCountText != null)
        {
            int count = ItemManager.Instance.GetColorBombCount();
            bombCountText.text = count > 0 ? count.ToString() : "+";
        }
    }
    public void SetLevel(int level, bool isEndless)
    {
        if (levelLabel != null)
            levelLabel.text = isEndless ? "Stage" : "Level";

        if (levelValue != null)
            levelValue.text = level.ToString();
    }
    public void SetMove(int moves)
    {
        movesText.text = "Moves Left:";
        movesValue.text = moves.ToString();

        movesValue.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.3f, 1, 0.5f);
    }
    public void SetGoal(Tile.TileColor color)
    {
        goalText.text = "Target:";
        goalValue.text = color.ToString();
        goalValue.color = GetColorForTile(color);
    }
    private Color GetColorForTile(Tile.TileColor tileColor)
    {
        switch (tileColor)
        {
            case Tile.TileColor.Red: return Color.red;
            case Tile.TileColor.Green: return Color.green;
            case Tile.TileColor.Blue: return Color.blue;
            case Tile.TileColor.Yellow: return Color.yellow;
        }
        return Color.white;
    }
}
