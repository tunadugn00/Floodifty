using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData data;
    private GUIStyle cellStyle;

    private void OnEnable()
    {
        data = (LevelData)target;

    }

    public override void OnInspectorGUI()
    {
        if (cellStyle == null)
        {
            cellStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };
        }

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rows"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cols"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("movesAllowed"));
        EditorGUILayout.Space();

        data.EnsureSize();
        EditorGUILayout.LabelField("Level Layout", EditorStyles.boldLabel);

        for (int r = data.rows - 1; r >= 0; r--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < data.cols; c++)
            {
                var color = data.Get(r, c);
                Color guiColor = GetColorForTile(color);

                GUI.backgroundColor = guiColor;
                if (GUILayout.Button(color.ToString().Substring(0, 1), cellStyle, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    int next = ((int)color + 1) % System.Enum.GetValues(typeof(Tile.TileColor)).Length;
                    data.Set(r, c, (Tile.TileColor)next);
                    EditorUtility.SetDirty(data);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }


    private Color GetColorForTile(Tile.TileColor tileColor)
    {
        switch(tileColor)
        {
            case Tile.TileColor.Red: return Color.red;
            case Tile.TileColor.Green: return Color.green;
            case Tile.TileColor.Blue: return Color.blue;
            case Tile.TileColor.Yellow: return Color.yellow;
        }
        return Color.white;
    }
}
