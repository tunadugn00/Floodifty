using UnityEngine;

public static class SaveSystem
{
    private const string KEY_UNLOCKED_LEVEL = "UnlockedLevel";
    private const string KEY_VOLUME = "Volume";
    private const string KEY_MUSIC = "MusicEnabled";
    private const string KEY_SFX = "SFXEnabled";

    //=====Level======//
    public static int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(KEY_UNLOCKED_LEVEL, 1); //unlock level 1
    }

    public static void SetUnlockedLevel(int level)
    {
        int current = GetUnlockedLevel();
        if(level > current) 
        {
            PlayerPrefs.SetInt(KEY_UNLOCKED_LEVEL,level);
            PlayerPrefs.Save();
        }
    }

    //======Volume======//
    public static float GetVolume()
    {
        return PlayerPrefs.GetFloat(KEY_VOLUME, 1.0f); // 100%
    }
    public static void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat(KEY_VOLUME, volume);
        PlayerPrefs.Save();
    }
    
    public static bool IsMusicEnabled()
    {
        return PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
    }
    public static void SetMusicEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(KEY_MUSIC, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool IsSFXEnabled()
    {
        return PlayerPrefs.GetInt(KEY_SFX, 1) == 1;
    }
    public static void SetSFXEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(KEY_SFX, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
