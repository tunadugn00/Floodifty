using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class JsonToSOImporter
{
    [MenuItem("Floodify/Import Levels From JSON")]
    public static void Import()
    {
        string path = Path.Combine(Application.dataPath, "Scripts/Data/levels.json");
        if (!File.Exists(path))
        {
            Debug.LogError("❌ Không tìm thấy file " + path);
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            Debug.Log("📄 JSON content preview: " + json.Substring(0, Mathf.Min(200, json.Length)) + "...");

            var levels = ParseJsonManually(json);

            if (levels == null || levels.Count == 0)
            {
                Debug.LogError("❌ Không parse được levels từ JSON");
                return;
            }

            string savePath = "Assets/Levels";
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            // Tìm tất cả số level hiện có
            string[] existing = Directory.GetFiles(savePath, "Level_*.asset");
            var existingNumbers = new HashSet<int>();

            foreach (var file in existing)
            {
                var filename = System.IO.Path.GetFileNameWithoutExtension(file);
                if (filename.StartsWith("Level_"))
                {
                    string numberPart = filename.Substring(6);
                    if (int.TryParse(numberPart, out int num))
                    {
                        existingNumbers.Add(num);
                    }
                }
            }

            Debug.Log($"📊 Đã có {existingNumbers.Count} levels: {string.Join(", ", existingNumbers.OrderBy(x => x))}");

            // Tìm các số trống để lấp vào
            var availableSlots = new List<int>();
            int maxLevel = existingNumbers.Count > 0 ? existingNumbers.Max() : 0;

            for (int i = 1; i <= maxLevel; i++)
            {
                if (!existingNumbers.Contains(i))
                {
                    availableSlots.Add(i);
                }
            }

            Debug.Log($"🔍 Tìm thấy {availableSlots.Count} khoảng trống: {string.Join(", ", availableSlots)}");

            for (int i = 0; i < levels.Count; i++)
            {
                var src = levels[i];
                var so = ScriptableObject.CreateInstance<LevelData>();
                so.rows = src.rows;
                so.cols = src.cols;
                so.movesAllowed = src.movesAllowed;
                so.targetColor = ParseColor(src.targetColor);
                so.layout = Flatten(src.layout, so.rows, so.cols);

                int levelNumber;

                if (availableSlots.Count > 0)
                {
                    levelNumber = availableSlots[0];
                    availableSlots.RemoveAt(0);
                    Debug.Log($"📝 Lấp khoảng trống: Level_{levelNumber}");
                }
                else
                {
                    levelNumber = maxLevel + 1;
                    maxLevel++;
                    Debug.Log($"➕ Chèn tiếp: Level_{levelNumber}");
                }

                string assetName = $"Level_{levelNumber}.asset";
                AssetDatabase.CreateAsset(so, Path.Combine(savePath, assetName));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Import failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    static List<LevelDataJson> ParseJsonManually(string json)
    {
        try
        {
            json = json.Trim();

            if (!json.StartsWith("[") || !json.EndsWith("]"))
            {
                Debug.LogError("JSON không phải là array");
                return null;
            }

            var levels = new List<LevelDataJson>();
            int startIndex = 1;
            int braceCount = 0;
            int objectStart = -1;

            for (int i = startIndex; i < json.Length - 1; i++)
            {
                char c = json[i];

                if (c == '{')
                {
                    if (braceCount == 0)
                        objectStart = i;
                    braceCount++;
                }
                else if (c == '}')
                {
                    braceCount--;
                    if (braceCount == 0 && objectStart >= 0)
                    {
                        string objectJson = json.Substring(objectStart, i - objectStart + 1);
                        var level = ParseSingleLevel(objectJson);
                        if (level != null)
                            levels.Add(level);
                        objectStart = -1;
                    }
                }
            }

            return levels;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Parse JSON failed: {ex.Message}");
            return null;
        }
    }

    static LevelDataJson ParseSingleLevel(string objectJson)
    {
        try
        {
            var level = new LevelDataJson();
            level.rows = ExtractIntValue(objectJson, "rows");
            level.cols = ExtractIntValue(objectJson, "cols");
            level.movesAllowed = ExtractIntValue(objectJson, "movesAllowed");
            level.targetColor = ExtractStringValue(objectJson, "targetColor");
            level.layout = ParseLayout(objectJson);
            return level;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Parse single level failed: {ex.Message}");
            return null;
        }
    }

    static int ExtractIntValue(string json, string key)
    {
        string pattern = $"\"{key}\":\\s*(\\d+)";
        var match = System.Text.RegularExpressions.Regex.Match(json, pattern);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    static string ExtractStringValue(string json, string key)
    {
        string pattern = $"\"{key}\":\\s*\"([^\"]+)\"";
        var match = System.Text.RegularExpressions.Regex.Match(json, pattern);
        return match.Success ? match.Groups[1].Value : "";
    }

    static string[][] ParseLayout(string json)
    {
        int layoutStart = json.IndexOf("\"layout\":");
        if (layoutStart == -1) return null;

        int arrayStart = json.IndexOf('[', layoutStart);
        if (arrayStart == -1) return null;

        int arrayEnd = FindMatchingCloseBracket(json, arrayStart);
        if (arrayEnd == -1) return null;

        string layoutJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

        var rows = new List<string[]>();
        int rowStart = 0;

        while (rowStart < layoutJson.Length)
        {
            int nextRowStart = layoutJson.IndexOf('[', rowStart);
            if (nextRowStart == -1) break;

            int rowEnd = FindMatchingCloseBracket(layoutJson, nextRowStart);
            if (rowEnd == -1) break;

            string rowJson = layoutJson.Substring(nextRowStart + 1, rowEnd - nextRowStart - 1);
            var rowData = ParseRow(rowJson);
            if (rowData != null)
                rows.Add(rowData);

            rowStart = rowEnd + 1;
        }

        return rows.ToArray();
    }

    static string[] ParseRow(string rowJson)
    {
        var items = new List<string>();
        var matches = System.Text.RegularExpressions.Regex.Matches(rowJson, "\"([^\"]+)\"");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            items.Add(match.Groups[1].Value);
        }

        return items.ToArray();
    }

    static int FindMatchingCloseBracket(string text, int openIndex)
    {
        if (openIndex >= text.Length || text[openIndex] != '[') return -1;

        int count = 1;
        for (int i = openIndex + 1; i < text.Length; i++)
        {
            if (text[i] == '[') count++;
            else if (text[i] == ']') count--;

            if (count == 0) return i;
        }
        return -1;
    }

    static Tile.TileColor ParseColor(string s)
    {
        return s switch
        {
            "R" => Tile.TileColor.Red,
            "G" => Tile.TileColor.Green,
            "B" => Tile.TileColor.Blue,
            "Y" => Tile.TileColor.Yellow,
            "Rock" => Tile.TileColor.Rock,
            _ => Tile.TileColor.Red
        };
    }

    // ===== SỬA: ĐẢO NGƯỢC ROW KHI FLATTEN =====
    static Tile.TileColor[] Flatten(string[][] arr, int rows, int cols)
    {
        if (arr == null)
        {
            Debug.LogError("Layout array is null!");
            return new Tile.TileColor[rows * cols];
        }

        var flat = new Tile.TileColor[rows * cols];

        for (int r = 0; r < rows && r < arr.Length; r++)
        {
            if (arr[r] == null)
            {
                Debug.LogWarning($"Row {r} is null, skipping");
                continue;
            }

            // ĐẢO NGƯỢC: JSON row 0 (top) → Unity row (rows-1) (bottom)
            int unityRow = rows - 1 - r;

            for (int c = 0; c < cols && c < arr[r].Length; c++)
            {
                flat[unityRow * cols + c] = ParseColor(arr[r][c]);
            }
        }

        Debug.Log($"✅ Flattened với flip vertical: JSON row 0 → Unity row {rows - 1}");
        return flat;
    }

    [System.Serializable]
    public class LevelDataJson
    {
        public int rows;
        public int cols;
        public string targetColor;
        public int movesAllowed;
        public string[][] layout;
    }
}