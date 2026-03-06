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
        if(shopPanel != null)
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
            Sprite originalSprite = sr.sprite;
            Sprite targetSprite = boardManager.colorSprites[(int)hintColor];

            for (int i = 0; i < 3; i++)
            {
                hintTile.transform.DOScale(hintTile.transform.localScale * 1.2f, 0.2f);
                sr.sprite = targetSprite;
                yield return new WaitForSeconds(0.3f);

                hintTile.transform.DOScale(Vector3.one, 0.2f);
                sr.sprite = originalSprite;
                yield return new WaitForSeconds(0.2f);
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