using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class AStarSolver
{
    public static List<Tile.TileColor> GetFullSolution(Tile[,] tiles, int rows, int cols, Tile.TileColor targetColor)
    {
        // DEBUG: In ra trạng thái ban đầu
        Debug.Log($"[A*] === BẮT ĐẦU TÌM GIẢI PHÁP ===");
        Debug.Log($"[A*] Board size: {rows}x{cols}, Target: {targetColor}");
        Debug.Log($"[A*] Màu góc (0,0): {tiles[0, 0].Color}");

        var openSet = new SortedSet<AStarNode>(Comparer<AStarNode>.Create((a, b) =>
        {
            int cmp = a.fScore.CompareTo(b.fScore);
            if (cmp != 0) return cmp;

            // TIE-BREAKING: Ưu tiên PATH có nước đầu tiên cho gain lớn
            // Chỉ so sánh khi đều là nước đầu tiên (path.Count == 1)
            if (a.path.Count == 1 && b.path.Count == 1)
            {
                cmp = b.firstMoveGain.CompareTo(a.firstMoveGain); // Lớn hơn = tốt hơn
                if (cmp != 0) return cmp;
            }

            cmp = a.gScore.CompareTo(b.gScore);
            if (cmp != 0) return cmp;

            return a.stateHash.CompareTo(b.stateHash);
        }));

        var visited = new HashSet<string>();

        BoardState initialState = new BoardState(tiles, rows, cols);
        string startHash = initialState.GetHash();

        Debug.Log($"[A*] Vùng đất ban đầu: {initialState.playerTerritory.Count} ô");

        int hStart = CalculateHeuristic(initialState, targetColor);

        var startNode = new AStarNode
        {
            state = initialState,
            gScore = 0,
            fScore = hStart,
            firstMoveGain = 0,
            path = new List<Tile.TileColor>(),
            stateHash = startHash
        };

        openSet.Add(startNode);

        int nodesVisited = 0;
        int maxNodes = 10000;

        while (openSet.Count > 0 && nodesVisited < maxNodes)
        {
            var current = openSet.Min;
            openSet.Remove(current);

            string currentHash = current.stateHash;
            if (visited.Contains(currentHash)) continue;
            visited.Add(currentHash);

            nodesVisited++;

            // Đã thắng?
            if (IsGoal(current.state, targetColor))
            {
                Debug.Log($"[A*] ✅ GIẢI PHÁP TÌM THẤY:");
                Debug.Log($"[A*]    - Số bước: {current.path.Count}");
                Debug.Log($"[A*]    - Nodes duyệt: {nodesVisited}");
                Debug.Log($"[A*]    - Đường đi: {string.Join(" → ", current.path)}");
                Debug.Log($"[A*]    - Màu cuối: {current.state.playerColor} (Target: {targetColor})");
                return current.path;
            }

            // DEBUG: In ra trạng thái hiện tại
            if (current.gScore == 0)
            {
                Debug.Log($"[A*] --- ĐÁNH GIÁ CÁC NƯỚC ĐI TỪ TRẠNG THÁI BAN ĐẦU ---");
            }

            // Thử tất cả màu và log kết quả
            List<(Tile.TileColor color, int gain, int newSize)> moveOptions = new List<(Tile.TileColor, int, int)>();

            foreach (Tile.TileColor color in System.Enum.GetValues(typeof(Tile.TileColor)))
            {
                // Skip màu không hợp lệ, màu hiện tại, và ROCK
                if ((int)color < 0)
                {
                    Debug.Log($"[A*] Skip màu có index < 0: {color}");
                    continue;
                }

                if (color == current.state.playerColor)
                {
                    Debug.Log($"[A*] Skip màu hiện tại: {color}");
                    continue;
                }

                if (color == Tile.TileColor.Rock)
                {
                    Debug.Log($"[A*] Skip Rock!");
                    continue;
                }

                BoardState testState = current.state.Clone();
                int oldSize = testState.playerTerritory.Count;
                testState.ApplyMove(color);
                int newSize = testState.playerTerritory.Count;
                int gain = newSize - oldSize;

                moveOptions.Add((color, gain, newSize));

                string newHash = testState.GetHash();
                if (visited.Contains(newHash)) continue;

                int newG = current.gScore + 1;
                int newH = CalculateHeuristic(testState, targetColor);
                int newF = newG + newH;

                List<Tile.TileColor> newPath = new List<Tile.TileColor>(current.path);
                newPath.Add(color);

                // Lưu lại gain của nước đầu tiên
                int firstGain = (current.gScore == 0) ? gain : current.firstMoveGain;

                var newNode = new AStarNode
                {
                    state = testState,
                    gScore = newG,
                    fScore = newF,
                    firstMoveGain = firstGain, // Gain của nước ĐẦU TIÊN trong path
                    path = newPath,
                    stateHash = newHash
                };

                openSet.Add(newNode);
            }

            // DEBUG: In bảng so sánh các nước đi (chỉ ở bước đầu)
            if (current.gScore == 0 && moveOptions.Count > 0)
            {
                Debug.Log($"[A*] Các nước đi khả thi:");
                foreach (var (color, gain, newSize) in moveOptions.OrderByDescending(x => x.gain))
                {
                    Debug.Log($"[A*]   - {color}: +{gain} ô → {newSize} ô tổng");
                }
            }
        }

        Debug.LogWarning($"[A*] ❌ Không tìm thấy giải pháp sau {nodesVisited} nodes!");
        return new List<Tile.TileColor>();
    }

    private static int CalculateHeuristic(BoardState state, Tile.TileColor targetColor)
    {
        int totalTiles = state.rows * state.cols;
        int conqueredTiles = state.playerTerritory.Count;
        int remaining = totalTiles - conqueredTiles;

        if (remaining == 0) return 0; // Đã chiếm hết, IsGoal sẽ check màu

        // Heuristic đơn giản: ước lượng số bước còn lại
        return (int)System.Math.Ceiling(remaining / (double)totalTiles * 4);
    }

    private static bool IsGoal(BoardState state, Tile.TileColor targetColor)
    {
        // Phải chiếm hết board VÀ màu cuối cùng phải là targetColor
        return state.playerTerritory.Count == state.rows * state.cols
               && state.playerColor == targetColor;
    }

    public static Tile.TileColor GetBestMove(Tile[,] tiles, int rows, int cols, Tile.TileColor targetColor)
    {
        var solution = GetFullSolution(tiles, rows, cols, targetColor);
        return solution.Count > 0 ? solution[0] : (Tile.TileColor)(-1);
    }

    private class AStarNode
    {
        public BoardState state;
        public int gScore;
        public int fScore;
        public int firstMoveGain; // Gain của NƯỚC ĐẦU TIÊN trong path này
        public List<Tile.TileColor> path;
        public string stateHash;
    }

    private class BoardState
    {
        public int rows, cols;
        public Tile.TileColor[,] grid;
        public Tile.TileColor playerColor;
        public HashSet<(int r, int c)> playerTerritory;

        public BoardState(Tile[,] tiles, int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            this.grid = new Tile.TileColor[rows, cols];
            this.playerColor = tiles[0, 0].Color;
            this.playerTerritory = new HashSet<(int r, int c)>();

            // DEBUG: In ra màu các góc
            Debug.Log($"[BoardState] Khởi tạo board {rows}x{cols}");
            Debug.Log($"[BoardState] tiles[0,0] = {tiles[0, 0].Color}");
            Debug.Log($"[BoardState] tiles[0,{cols - 1}] = {tiles[0, cols - 1].Color}");
            Debug.Log($"[BoardState] tiles[{rows - 1},0] = {tiles[rows - 1, 0].Color}");
            Debug.Log($"[BoardState] tiles[{rows - 1},{cols - 1}] = {tiles[rows - 1, cols - 1].Color}");

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = tiles[r, c].Color;
                }
            }

            UpdatePlayerTerritory();

            Debug.Log($"[BoardState] playerColor sau init = {this.playerColor}");
            Debug.Log($"[BoardState] playerTerritory size = {this.playerTerritory.Count}");
        }

        private BoardState() { }

        public BoardState Clone()
        {
            BoardState clone = new BoardState
            {
                rows = this.rows,
                cols = this.cols,
                grid = (Tile.TileColor[,])this.grid.Clone(),
                playerColor = this.playerColor,
                playerTerritory = new HashSet<(int r, int c)>(this.playerTerritory)
            };
            return clone;
        }

        public void ApplyMove(Tile.TileColor newColor)
        {
            foreach (var (r, c) in playerTerritory)
            {
                grid[r, c] = newColor;
            }
            playerColor = newColor;
            UpdatePlayerTerritory();
        }

        private void UpdatePlayerTerritory()
        {
            playerTerritory.Clear();
            bool[,] visited = new bool[rows, cols];

            Queue<(int r, int c)> q = new Queue<(int r, int c)>();
            q.Enqueue((0, 0));
            visited[0, 0] = true;
            playerTerritory.Add((0, 0));

            int[] dr = { 1, -1, 0, 0 };
            int[] dc = { 0, 0, 1, -1 };

            while (q.Count > 0)
            {
                var (r, c) = q.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nr = r + dr[i];
                    int nc = c + dc[i];

                    if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && !visited[nr, nc])
                    {
                        if (grid[nr, nc] == playerColor)
                        {
                            visited[nr, nc] = true;
                            playerTerritory.Add((nr, nc));
                            q.Enqueue((nr, nc));
                        }
                    }
                }
            }
        }

        public string GetHash()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    sb.Append((int)grid[r, c]);
                }
            }
            return sb.ToString();
        }
    }
}