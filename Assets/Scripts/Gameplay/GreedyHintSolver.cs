using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GreedyHintSolver
{
    // ===== TÌM GỢI Ý CHO GAMEPLAY CLICK BẤT KỲ Ô =====
    public static (int row, int col, Tile.TileColor suggestedColor) GetHint(
        Tile[,] tiles,
        int rows,
        int cols,
        Tile.TileColor targetColor)
    {
        Debug.Log($"[Greedy Hint] Bắt đầu tìm hint, Target: {targetColor}");

        // 1. Tìm tất cả các cụm trên board
        List<Cluster> allClusters = FindAllClusters(tiles, rows, cols);

        Debug.Log($"[Greedy Hint] Tìm thấy {allClusters.Count} cụm");

        // 2. Chiến lược: Tìm cụm NON-TARGET có nhiều ô láng giềng TARGET nhất
        // Khi fill cụm đó sang target → merge với các ô target lân cận

        List<(Cluster cluster, int targetNeighbors)> candidates = new List<(Cluster, int)>();

        foreach (var cluster in allClusters)
        {
            // Bỏ qua cụm đã là target hoặc là Rock
            if (cluster.color == targetColor || cluster.color == Tile.TileColor.Rock)
                continue;

            // Đếm số ô láng giềng là màu target
            int targetNeighborCount = CountTargetNeighbors(cluster, tiles, rows, cols, targetColor);

            if (targetNeighborCount > 0)
            {
                candidates.Add((cluster, targetNeighborCount));
            }
        }

        // Sắp xếp: Cân bằng giữa số láng giềng target và kích thước cụm
        // Score = (targetNeighbors * size) để ưu tiên CẢ HAI yếu tố
        // Cụm lớn + nhiều neighbors = score cao nhất
        candidates.Sort((a, b) =>
        {
            // Nhân với nhau để cả 2 yếu tố đều quan trọng
            float scoreA = a.targetNeighbors * a.cluster.size;
            float scoreB = b.targetNeighbors * b.cluster.size;

            return scoreB.CompareTo(scoreA);
        });

        // Debug: In ra top 3 candidates
        Debug.Log($"[Greedy Hint] Top candidates:");
        for (int i = 0; i < System.Math.Min(3, candidates.Count); i++)
        {
            var (cluster, neighbors) = candidates[i];
            float score = neighbors * cluster.size;
            Debug.Log($"  {i + 1}. {cluster.color} ({cluster.size} ô) × {neighbors} neighbors = Score {score}");
        }

        if (candidates.Count > 0)
        {
            var (bestCluster, neighborCount) = candidates[0];

            Debug.Log($"[Greedy Hint] Cụm tốt nhất: {bestCluster.color} ({bestCluster.size} ô) có {neighborCount} láng giềng target tại ({bestCluster.representative.r}, {bestCluster.representative.c})");

            return (bestCluster.representative.r, bestCluster.representative.c, targetColor);
        }

        // 3. Fallback: Không tìm thấy cụm có láng giềng target
        // → Tìm cụm NON-TARGET lớn nhất để fill trước
        List<Cluster> nonTargetClusters = allClusters
            .Where(c => c.color != targetColor && c.color != Tile.TileColor.Rock)
            .OrderByDescending(c => c.size)
            .ToList();

        if (nonTargetClusters.Count > 0)
        {
            var bestCluster = nonTargetClusters[0];

            Debug.Log($"[Greedy Hint] Không tìm thấy cụm kề target, fill cụm lớn nhất: {bestCluster.color} ({bestCluster.size} ô) tại ({bestCluster.representative.r}, {bestCluster.representative.c})");

            return (bestCluster.representative.r, bestCluster.representative.c, targetColor);
        }

        // 4. Nếu toàn board đã là target → Không cần hint nữa
        Debug.Log("[Greedy Hint] Board đã hoàn thành hoặc không tìm thấy hint!");
        return (-1, -1, targetColor);
    }

    // ===== ĐẾM SỐ Ô LÁNG GIỀNG LÀ MÀU TARGET =====
    private static int CountTargetNeighbors(Cluster cluster, Tile[,] tiles, int rows, int cols, Tile.TileColor targetColor)
    {
        HashSet<(int r, int c)> targetNeighbors = new HashSet<(int, int)>();

        int[] dr = { 1, -1, 0, 0 };
        int[] dc = { 0, 0, 1, -1 };

        foreach (var (r, c) in cluster.cells)
        {
            for (int i = 0; i < 4; i++)
            {
                int nr = r + dr[i];
                int nc = c + dc[i];

                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                {
                    if (tiles[nr, nc].Color == targetColor)
                    {
                        targetNeighbors.Add((nr, nc));
                    }
                }
            }
        }

        return targetNeighbors.Count;
    }

    // ===== TÌM TẤT CẢ CÁC CỤM TRÊN BOARD =====
    private static List<Cluster> FindAllClusters(Tile[,] tiles, int rows, int cols)
    {
        List<Cluster> clusters = new List<Cluster>();
        bool[,] visited = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!visited[r, c] && tiles[r, c].Color != Tile.TileColor.Rock)
                {
                    Cluster cluster = BFSCluster(tiles, rows, cols, r, c, visited);
                    clusters.Add(cluster);
                }
            }
        }

        return clusters;
    }

    // ===== BFS TÌM 1 CỤM =====
    private static Cluster BFSCluster(Tile[,] tiles, int rows, int cols, int startR, int startC, bool[,] visited)
    {
        Tile.TileColor color = tiles[startR, startC].Color;
        List<(int r, int c)> cells = new List<(int, int)>();

        Queue<(int r, int c)> queue = new Queue<(int, int)>();
        queue.Enqueue((startR, startC));
        visited[startR, startC] = true;

        int[] dr = { 1, -1, 0, 0 };
        int[] dc = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            cells.Add((r, c));

            for (int i = 0; i < 4; i++)
            {
                int nr = r + dr[i];
                int nc = c + dc[i];

                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && !visited[nr, nc])
                {
                    if (tiles[nr, nc].Color == color)
                    {
                        visited[nr, nc] = true;
                        queue.Enqueue((nr, nc));
                    }
                }
            }
        }

        return new Cluster
        {
            color = color,
            cells = cells,
            size = cells.Count,
            representative = cells[0] // Ô đại diện (để gợi ý click)
        };
    }

    // ===== CLASS CỤM =====
    private class Cluster
    {
        public Tile.TileColor color;
        public List<(int r, int c)> cells;
        public int size;
        public (int r, int c) representative; // Ô đại diện
    }
}