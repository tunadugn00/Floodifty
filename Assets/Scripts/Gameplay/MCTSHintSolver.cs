using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MCTSHintSolver
{
    const int SIMULATION_COUNT = 100; // Số lần simulate
    const int MAX_DEPTH = 6; // Độ sâu tối đa

    // ===== MAIN: TÌM HINT BẰNG MCTS =====
    public static (int row, int col, Tile.TileColor suggestedColor) GetHint(
        Tile[,] tiles,
        int rows,
        int cols,
        Tile.TileColor targetColor)
    {
        Debug.Log($"[MCTS Hint] Bắt đầu tìm hint với {SIMULATION_COUNT} simulations...");

        var startTime = System.DateTime.Now;

        // Tìm tất cả moves có thể
        var possibleMoves = GetAllPossibleMoves(tiles, rows, cols, targetColor);

        if (possibleMoves.Count == 0)
        {
            Debug.LogWarning("[MCTS Hint] Không tìm thấy move hợp lệ!");
            return (-1, -1, targetColor);
        }

        // Chạy simulations cho mỗi move
        var moveScores = new Dictionary<(int, int, Tile.TileColor), float>();

        foreach (var move in possibleMoves)
        {
            float totalScore = 0;

            for (int i = 0; i < SIMULATION_COUNT; i++)
            {
                float score = SimulateGame(tiles, rows, cols, targetColor, move);
                totalScore += score;
            }

            moveScores[move] = totalScore / SIMULATION_COUNT;
        }

        // Chọn move có score cao nhất
        var bestMove = moveScores.OrderByDescending(x => x.Value).First();

        var elapsed = (System.DateTime.Now - startTime).TotalMilliseconds;
        Debug.Log($"[MCTS Hint] Tìm thấy best move trong {elapsed:F0}ms");
        Debug.Log($"[MCTS Hint] Best: ({bestMove.Key.Item1}, {bestMove.Key.Item2}) → {bestMove.Key.Item3} (Score: {bestMove.Value:F2})");

        return bestMove.Key;
    }

    // ===== TÌM TẤT CẢ MOVES CÓ THỂ =====
    static List<(int r, int c, Tile.TileColor color)> GetAllPossibleMoves(
        Tile[,] tiles, int rows, int cols, Tile.TileColor target)
    {
        var moves = new List<(int, int, Tile.TileColor)>();
        var clusters = FindAllClusters(tiles, rows, cols);

        // Với mỗi cụm, thử fill sang tất cả màu khác
        foreach (var cluster in clusters)
        {
            if (cluster.color == target) continue;

            var (r, c) = cluster.representative;

            // Thử fill sang target
            moves.Add((r, c, target));

            // Thử fill sang các màu khác (để merge trung gian)
            foreach (Tile.TileColor color in System.Enum.GetValues(typeof(Tile.TileColor)))
            {
                if ((int)color < 0 || color == Tile.TileColor.Rock || color == cluster.color)
                    continue;

                moves.Add((r, c, color));
            }
        }

        return moves;
    }

    // ===== SIMULATE GAME: CHƠI RANDOM TỪ 1 MOVE =====
    static float SimulateGame(
        Tile[,] tiles, int rows, int cols,
        Tile.TileColor target,
        (int r, int c, Tile.TileColor color) firstMove)
    {
        // Clone board
        var board = new Tile.TileColor[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                board[r, c] = tiles[r, c].Color;

        // Apply first move
        ApplyMove(board, rows, cols, firstMove.r, firstMove.c, firstMove.color);

        int moves = 1;

        // Simulate thêm random moves
        for (int depth = 1; depth < MAX_DEPTH; depth++)
        {
            if (IsWin(board, rows, cols, target))
            {
                // Win càng sớm = score càng cao
                return 100.0f / moves;
            }

            // Random move tiếp theo (với bias về target)
            var clusters = FindAllClusters(board, rows, cols);
            if (clusters.Count == 0) break;

            // 70% fill về target, 30% random
            Cluster chosenCluster;
            Tile.TileColor chosenColor;

            if (Random.value < 0.7f)
            {
                // Ưu tiên fill sang target
                chosenCluster = clusters
                    .Where(c => c.color != target)
                    .OrderByDescending(c => CountTargetNeighbors(c, board, rows, cols, target) * c.size)
                    .FirstOrDefault();

                if (chosenCluster == null) break;
                chosenColor = target;
            }
            else
            {
                // Random
                chosenCluster = clusters[Random.Range(0, clusters.Count)];
                var colors = new[] { Tile.TileColor.Red, Tile.TileColor.Green, Tile.TileColor.Blue, Tile.TileColor.Yellow };
                chosenColor = colors[Random.Range(0, colors.Length)];
            }

            var (r, c) = chosenCluster.representative;
            ApplyMove(board, rows, cols, r, c, chosenColor);
            moves++;
        }

        // Không win trong MAX_DEPTH → Score dựa vào % target đã có
        int targetCount = 0;
        int total = rows * cols;

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (board[r, c] == target)
                    targetCount++;

        return (float)targetCount / total * 10; // Score 0-10 dựa vào % target
    }

    // ===== APPLY MOVE: FLOOD FILL TỪ 1 Ô =====
    static void ApplyMove(Tile.TileColor[,] board, int rows, int cols, int startR, int startC, Tile.TileColor newColor)
    {
        Tile.TileColor oldColor = board[startR, startC];
        if (oldColor == newColor) return;

        var stack = new Stack<(int r, int c)>();
        stack.Push((startR, startC));
        var visited = new bool[rows, cols];

        while (stack.Count > 0)
        {
            var (r, c) = stack.Pop();

            if (r < 0 || r >= rows || c < 0 || c >= cols || visited[r, c])
                continue;
            if (board[r, c] != oldColor)
                continue;

            board[r, c] = newColor;
            visited[r, c] = true;

            stack.Push((r + 1, c));
            stack.Push((r - 1, c));
            stack.Push((r, c + 1));
            stack.Push((r, c - 1));
        }
    }

    // ===== HELPER FUNCTIONS =====

    static bool IsWin(Tile.TileColor[,] board, int rows, int cols, Tile.TileColor target)
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (board[r, c] != target)
                    return false;
        return true;
    }

    static List<Cluster> FindAllClusters(Tile[,] tiles, int rows, int cols)
    {
        var clusters = new List<Cluster>();
        var visited = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!visited[r, c] && tiles[r, c].Color != Tile.TileColor.Rock)
                {
                    var cluster = BFSCluster(tiles, rows, cols, r, c, visited);
                    clusters.Add(cluster);
                }
            }
        }

        return clusters;
    }

    static List<Cluster> FindAllClusters(Tile.TileColor[,] board, int rows, int cols)
    {
        var clusters = new List<Cluster>();
        var visited = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!visited[r, c] && board[r, c] != Tile.TileColor.Rock)
                {
                    var cluster = BFSCluster(board, rows, cols, r, c, visited);
                    clusters.Add(cluster);
                }
            }
        }

        return clusters;
    }

    static Cluster BFSCluster(Tile[,] tiles, int rows, int cols, int startR, int startC, bool[,] visited)
    {
        Tile.TileColor color = tiles[startR, startC].Color;
        var cells = new List<(int r, int c)>();
        var queue = new Queue<(int, int)>();

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
            representative = cells[0]
        };
    }

    static Cluster BFSCluster(Tile.TileColor[,] board, int rows, int cols, int startR, int startC, bool[,] visited)
    {
        Tile.TileColor color = board[startR, startC];
        var cells = new List<(int r, int c)>();
        var queue = new Queue<(int, int)>();

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
                    if (board[nr, nc] == color)
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
            representative = cells[0]
        };
    }

    static int CountTargetNeighbors(Cluster cluster, Tile.TileColor[,] board, int rows, int cols, Tile.TileColor target)
    {
        var neighbors = new HashSet<(int, int)>();
        int[] dr = { 1, -1, 0, 0 };
        int[] dc = { 0, 0, 1, -1 };

        foreach (var (r, c) in cluster.cells)
        {
            for (int i = 0; i < 4; i++)
            {
                int nr = r + dr[i];
                int nc = c + dc[i];
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && board[nr, nc] == target)
                    neighbors.Add((nr, nc));
            }
        }
        return neighbors.Count;
    }

    class Cluster
    {
        public Tile.TileColor color;
        public List<(int r, int c)> cells;
        public int size;
        public (int r, int c) representative;
    }
}