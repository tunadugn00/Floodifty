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

            // Sử dụng Newtonsoft.Json thay vì JsonUtility
            var levels = ParseJsonManually(json);

            if (levels == null || levels.Count == 0)
            {
                Debug.LogError("❌ Không parse được levels từ JSON");
                return;
            }

            string savePath = "Assets/Levels";
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            // Tìm số level hiện có
            string[] existing = Directory.GetFiles(savePath, "Level_*.asset");
            int startIndex = existing.Length + 1;

            for (int i = 0; i < levels.Count; i++)
            {
                var src = levels[i];
                var so = ScriptableObject.CreateInstance<LevelData>();
                so.rows = src.rows;
                so.cols = src.cols;
                so.movesAllowed = src.movesAllowed;
                so.targetColor = ParseColor(src.targetColor);
                so.layout = Flatten(src.layout, so.rows, so.cols);

                string assetName = $"Level_{startIndex + i}.asset";
                AssetDatabase.CreateAsset(so, Path.Combine(savePath, assetName));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✅ Imported {levels.Count} levels, starting from Level_{startIndex}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Import failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Parse JSON manually vì JsonUtility không support jagged arrays
    static List<LevelDataJson> ParseJsonManually(string json)
    {
        try
        {
            // Remove whitespace và newlines
            json = json.Trim();

            if (!json.StartsWith("[") || !json.EndsWith("]"))
            {
                Debug.LogError("JSON không phải là array");
                return null;
            }

            var levels = new List<LevelDataJson>();

            // Tìm các object level
            int startIndex = 1; // Skip opening [
            int braceCount = 0;
            int objectStart = -1;

            for (int i = startIndex; i < json.Length - 1; i++) // Skip closing ]
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

            // Parse basic fields
            level.rows = ExtractIntValue(objectJson, "rows");
            level.cols = ExtractIntValue(objectJson, "cols");
            level.movesAllowed = ExtractIntValue(objectJson, "movesAllowed");
            level.targetColor = ExtractStringValue(objectJson, "targetColor");

            // Parse layout array
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
        // Tìm "layout": [...]
        int layoutStart = json.IndexOf("\"layout\":");
        if (layoutStart == -1) return null;

        int arrayStart = json.IndexOf('[', layoutStart);
        if (arrayStart == -1) return null;

        int arrayEnd = FindMatchingCloseBracket(json, arrayStart);
        if (arrayEnd == -1) return null;

        string layoutJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

        // Parse rows
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

    static Tile.TileColor[] Flatten(string[][] arr, int rows, int cols)
    {
        if (arr == null)
        {
            Debug.LogError("Layout array is null!");
            return new Tile.TileColor[rows * cols]; // Return default array
        }

        var flat = new Tile.TileColor[rows * cols];
        for (int r = 0; r < rows && r < arr.Length; r++)
        {
            if (arr[r] == null)
            {
                Debug.LogWarning($"Row {r} is null, skipping");
                continue;
            }

            for (int c = 0; c < cols && c < arr[r].Length; c++)
            {
                flat[r * cols + c] = ParseColor(arr[r][c]);
            }
        }
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
