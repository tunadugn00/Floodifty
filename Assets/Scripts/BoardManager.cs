using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board size")]
    public int rows = 8;
    public int cols = 8;
    public float cellSize = 1f;

    [Header("Prefabs & art")]
    public GameObject tilePrefab;
    public Sprite[] colorSprites;

    [Header("Game state")]
    public int colorCount = 4;
    private Tile[,] tiles;
    public Tile.TileColor selectedColor;
    public Tile.TileColor goalColor;
    public int movesLeft = 5;

    public UIController uiController;

    void Start()
    {
        GenerateBoard();
        goalColor = Tile.TileColor.Red;
    }

    void GenerateBoard()
    {
        tiles = new Tile[rows, cols];

        float offsetX = -(cols - 1) / 2f * cellSize;
        float offsetY = -(rows - 1) / 2f * cellSize;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(c * cellSize + offsetX, r * cellSize + offsetY, 0);

                GameObject tileObj = Instantiate(tilePrefab, transform);
                tileObj.transform.localPosition = new Vector3(c * cellSize + offsetX, r * cellSize + offsetY, 0);


                Tile tile = tileObj.GetComponent<Tile>();
                tile.Row = r;
                tile.Col = c;

                int colorIndex = Random.Range(0, colorCount);
                tile.Color = (Tile.TileColor)colorIndex;
                tileObj.GetComponent<SpriteRenderer>().sprite = colorSprites[colorIndex];

                tiles[r, c] = tile;
            }
        }
    }

    public void OnColorSelected(Tile.TileColor color)
    {
        selectedColor = color;
        // Debug.Log("Selected: " + color);
    }

    public void OnTileClicked(int r, int c)
    {
        Tile.TileColor originalColor = tiles[r,c].Color;

        if (originalColor == selectedColor) return;

        FloodFill(r, c, originalColor, selectedColor);
        movesLeft--;
        uiController.UpdateMove(movesLeft);

        if (CheckWin())
        {
            uiController.UIWin();
        }
        else if (movesLeft<=0)
        {
            uiController.UILose();
        }
    }

    void FloodFill(int r, int c, Tile.TileColor targetColor, Tile.TileColor replacementColor)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols)
            return;
        if (tiles[r, c].Color != targetColor) return;

        tiles[r, c].Color = replacementColor;
        tiles[r, c].GetComponent<SpriteRenderer>().sprite = colorSprites[(int)replacementColor];

        FloodFill(r + 1, c, targetColor, replacementColor);
        FloodFill(r - 1, c, targetColor, replacementColor);
        FloodFill(r, c + 1, targetColor, replacementColor);
        FloodFill(r, c - 1, targetColor, replacementColor);

    }

    bool CheckWin()
    {
        for(int r =0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (tiles[r, c].Color != goalColor)
                    return false;
            }
        }
        return true;
    }

}
