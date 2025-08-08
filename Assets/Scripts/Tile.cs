using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileColor { Red, Green, Blue, Yellow }

    public int Row;
    public int Col;
    public TileColor Color;
}
