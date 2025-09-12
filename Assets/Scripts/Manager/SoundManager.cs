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
}
