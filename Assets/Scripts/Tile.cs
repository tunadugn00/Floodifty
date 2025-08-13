using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileColor { Red, Green, Blue, Yellow }

    public int Row;
    public int Col;
    public TileColor Color;

    public void OnMouseDown()
    {
        Debug.Log($"Tile clicked at {Row},{Col} with color {Color}");
        FindFirstObjectByType<BoardManager>().OnTileClicked(Row, Col);
    }
}
