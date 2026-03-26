using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HintManager : MonoBehaviour
{
    [System.Serializable]
    private class TutorialHintStep
    {
        public int row;
        public int col;
        public Tile.TileColor suggestedColor;
    }

    public BoardManager boardManager;
    public HUDController hudController;
    private bool isHinting = false;

    [Header("Tutorial Hint")]
    [SerializeField] private bool useTutorialHintsForLevelOne = true;
    [SerializeField] private List<TutorialHintStep> tutorialHintSteps = new List<TutorialHintStep>();

    private int tutorialHintIndex = 0;

    private void OnEnable()
    {
        tutorialHintIndex = 0;
    }

    public void StartHint()
    {
        if (isHinting) return;
        StartCoroutine(ShowHintRoutine());
    }

    private IEnumerator ShowHintRoutine()
    {
        isHinting = true;

        Tile[,] tiles = boardManager.GetTiles();
        int rows = tiles.GetLength(0);
        int cols = tiles.GetLength(1);

        // An toàn cho cả Normal và Endless mode
        Tile.TileColor targetColor = (boardManager.currentLevel != null)
            ? boardManager.currentLevel.targetColor
            : boardManager.GetGoalColor();

        var (hintRow, hintCol, hintColor) = ResolveHint(tiles, rows, cols, targetColor);

        if (hintRow < 0 || hintCol < 0)
        {
            isHinting = false;
            yield break;
        }

        Tile hintTile = tiles[hintRow, hintCol];
        SpriteRenderer sr = hintTile.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Tile.TileColor originalColor = hintTile.Color;
            Sprite originalSprite = sr.sprite;
            Sprite targetSprite = boardManager.colorSprites[(int)hintColor];
            Vector3 originalScale = hintTile.transform.localScale;

            DOTween.Kill(hintTile.transform);

            // Tạo sequence nổi bật hơn: zoom + nháy màu rõ ràng
            var seq = DOTween.Sequence();

            for (int i = 0; i < 3; i++)
            {
                // Nếu người chơi đã đổi màu ô này trong lúc hint, dừng sớm
                if (hintTile.Color != originalColor)
                {
                    hintTile.transform.localScale = originalScale;
                    sr.sprite = hintTile.Color == originalColor ? originalSprite : sr.sprite;
                    isHinting = false;
                    yield break;
                }

                seq.AppendCallback(() =>
                {
                    sr.sprite = targetSprite;
                });

                seq.Append(hintTile.transform
                    .DOScale(originalScale * 1.3f, 0.18f)
                    .SetEase(Ease.OutBack));

                seq.AppendInterval(0.05f);

                seq.Append(hintTile.transform
                    .DOScale(originalScale * 0.95f, 0.16f)
                    .SetEase(Ease.InOutSine));

                seq.AppendInterval(0.05f);
            }

            seq.OnComplete(() =>
            {
                hintTile.transform.localScale = originalScale;
                if (hintTile.Color == originalColor)
                {
                    sr.sprite = originalSprite;
                }
            });

            // Chờ sequence chạy xong
            yield return seq.WaitForCompletion();
        }

        // 1) Tự chọn sẵn màu được gợi ý (UX: người chơi chỉ cần tap board)
        boardManager.OnColorSelected(hintColor);

        // 1) Ghost preview flood-fill: cho người chơi thấy trước kết quả, rồi trả board về trạng thái cũ
        yield return StartCoroutine(PreviewFloodFill(tiles, hintRow, hintCol, hintTile.Color, hintColor));

        HighlightUIButton(hintColor);

        isHinting = false;
    }

    private (int row, int col, Tile.TileColor color) ResolveHint(
        Tile[,] tiles,
        int rows,
        int cols,
        Tile.TileColor targetColor)
    {
        if (ShouldUseTutorialHint())
        {
            var tutorialHint = TryGetTutorialHint(tiles, rows, cols);
            if (tutorialHint.row >= 0)
            {
                return tutorialHint;
            }
        }

        return MCTSHintSolver.GetHint(tiles, rows, cols, targetColor);
    }

    private bool ShouldUseTutorialHint()
    {
        if (!useTutorialHintsForLevelOne) return false;
        if (GameManager.Instance == null || GameManager.Instance.isEndlessMode) return false;
        return PlayerPrefs.GetInt("SelectedLevel", 1) == 1;
    }

    private (int row, int col, Tile.TileColor color) TryGetTutorialHint(Tile[,] tiles, int rows, int cols)
    {
        if (tutorialHintSteps == null || tutorialHintSteps.Count == 0)
        {
            return (-1, -1, Tile.TileColor.Red);
        }

        while (tutorialHintIndex < tutorialHintSteps.Count)
        {
            var step = tutorialHintSteps[tutorialHintIndex];
            tutorialHintIndex++;

            if (step == null) continue;
            if (step.row < 0 || step.row >= rows || step.col < 0 || step.col >= cols) continue;

            var tile = tiles[step.row, step.col];
            if (tile == null || tile.isRock) continue;

            return (step.row, step.col, step.suggestedColor);
        }

        return (-1, -1, Tile.TileColor.Red);
    }

    private IEnumerator PreviewFloodFill(Tile[,] tiles, int startR, int startC, Tile.TileColor fromColor, Tile.TileColor toColor)
    {
        int rows = tiles.GetLength(0);
        int cols = tiles.GetLength(1);

        // Sao lưu màu hiện tại
        var backup = new Tile.TileColor[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                backup[r, c] = tiles[r, c].Color;
            }
        }

        // Chạy animation flood-fill thật nhưng KHÔNG trừ move, KHÔNG check win
        if (boardManager.floodAnimator != null)
        {
            yield return boardManager.StartCoroutine(
                boardManager.floodAnimator.AnimateFloodFill(startR, startC, fromColor, toColor)
            );
        }

        // Trả lại trạng thái board ban đầu
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Tile tile = tiles[r, c];
                if (tile == null) continue;

                tile.Color = backup[r, c];

                var sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (tile.isRock)
                    {
                        sr.sprite = boardManager.rockSprite;
                    }
                    else
                    {
                        sr.sprite = boardManager.colorSprites[(int)tile.Color];
                    }
                }
            }
        }
    }

    private void HighlightUIButton(Tile.TileColor colorToPress)
    {
        foreach (var btn in boardManager.colorButtons)
        {
            if (btn != null && (int)btn.colorType == (int)colorToPress)
            {
                btn.transform.DOScale(btn.transform.localScale * 1.3f, 0.2f).SetLoops(4, LoopType.Yoyo);
                break;
            }
        }
    }
}