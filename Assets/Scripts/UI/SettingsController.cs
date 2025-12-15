using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private RectTransform musicHandle;
    [SerializeField] private RectTransform sfxHandle;
    [SerializeField] private RectTransform vibrationHandle;

    [SerializeField] private Image musicHandleImage;
    [SerializeField] private Image sfxHandleImage;
    [SerializeField] private Image vibrationHandleImage;

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
    public void ToggleVibration()
    {
        bool enabled = !SaveSystem.IsVibrationEnabled();
        SaveSystem.SetVibrationEnabled(enabled);

        if (enabled) SoundManager.VibrateIfEnabled();
        UpdateVisuals(true);
        SoundManager.Instance.PlayClick();
    }

    private void UpdateVisuals(bool animate)
    {
        // 1. Music
        UpdateSingleToggle(musicHandle, musicHandleImage, SaveSystem.IsMusicEnabled(), animate);
        // 2. SFX
        UpdateSingleToggle(sfxHandle, sfxHandleImage, SaveSystem.IsSFXEnabled(), animate);
        // 3. Vibration
        UpdateSingleToggle(vibrationHandle, vibrationHandleImage, SaveSystem.IsVibrationEnabled(), animate);
    }

    private void UpdateSingleToggle(RectTransform handle, Image handleImg, bool isOn, bool animate)
    {
        if (handle == null) return;

        DOTween.Kill(handle);
        float targetX = isOn ? handleOnX : handleOffX;
        Color targetColor = isOn ? onColor : offColor;

        if (animate)
        {
            handle.DOAnchorPosX(targetX, moveDuration).SetEase(Ease.OutBack).SetUpdate(true); ;
            if (handleImg) handleImg.DOColor(targetColor, moveDuration).SetUpdate(true); ;
        }
        else
        {
            handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
            if (handleImg) handleImg.color = targetColor;
        }
    }
}
