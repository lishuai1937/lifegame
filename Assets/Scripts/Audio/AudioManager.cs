using UnityEngine;

/// <summary>
/// 音频管理器
/// 根据年龄阶段切换BGM，管理音效播放
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM（按年龄阶段）")]
    public AudioClip ChildhoodBGM;
    public AudioClip YouthBGM;
    public AudioClip YoungBGM;
    public AudioClip PrimeBGM;
    public AudioClip MiddleBGM;
    public AudioClip ElderBGM;

    [Header("音效")]
    public AudioClip DiceRollSFX;
    public AudioClip InteractSFX;
    public AudioClip DeathSFX;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.volume = 0.5f;

        _sfxSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// 根据年龄阶段播放对应BGM
    /// </summary>
    public void PlayBGMForPhase(AgePhase phase)
    {
        AudioClip clip = phase switch
        {
            AgePhase.Childhood => ChildhoodBGM,
            AgePhase.Youth => YouthBGM,
            AgePhase.Young => YoungBGM,
            AgePhase.Prime => PrimeBGM,
            AgePhase.Middle => MiddleBGM,
            AgePhase.Elder => ElderBGM,
            _ => null
        };

        if (clip != null && _bgmSource.clip != clip)
        {
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) _sfxSource.PlayOneShot(clip);
    }
}
