using UnityEngine;

[CreateAssetMenu(fileName ="LeveDatabase", menuName = "Floodify/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public LevelData[] levels;

    public int TotalLevels => levels.Length;
    public LevelData GetLevel(int index)
    {
        if (index < 1 || index > levels.Length) return null;
        return levels[index -1];
    }
}
