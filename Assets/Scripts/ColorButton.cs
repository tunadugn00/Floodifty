using UnityEngine;

public class ColorButton : MonoBehaviour
{
    public Tile.TileColor color;

    public void OnClick()
    {
        FindFirstObjectByType<BoardManager>().OnColorSelected(color);
    }
}
