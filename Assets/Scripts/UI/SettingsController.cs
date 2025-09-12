using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private GameObject musicCrossLine;
    [SerializeField] private GameObject sfxCrossLine;

    private void Start()
    {
        RefreshUI();
        SoundManager.Instance.PlayMusic();
    }

    public void ToggleMusic()
    {
        bool enabled = !SaveSystem.IsMusicEnabled();
        SaveSystem.SetMusicEnabled(enabled);
        SoundManager.Instance.PlayMusic();
        RefreshUI();
        SoundManager.Instance.PlayClick();
    }
    public void ToggleSFX()
    {
        bool enabled = !SaveSystem.IsSFXEnabled();
        SaveSystem.SetSFXEnabled(enabled);
        RefreshUI();
        SoundManager.Instance.PlayClick();
    }
    private void RefreshUI()
    {
        if (musicCrossLine) musicCrossLine.SetActive(!SaveSystem.IsMusicEnabled());
        if (sfxCrossLine) sfxCrossLine.SetActive(!SaveSystem.IsSFXEnabled());
    }
}
