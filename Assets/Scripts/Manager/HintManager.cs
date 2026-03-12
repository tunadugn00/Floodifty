using System.Collections;
using UnityEngine;
using DG.Tweening;

public class HintManager : MonoBehaviour
{
    public BoardManager boardManager;
    public HUDController hudController;
    public ShopController shopController;
    private bool isHinting = false;

    public void OnHintButtonClicked()
    {
        if (isHinting) return;

        if (!ItemManager.Instance.HasHint())
        {
            OpenShop();
            SoundManager.Instance?.PlayClick();
            return;
        }
        bool success = ItemManager.Instance.UseHint();

        if (success)
        {
            hudController?.UpdateItemCounts();
            StartCoroutine(ShowHintRoutine());
        }
    }

    private void OpenShop()
    {
        if (shopController != null)
        {
            shopController.OpenShop();
        }
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

        var (hintRow, hintCol, hintColor) = MCTSHintSolver.GetHint(tiles, rows, cols, targetColor);

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

            for (int i = 0; i < 3; i++)
            {
                if (hintTile.Color != originalColor)
                {

                    hintTile.transform.localScale = originalScale;
                    isHinting = false;
                    yield break;
                }

                hintTile.transform.DOScale(originalScale * 1.2f, 0.2f);
                sr.sprite = targetSprite;
                yield return new WaitForSeconds(0.3f);

            }

            hintTile.transform.localScale = originalScale;
            if (hintTile.Color == originalColor)
            {
                sr.sprite = originalSprite;
            }
        }

        HighlightUIButton(hintColor);

        isHinting = false;
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