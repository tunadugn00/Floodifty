using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public float cellSize = 1f;
    public GameObject tilePrefab;
    public Sprite[] colorSprites;
    public Sprite rockSprite;
    public HUDController hudController;
    public UIController uiController;
    public FloodFillAnimator floodAnimator;

    [Header("Level Database")]
    public LevelData currentLevel;
    public LevelDatabase database;

    private Tile[,] tiles;
    private int rows, cols;
    private Tile.TileColor goalColor;
    private int movesLeft;
    private int actualMovesUsed;
    private Tile.TileColor selectedColor;

    void Start()
    {
        int selected = PlayerPrefs.GetInt("SelectedLevel", 1);
        currentLevel = database.GetLevel(selected);
        LoadLevel(currentLevel);

      
    }

    // Load level from LevelData
    public void LoadLevel(LevelData data)
    {
        rows = data.rows;
        cols = data.cols;
        goalColor = data.targetColor;
        movesLeft = data.movesAllowed;
        actualMovesUsed = 0;

        hudController?.SetMove(movesLeft);
        hudController?.SetGoal(goalColor);

        GenerateBoard(data);
        floodAnimator.Init(tiles, rows, cols, colorSprites);

        StartCoroutine(floodAnimator.AnimateBoardSpawn());
    }

    // spawn board from LevelData
    private void GenerateBoard(LevelData data)
    {
        // Clear old board 
        foreach (Transform t in transform) Destroy(t.gameObject);

        tiles = new Tile[rows, cols];
        float offsetX = -(cols - 1) / 2f * cellSize;
        float offsetY = -(rows - 1) / 2f * cellSize;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject tileObj = Instantiate(tilePrefab, transform);
                tileObj.transform.localPosition = new Vector3(c * cellSize + offsetX, r * cellSize + offsetY, 0);
                tileObj.transform.localScale = Vector3.zero;
                
                Tile tile = tileObj.GetComponent<Tile>();
                tile.Row = r;
                tile.Col = c;

                var color = data.Get(r, c);
                tile.Color = color;

                var sr = tileObj.GetComponent<SpriteRenderer>();
                if (tile.isRock)
                {
                    sr.sprite = rockSprite;
                }
                else
                {
                    sr.sprite = colorSprites[(int)color];
                }

                tiles[r, c] = tile;
            }
        }
    }
    public void ResetBoard()
    {
        movesLeft = currentLevel.movesAllowed;
        actualMovesUsed = 0;
        hudController?.SetMove(movesLeft);

        goalColor = currentLevel.targetColor;
        hudController?.SetGoal(goalColor);

        GenerateBoard(currentLevel);

        // Re-init lại animator với tile mới
        floodAnimator.Init(tiles, rows, cols, colorSprites);

        StartCoroutine(floodAnimator.AnimateBoardSpawn());
    }
    public void AddMove()
    {
        movesLeft += 1;
        hudController?.SetMove(movesLeft);
    }


    public void OnColorSelected(Tile.TileColor color)
    {
        selectedColor = color;
        SoundManager.Instance.PlayClick();
    }

    public void OnTileClicked(int r, int c)
    {
        if (!GameManager.Instance.IsGameActive()) return;
        SoundManager.Instance.PlayFillClick();

        Tile.TileColor originalColor = tiles[r, c].Color;
        if (originalColor == selectedColor) return;

        StartCoroutine(RunFloodFill(r, c, originalColor, selectedColor));
    }

    private void FloodFill(int r, int c, Tile.TileColor targetColor, Tile.TileColor replacementColor)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols) return;
        if (tiles[r, c].Color != targetColor) return;

        tiles[r, c].Color = replacementColor;
        tiles[r, c].GetComponent<SpriteRenderer>().sprite = colorSprites[(int)replacementColor];

        FloodFill(r + 1, c, targetColor, replacementColor);
        FloodFill(r - 1, c, targetColor, replacementColor);
        FloodFill(r, c + 1, targetColor, replacementColor);
        FloodFill(r, c - 1, targetColor, replacementColor);
    }

    private IEnumerator RunFloodFill(int r, int c, Tile.TileColor originalColor, Tile.TileColor replacementColor)
    {
        movesLeft--;
        actualMovesUsed++;
        hudController?.SetMove(movesLeft);

        yield return StartCoroutine(floodAnimator.AnimateFloodFill(r, c, originalColor, replacementColor));

        if (CheckWin()) {
            //tính *
            int stars = StarSystem.CalculateStars(actualMovesUsed, currentLevel.movesAllowed);
            uiController?.UIWin(stars);
        }
        else if (movesLeft <= 0)
            uiController?.UILose();
    }

    
    private bool CheckWin()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (tiles[r, c].Color != goalColor && !tiles[r,c].isRock)
                        return false;
                }
            }
            return true;
        }
}
