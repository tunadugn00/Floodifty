using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    private EndlessLevelGenerator.GeneratedLevel currentEndlessLevel;

    private Tile[,] tiles;
    private int rows, cols;
    private Tile.TileColor goalColor;
    private int movesLeft;
    private int actualMovesUsed;
    private Tile.TileColor selectedColor;
    public ColorButton[] colorButtons;
    public ItemButtonController itemButtonController;

    private bool isUIBlocking = false;
    public void SetUIBlocking(bool blocking) { isUIBlocking = blocking; }

    private bool hammerArmed = false;
    private bool colorBombArmed = false;

    public void ArmHammer()
    {
        if (ItemManager.Instance != null && ItemManager.Instance.HasHammer())
        {
            hammerArmed = true;
            colorBombArmed = false;
            SoundManager.Instance?.PlayClick();
        }
        else
        {
            SoundManager.VibrateIfEnabled();
        }
    }

    public void DisarmHammer()
    {
        hammerArmed = false;
    }

    public void ArmColorBomb()
    {
        if (ItemManager.Instance != null && ItemManager.Instance.HasColorBomb())
        {
            colorBombArmed = true;
            hammerArmed = false;
            SoundManager.Instance?.PlayClick();
        }
        else
        {
            SoundManager.VibrateIfEnabled();
        }
    }

    public void DisarmColorBomb()
    {
        colorBombArmed = false;
    }
    
    public void UseHammerOnTile(int r, int c)
    {
        if (!GameManager.Instance.IsGameActive()) return;
        if (isUIBlocking) return;
        if (tiles == null) return;
        if (ItemManager.Instance == null) return;
        if (r < 0 || r >= rows || c < 0 || c >= cols) return;

        Tile tile = tiles[r, c];
        if (tile == null || !tile.isRock)
        {
            SoundManager.VibrateIfEnabled();
            return;
        }

        if (!ItemManager.Instance.UseHammer())
        {
            SoundManager.VibrateIfEnabled();
            return;
        }

        // Spawn rock break FX
        if (floodAnimator != null && floodAnimator.rockParticlePrefab != null)
        {
            var fx = Instantiate(
                floodAnimator.rockParticlePrefab,
                tile.transform.position,
                Quaternion.identity
            );

            // Ensure FX doesn't leak (prefab should be non-looping)
            Destroy(fx, 1.5f);
        }

        Tile.TileColor newColor = GetDominantNeighborColor(r, c);
        tile.Color = newColor;

        var sr = tile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = colorSprites[(int)newColor];
        }

        hudController?.UpdateItemCounts();
        SoundManager.Instance?.PlayHammer();
    }

    private Tile.TileColor GetDominantNeighborColor(int r, int c)
    {
        int[] dr = { 1, -1, 0, 0 };
        int[] dc = { 0, 0, 1, -1 };

        var counts = new Dictionary<Tile.TileColor, int>();

        for (int i = 0; i < 4; i++)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];
            if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) continue;

            Tile neighbor = tiles[nr, nc];
            if (neighbor == null || neighbor.isRock) continue;

            if (!counts.ContainsKey(neighbor.Color))
                counts[neighbor.Color] = 0;
            counts[neighbor.Color]++;
        }

        if (counts.Count == 0)
            return goalColor;

        return counts.OrderByDescending(kv => kv.Value).First().Key;
    }

    void Start()
    {
        if (GameManager.Instance.isEndlessMode)
        {
            int stage = PlayerPrefs.GetInt("EndlessStage", 1);
            LoadEndlessLevel(stage);
        }
        else
        {
            int selected = PlayerPrefs.GetInt("SelectedLevel", 1);
            currentLevel = database.GetLevel(selected);
            LoadLevel(currentLevel);
        }
    }
    private Tile.TileColor ParseColor(string colorStr)
    {
        switch (colorStr)
        {
            case "R": return Tile.TileColor.Red;
            case "G": return Tile.TileColor.Green;
            case "B": return Tile.TileColor.Blue;
            case "Y": return Tile.TileColor.Yellow;
            default: return Tile.TileColor.Rock;
        }
    }
    public void LoadLevel(LevelData data)
    {
        rows = data.rows;
        cols = data.cols;
        goalColor = data.targetColor;
        movesLeft = data.movesAllowed;
        actualMovesUsed = 0;

        hudController?.SetMove(movesLeft);
        hudController?.SetGoal(goalColor);
        hudController?.SetLevel(PlayerPrefs.GetInt("SelectedLevel", 1), false);

        GenerateBoard(data);
        floodAnimator.Init(tiles, rows, cols, colorSprites);

        StartCoroutine(floodAnimator.AnimateBoardSpawn());
    }

    private void GenerateBoard(LevelData data)
    {
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
    public void LoadEndlessLevel(int stage)
    {
        currentEndlessLevel = EndlessLevelGenerator.GenerateLevel(stage);

        rows = currentEndlessLevel.rows;
        cols = currentEndlessLevel.cols;
        goalColor = ParseColor(currentEndlessLevel.targetColor);
        movesLeft = currentEndlessLevel.movesAllowed;
        actualMovesUsed = 0;

        hudController?.SetMove(movesLeft);
        hudController?.SetGoal(goalColor);
        hudController?.SetLevel(stage, true);

        GenerateEndlessBoard(currentEndlessLevel);

        floodAnimator.Init(tiles, rows, cols, colorSprites);
        StartCoroutine(floodAnimator.AnimateBoardSpawn());
    }

    private void GenerateEndlessBoard(EndlessLevelGenerator.GeneratedLevel data)
    {
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

                var color = ParseColor(data.layout[r, c]);
                tile.Color = color;

                var sr = tileObj.GetComponent<SpriteRenderer>();
                sr.sprite = colorSprites[(int)color];

                tiles[r, c] = tile;
            }
        }
    }
    public void ResetBoard()
    {
        actualMovesUsed = 0;
        if (GameManager.Instance.isEndlessMode && currentEndlessLevel != null)
        {
            movesLeft = currentEndlessLevel.movesAllowed;
            goalColor = ParseColor(currentEndlessLevel.targetColor);
            GenerateEndlessBoard(currentEndlessLevel);
        }
        else
        {
            movesLeft = currentLevel.movesAllowed;
            goalColor = currentLevel.targetColor;
            GenerateBoard(currentLevel);
        }

        hudController?.SetMove(movesLeft);
        hudController?.SetGoal(goalColor);

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

        foreach (var btn in colorButtons)
        {
            if (btn != null)
            {
                btn.UpdateVisualState(color);
            }
        }
    }

    public void OnTileClicked(int r, int c)
    {
        if (isUIBlocking) return;
        if (!GameManager.Instance.IsGameActive()) return;
        if (hammerArmed)
        {
            UseHammerOnTile(r, c);
            hammerArmed = false;
            return;
        }
        if (colorBombArmed)
        {
            UseColorBombOnTile(r, c);
            colorBombArmed = false;
            return;
        }

        Tile.TileColor originalColor = tiles[r, c].Color;
        if (originalColor == selectedColor)
        {
            SoundManager.VibrateIfEnabled();
            SoundManager.Instance.PlayClick();
            return;
        }

        SoundManager.Instance.PlayFillClick();
        StartCoroutine(RunFloodFill(r, c, originalColor, selectedColor));
    }

    public void UseColorBombOnTile(int r, int c)
    {
        if (!GameManager.Instance.IsGameActive()) return;
        if (isUIBlocking) return;
        if (tiles == null) return;
        if (ItemManager.Instance == null) return;
        if (r < 0 || r >= rows || c < 0 || c >= cols) return;

        Tile tile = tiles[r, c];
        if (tile == null || tile.isRock)
        {
            SoundManager.VibrateIfEnabled();
            return;
        }

        Tile.TileColor fromColor = tile.Color;  
        Tile.TileColor toColor = selectedColor; 

        if (fromColor == toColor)
        {
            SoundManager.VibrateIfEnabled();
            SoundManager.Instance?.PlayClick();
            return;
        }

        if (!ItemManager.Instance.UseColorBomb())
        {
            SoundManager.VibrateIfEnabled();
            return;
        }

        hudController?.UpdateItemCounts();
        SoundManager.Instance?.PlayColorBomb();
        StartCoroutine(RunColorBomb(fromColor, toColor));
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

        if (CheckWin())
        {
            int stars = 1;
            if (!GameManager.Instance.isEndlessMode && currentLevel != null)
            {
                stars = StarSystem.CalculateStars(actualMovesUsed, currentLevel.movesAllowed);
            }
            else if (currentEndlessLevel != null)
            {
                stars = StarSystem.CalculateStars(actualMovesUsed, currentEndlessLevel.movesAllowed);
            }
            uiController?.UIWin(stars, movesLeft);
        }
        else if (movesLeft <= 0)
            uiController?.UILose();
    }

    private IEnumerator RunColorBomb(Tile.TileColor fromColor, Tile.TileColor toColor)
    {
        if (floodAnimator != null)
            yield return StartCoroutine(floodAnimator.AnimateColorBomb(fromColor, toColor));

        itemButtonController?.OnColorBombConsumed();

        if (CheckWin())
        {
            int stars = 1;
            if (!GameManager.Instance.isEndlessMode && currentLevel != null)
            {
                stars = StarSystem.CalculateStars(actualMovesUsed, currentLevel.movesAllowed);
            }
            else if (currentEndlessLevel != null)
            {
                stars = StarSystem.CalculateStars(actualMovesUsed, currentEndlessLevel.movesAllowed);
            }
            uiController?.UIWin(stars, movesLeft);
        }
        else if (movesLeft <= 0)
        {
            uiController?.UILose();
        }
    }


    private bool CheckWin()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (tiles[r, c].Color != goalColor && !tiles[r, c].isRock)
                    return false;
            }
        }
        return true;
    }
    public Tile[,] GetTiles()
    {
        return tiles;
    }

    public Tile.TileColor GetGoalColor()
    {
        return goalColor;
    }
}