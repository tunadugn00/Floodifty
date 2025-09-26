using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileColor { Red, Green, Blue, Yellow, Rock }

    public int Row;
    public int Col;
    public TileColor Color;
    public bool isRock => Color == TileColor.Rock;

    public void OnMouseDown()
    {
        if (!GameManager.Instance.IsGameActive()) return;
        FindFirstObjectByType<BoardManager>().OnTileClicked(Row, Col);
    }
}
