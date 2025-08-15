using UnityEngine;


[CreateAssetMenu(fileName ="LevelData", menuName ="Floodify/Level Data")]
public class LevelData : ScriptableObject
{
    [Min(1)] public int rows = 8;
    [Min(1)] public int cols = 10;

    public Tile.TileColor targetColor = Tile.TileColor.Red;
    [Min(1)] public int movesAllowed = 5;

    //Layout
    public Tile.TileColor[] layout;

    public Tile.TileColor Get(int r,int c) => layout[r*cols + c];
    public void Set(int r, int c, Tile.TileColor color) => layout[r*cols + c] = color;

    public void EnsureSize()
    {
        int size = rows * cols;
        if(layout == null || layout.Length != size)
        {
            var newLayout = new Tile.TileColor[size];
            if(layout != null )
            {
                System.Array.Copy(layout, newLayout, Mathf.Min(layout.Length, newLayout.Length));
            }
            layout = newLayout;
        }
    }

    private void OnValidate()
    {
        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(1, cols);
        EnsureSize();
    }
}
