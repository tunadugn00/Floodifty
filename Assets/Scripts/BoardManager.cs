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
    public int movesLeft = 20;
    public int colorCount = 4;

    private Tile[,] tiles;

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        tiles = new Tile[rows, cols];

        // Tính offset để bảng nằm giữa
        float offsetX = -(cols - 1) / 2f * cellSize;
        float offsetY = -(rows - 1) / 2f * cellSize;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(c * cellSize + offsetX, r * cellSize + offsetY, 0);

                GameObject tileObj = Instantiate(tilePrefab, transform); // spawn làm con của BoardManager ngay từ đầu
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
        Debug.Log("Màu" + color);
    }

}
