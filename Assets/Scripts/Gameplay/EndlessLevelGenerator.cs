using System.Collections.Generic;
using UnityEngine;

public static class EndlessLevelGenerator
{
    static int ROWS = 10;
    static int COLS = 8;
    static string[] COLORS = { "R", "G", "B", "Y" };

    public class GeneratedLevel
    {
        public int rows;
        public int cols;
        public string targetColor;
        public int movesAllowed;
        public string[,] layout;
    }

    public static GeneratedLevel GenerateLevel(int endlessStage,
    PlayerPerformanceTracker.PlayerTier tier = PlayerPerformanceTracker.PlayerTier.Normal)
    {
        int targetMoves = Mathf.Clamp(3 + (endlessStage / 3), 3, 8);

        // Bonus moves theo tier
        int bonusMoves = tier switch
        {
            PlayerPerformanceTracker.PlayerTier.Beginner => 2,
            PlayerPerformanceTracker.PlayerTier.Normal => 1,
            PlayerPerformanceTracker.PlayerTier.Expert => 0,
            _ => 1
        };

        // Blob adjustment theo tier
        int blobBonus = tier switch
        {
            PlayerPerformanceTracker.PlayerTier.Beginner => -1, // ít blob hơn
            PlayerPerformanceTracker.PlayerTier.Normal => 0,
            PlayerPerformanceTracker.PlayerTier.Expert => 1, // nhiều blob hơn
            _ => 0
        };

        for (int i = 0; i < 5; i++)
        {
            string targetCol;
            var board = GenGuaranteedBoard(targetMoves, out targetCol, blobBonus);
            int? actualMoves = MinMovesClickAnywhere(board, targetCol, targetMoves + 2);

            if (actualMoves.HasValue && actualMoves.Value > 0)
            {
                Debug.Log($"[Endless] Stage {endlessStage} | Tier: {tier} | Moves: {actualMoves.Value + bonusMoves}");
                return CreateLevelData(board, targetCol, actualMoves.Value + bonusMoves);
            }
        }

        string safeTarget;
        var safeBoard = GenGuaranteedBoard(targetMoves, out safeTarget, blobBonus);
        return CreateLevelData(safeBoard, safeTarget, targetMoves + bonusMoves + 2);
    }

    private static GeneratedLevel CreateLevelData(string[,] board, string target, int moves)
    {
        return new GeneratedLevel { rows = ROWS, cols = COLS, targetColor = target, movesAllowed = moves, layout = board };
    }

    // ===== THUẬT TOÁN "TẠO MAP NGƯỢC" (CONSTRUCTIVE GENERATION) =====
    static string[,] GenGuaranteedBoard(int difficulty, out string targetColor, int blobBonus = 0)
    {
        var board = new string[ROWS, COLS];
        targetColor = COLORS[Random.Range(0, COLORS.Length)];

        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                board[r, c] = targetColor;

        // Áp dụng blobBonus vào số lượng blob
        int blobsToDraw = Mathf.Max(1, difficulty + Random.Range(1, 4) + blobBonus);

        for (int i = 0; i < blobsToDraw; i++)
        {
            string randomColor = COLORS[Random.Range(0, COLORS.Length)];
            if (randomColor == targetColor) continue;

            int startR = Random.Range(0, ROWS);
            int startC = Random.Range(0, COLS);
            int blobSize = Random.Range(4, 10);
            DrawBlob(board, startR, startC, randomColor, blobSize);
        }

        return board;
    }

    static void DrawBlob(string[,] board, int startR, int startC, string color, int maxSize)
    {
        var queue = new Queue<(int r, int c)>();
        var visited = new HashSet<(int, int)>();
        queue.Enqueue((startR, startC));
        visited.Add((startR, startC));

        int filled = 0;
        while (queue.Count > 0 && filled < maxSize)
        {
            var (r, c) = queue.Dequeue();
            board[r, c] = color; // Ghi đè màu lên lớp nền
            filled++;

            var directions = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dr, dc) in directions)
            {
                int nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < ROWS && nc >= 0 && nc < COLS && !visited.Contains((nr, nc)) && Random.value < 0.6f)
                {
                    visited.Add((nr, nc));
                    queue.Enqueue((nr, nc));
                }
            }
        }
    }

    // ===== HÀM BFS ĐỂ ĐO LƯỜNG CHÍNH XÁC (GIỮ NGUYÊN) =====
    static int? MinMovesClickAnywhere(string[,] board, string target, int maxMoves)
    {
        if (IsWin(board, target)) return 0;
        var queue = new Queue<(string state, int moves)>();
        var visited = new HashSet<string>();

        string startState = BoardToString(board);
        queue.Enqueue((startState, 0));
        visited.Add(startState);

        while (queue.Count > 0)
        {
            var (state, moves) = queue.Dequeue();
            if (moves >= maxMoves) continue;

            var currentBoard = StringToBoard(state);
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    string originalColor = currentBoard[r, c];
                    foreach (var color in COLORS)
                    {
                        if (originalColor == color) continue;
                        var newBoard = FloodFillFromCell(currentBoard, r, c, color);
                        if (IsWin(newBoard, target)) return moves + 1;

                        string newState = BoardToString(newBoard);
                        if (!visited.Contains(newState))
                        {
                            visited.Add(newState);
                            if (moves + 1 + EstimateRemainingMoves(newBoard, target) <= maxMoves)
                                queue.Enqueue((newState, moves + 1));
                        }
                    }
                }
            }
        }
        return null;
    }

    static string[,] FloodFillFromCell(string[,] board, int startR, int startC, string newColor)
    {
        var newBoard = (string[,])board.Clone();
        string originalColor = newBoard[startR, startC];
        if (originalColor == newColor) return newBoard;

        var stack = new Stack<(int r, int c)>();
        var visited = new bool[ROWS, COLS];
        stack.Push((startR, startC));

        while (stack.Count > 0)
        {
            var (r, c) = stack.Pop();
            if (r < 0 || r >= ROWS || c < 0 || c >= COLS) continue;
            if (visited[r, c]) continue;
            if (newBoard[r, c] != originalColor) continue;

            newBoard[r, c] = newColor;
            visited[r, c] = true;
            stack.Push((r + 1, c)); stack.Push((r - 1, c));
            stack.Push((r, c + 1)); stack.Push((r, c - 1));
        }
        return newBoard;
    }

    static int EstimateRemainingMoves(string[,] board, string target)
    {
        var colors = new HashSet<string>();
        for (int r = 0; r < ROWS; r++) for (int c = 0; c < COLS; c++) colors.Add(board[r, c]);
        return colors.Count == 1 && colors.Contains(target) ? 0 : colors.Count - 1;
    }

    static string BoardToString(string[,] board)
    {
        var sb = new System.Text.StringBuilder();
        for (int r = 0; r < ROWS; r++) for (int c = 0; c < COLS; c++) sb.Append(board[r, c]);
        return sb.ToString();
    }

    static string[,] StringToBoard(string state)
    {
        var board = new string[ROWS, COLS];
        for (int i = 0; i < state.Length; i++) board[i / COLS, i % COLS] = state[i].ToString();
        return board;
    }

    static bool IsWin(string[,] board, string target)
    {
        for (int r = 0; r < ROWS; r++) for (int c = 0; c < COLS; c++) if (board[r, c] != target) return false;
        return true;
    }
}