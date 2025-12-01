using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private RectTransform musicHandle;
    [SerializeField] private RectTransform sfxHandle;

    [SerializeField] private Image musicHandleImage;
    [SerializeField] private Image sfxHandleImage;

    [SerializeField] private float moveDuration = 0.2f; 
    [SerializeField] private float handleOnX = 35f; 
    [SerializeField] private float handleOffX = -35f;

    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.white;

    private void Start()
    {
        UpdateVisuals(false);
        SoundManager.Instance.PlayMusic();
    }

    public void ToggleMusic()
    {
        bool enabled = !SaveSystem.IsMusicEnabled();
        SaveSystem.SetMusicEnabled(enabled);
        SoundManager.Instance.PlayMusic();
        UpdateVisuals(true);
        SoundManager.Instance.PlayClick();
    }
    public void ToggleSFX()
    {
        bool enabled = !SaveSystem.IsSFXEnabled();
        SaveSystem.SetSFXEnabled(enabled);
        UpdateVisuals(true);
        SoundManager.Instance.PlayClick();
    }
    private void UpdateVisuals(bool animate)
    {
        // --- Music ---
        bool musicOn = SaveSystem.IsMusicEnabled();
        float targetMusicX = musicOn ? handleOnX : handleOffX;
        Color targetMusicColor = musicOn ? onColor : offColor;

        if (animate)
        {
            musicHandle.DOAnchorPosX(targetMusicX, moveDuration).SetEase(Ease.OutBack);
            // Hiệu ứng đổi màu 
            if (musicHandleImage) musicHandleImage.DOColor(targetMusicColor, moveDuration);
        }
        else
        {
            musicHandle.anchoredPosition = new Vector2(targetMusicX, musicHandle.anchoredPosition.y);
            if (musicHandleImage) musicHandleImage.color = targetMusicColor;
        }

        // --- SFX ---
        bool sfxOn = SaveSystem.IsSFXEnabled();
        float targetSfxX = sfxOn ? handleOnX : handleOffX;
        Color targetSfxColor = sfxOn ? onColor : offColor;

        if (animate)
        {
            sfxHandle.DOAnchorPosX(targetSfxX, moveDuration).SetEase(Ease.OutBack);
            if (sfxHandleImage) sfxHandleImage.DOColor(targetSfxColor, moveDuration);
        }
        else
        {
            sfxHandle.anchoredPosition = new Vector2(targetSfxX, sfxHandle.anchoredPosition.y);
            if (sfxHandleImage) sfxHandleImage.color = targetSfxColor;
        }
    }
}
