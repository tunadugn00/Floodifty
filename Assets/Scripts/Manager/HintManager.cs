using System.Collections;
using UnityEngine;
using DG.Tweening;

public class HintManager : MonoBehaviour
{
    public BoardManager boardManager;
    public HUDController hudController;
    public GameObject shopPanel;
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
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
    }

    private IEnumerator ShowHintRoutine()
    {
        isHinting = true;

        Tile[,] tiles = boardManager.GetTiles();
        int rows = tiles.GetLength(0);
        int cols = tiles.GetLength(1);
        Tile.TileColor targetColor = boardManager.currentLevel.targetColor;

        var (hintRow, hintCol, hintColor) = MCTSHintSolver.GetHint(tiles, rows, cols, targetColor);

        if (hintRow < 0 || hintCol < 0)
        {
            Debug.Log("[Hint] Không tìm thấy gợi ý!");
            isHinting = false;
            yield break;
        }

        Debug.Log($"[Hint] Gợi ý: Click ô ({hintRow}, {hintCol}) màu {tiles[hintRow, hintCol].Color} → Fill sang {hintColor}");

        Tile hintTile = tiles[hintRow, hintCol];
        SpriteRenderer sr = hintTile.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            // Lưu màu ban đầu để check
            Tile.TileColor originalColor = hintTile.Color;
            Sprite originalSprite = sr.sprite;
            Sprite targetSprite = boardManager.colorSprites[(int)hintColor];
            Vector3 originalScale = hintTile.transform.localScale;

            DOTween.Kill(hintTile.transform);

            // Nháy 3 lần (hoặc cho đến khi player fill)
            for (int i = 0; i < 3; i++)
            {
                // ===== CHECK NẾU TILE ĐÃ BỊ FILL → DỪNG ANIMATION =====
                if (hintTile.Color != originalColor)
                {
                    Debug.Log("[Hint] Tile đã bị fill, dừng animation!");
                    hintTile.transform.localScale = originalScale;
                    isHinting = false;
                    yield break;
                }

                // Scale up
                hintTile.transform.DOScale(originalScale * 1.2f, 0.2f);
                sr.sprite = targetSprite;
                yield return new WaitForSeconds(0.3f);

                // ===== CHECK LẦN NỮA =====
                if (hintTile.Color != originalColor)
                {
                    Debug.Log("[Hint] Tile đã bị fill, dừng animation!");
                    hintTile.transform.localScale = originalScale;
                    isHinting = false;
                    yield break;
                }

                // Scale down
                hintTile.transform.DOScale(originalScale, 0.2f);
                sr.sprite = originalSprite;
                yield return new WaitForSeconds(0.2f);
            }

            // Force về trạng thái cuối
            hintTile.transform.localScale = originalScale;

            // Chỉ set sprite nếu màu chưa đổi
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