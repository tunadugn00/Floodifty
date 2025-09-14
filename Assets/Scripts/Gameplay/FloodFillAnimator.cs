using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class FloodFillAnimator : MonoBehaviour
{
    public float tweenDuration = 0.15f;
    public float layerDelay = 0.05f;

    private Sprite[] colorSprites;
    private Tile[,] tiles;
    private int rows, cols;

    public GameObject particlePrefab;

    public void Init(Tile[,] tiles, int rows, int cols, Sprite[] sprites)
    {
        this.tiles = tiles;
        this.rows = rows;
        this.cols = cols;
        this.colorSprites = sprites;
    }

    public IEnumerator AnimateFloodFill(int startR, int startC, Tile.TileColor targetColor, Tile.TileColor replacementColor)
    {
        Queue<(int r, int c, int depth)> queue = new Queue<(int, int, int)>();
        bool[,] visited = new bool[rows, cols];
        List<List<Tile>> layers = new List<List<Tile>>();

        // BFS bắt đầu từ ô (startR, startC)
        queue.Enqueue((startR, startC, 0));
        visited[startR, startC] = true;

        // Vòng lặp BFS: lấy từng ô ra khỏi hàng đợi
        while (queue.Count > 0)
        {
            var (r, c, depth) = queue.Dequeue();
            if (tiles[r, c].isRock) continue; // không fill rock tile
            if (tiles[r, c].Color != targetColor) continue; // Nếu ô không cùng màu mục tiêu thì bỏ qua

            // Gom ô này vào đúng "layer" theo độ sâu BFS
            // depth = khoảng cách từ ô bắt đầu
            if (layers.Count <= depth) layers.Add(new List<Tile>());
            layers[depth].Add(tiles[r, c]);

            // Duyệt 4 hướng: lên, xuống, trái, phải 
            int[] dr = { 1, -1, 0, 0 };
            int[] dc = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int nr = r + dr[i];
                int nc = c + dc[i];
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && !visited[nr, nc] && !tiles[nr,nc].isRock)
                {
                    visited[nr, nc] = true;
                    queue.Enqueue((nr, nc, depth + 1));
                }
            }
        }

        // Sau khi BFS gom đủ các lớp -> chạy animation theo từng lớp
        foreach (var layer in layers)
        {
            foreach (var tile in layer)
            {
                tile.Color = replacementColor;
                StartCoroutine(TweenColor(tile, replacementColor, tweenDuration));

                if (layer == layers[0])
                {
                    SpawnParticle(tile, replacementColor);
                }

                // -----------------OPTION 2: DOTween----------------------
                /*
                tile.Color = replacementColor;
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();

                // set sprite mới trước
                sr.sprite = colorSprites[(int)replacementColor];
                sr.color = new Color(1, 1, 1, 0); // start alpha 0

                // dùng DOTween để fade-in
                sr.DOFade(1f, tweenDuration);
                */
            }
            // Delay giữa các lớp -> tạo hiệu ứng sóng lan
            yield return new WaitForSeconds(layerDelay);
        }
    }

    private IEnumerator TweenColor(Tile tile, Tile.TileColor targetColor, float duration)
    {
        // Lấy SpriteRenderer của ô hiện tại
        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        Sprite newSprite = colorSprites[(int)targetColor]; // Sprite màu mới

        // Đổi sprite ngay, nhưng set alpha = 0 (ẩn)
        sr.sprite = newSprite;
        sr.color = new Color(1f, 1f, 1f, 0f);

        // Tăng alpha từ 0 -> 1 theo thời gian -> fade-in
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            sr.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }

        //reset color
        sr.color = Color.white;
    }

    void SpawnParticle(Tile tile,Tile.TileColor color)
    {
        if (tile == null || tile.gameObject == null) return;

        var fx = Instantiate(particlePrefab, tile.transform.position, Quaternion.identity);
        var ps = fx.GetComponent<ParticleSystem>();

        // đổi màu hạt theo màu tile
        var main = ps.main;
        main.startColor = GetColorForTile(color);

        Destroy(fx, 1f);
    }
    private Color GetColorForTile(Tile.TileColor tileColor)
    {
        switch (tileColor)
        {
            case Tile.TileColor.Red: return Color.red;
            case Tile.TileColor.Green: return Color.green;
            case Tile.TileColor.Blue: return Color.blue;
            case Tile.TileColor.Yellow: return Color.yellow;
        }
        return Color.white;
    }
}
