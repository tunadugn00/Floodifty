using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Clip")]
    public AudioClip clickClip;
    public AudioClip fillClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip coinClip;

    public AudioClip hintClip;
    public AudioClip hammerClip;
    public AudioClip colorBombClip;
    public AudioClip shopPurchaseClip;
    public AudioClip adRewardClip;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick() => PlaySFX(clickClip);
    public void PlayFillClick() => PlaySFX(fillClip);
    public void PlayWin() => PlaySFX(winClip);
    public void PlayLose() => PlaySFX(loseClip);
    public void PlayCoin() => PlaySFX(coinClip);

    public void PlayHint() => PlaySFXOrFallbackClick(hintClip);
    public void PlayHammer() => PlaySFXOrFallbackClick(hammerClip);
    public void PlayColorBomb() => PlaySFXOrFallbackClick(colorBombClip);
    public void PlayShopPurchase() => PlaySFXOrFallbackClick(shopPurchaseClip);
    public void PlayAdReward() => PlaySFXOrFallbackClick(adRewardClip);

    private void PlaySFXOrFallbackClick(AudioClip clip)
    {
        if (clip != null) PlaySFX(clip);
        else PlayClick();
    }

    public void PlaySFX(AudioClip clip)
    {
        if(clip != null && sfxSource != null && SaveSystem.IsSFXEnabled())
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic()
    {
        if (musicSource == null) return;
        if (SaveSystem.IsMusicEnabled())
        {
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }

        }
    }

    public static void VibrateIfEnabled()
    {
        if (SaveSystem.IsVibrationEnabled())
        {
            Handheld.Vibrate();
        }
    }
}
