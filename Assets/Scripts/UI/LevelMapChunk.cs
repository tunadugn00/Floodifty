using UnityEngine;

public class LevelMapChunk : MonoBehaviour
{
    public LevelButton[] buttonInThisChunk;

    public void SetupChunk(int startLevel, int unlockdLevel)
    {
        for ( int i = 0; i< buttonInThisChunk.Length; i++ )
        {
            if (buttonInThisChunk[i] == null) continue;

            var btn = buttonInThisChunk[i];
            int currentLevel = startLevel + i;

            // unlockedLevel
            bool isBeaten = currentLevel < unlockdLevel;
            bool isNextPlayable = currentLevel == unlockdLevel;
            //( Level 6,7,8... có cả 2 bool này => false => isLocked = true)

            btn.Setup(currentLevel, isBeaten, isNextPlayable);
        }
    }
}
