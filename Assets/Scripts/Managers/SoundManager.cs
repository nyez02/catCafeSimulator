using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip coinSound;
    public AudioClip meowSound;
    public AudioClip clickSound;

    private AudioClip cachedMeowClip;
    private AudioClip cachedCoinClip;
    private AudioClip cachedBeepClip;
    private AudioClip cachedLofiBgmClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        PrecacheProceduralAudio();
    }

    private void Start()
    {
        PlayBGM();
    }

    private void PrecacheProceduralAudio()
    {
        // Khởi tạo trước và lưu bộ nhớ đệm (Cache) để tránh Memory Leak và GC Alloc
        if (meowSound == null && cachedMeowClip == null)
        {
            cachedMeowClip = GenerateRealisticCatMeowClip();
        }

        if (coinSound == null && cachedCoinClip == null)
        {
            cachedCoinClip = GenerateRealisticCoinChimeClip();
        }

        if (clickSound == null && cachedBeepClip == null)
        {
            cachedBeepClip = GenerateProceduralBeepClip(523.25f, 0.04f);
        }

        if (backgroundMusic == null && cachedLofiBgmClip == null)
        {
            cachedLofiBgmClip = GenerateProceduralLofiBGM();
        }
    }

    private const string PREFS_BGM_VOL = "Sound_BGM_Volume";
    private const string PREFS_SFX_VOL = "Sound_SFX_Volume";

    public float BGMVolume { get; private set; } = 0.5f;
    public float SFXVolume { get; private set; } = 0.8f;

    private void EnsureAudioSources()
    {
        BGMVolume = PlayerPrefs.GetFloat(PREFS_BGM_VOL, 0.5f);
        SFXVolume = PlayerPrefs.GetFloat(PREFS_SFX_VOL, 0.8f);

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
        bgmSource.volume = BGMVolume;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.volume = SFXVolume;
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = BGMVolume;
        }
        PlayerPrefs.SetFloat(PREFS_BGM_VOL, BGMVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = SFXVolume;
        }
        PlayerPrefs.SetFloat(PREFS_SFX_VOL, SFXVolume);
        PlayerPrefs.Save();
    }

    public void PlayBGM()
    {
        if (bgmSource == null) return;

        if (backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.Play();
        }
        else if (cachedLofiBgmClip != null)
        {
            bgmSource.clip = cachedLofiBgmClip;
            bgmSource.Play();
        }
    }

    public void PlayCoin()
    {
        if (coinSound != null)
        {
            sfxSource?.PlayOneShot(coinSound);
        }
        else if (cachedCoinClip != null)
        {
            sfxSource?.PlayOneShot(cachedCoinClip);
        }
    }

    public void PlayCatMeow()
    {
        if (meowSound != null)
        {
            sfxSource?.PlayOneShot(meowSound);
        }
        else if (cachedMeowClip != null)
        {
            sfxSource?.PlayOneShot(cachedMeowClip);
        }
    }

    public void PlayClick()
    {
        if (clickSound != null)
        {
            sfxSource?.PlayOneShot(clickSound);
        }
        else if (cachedBeepClip != null)
        {
            sfxSource?.PlayOneShot(cachedBeepClip);
        }
    }

    /// <summary>
    /// Thuật toán tổng hợp tiếng kêu mèo con 'Meo-w' tạo 1 lần duy nhất
    /// </summary>
    private AudioClip GenerateRealisticCatMeowClip()
    {
        int sampleRate = 44100;
        float duration = 0.55f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;

            float freq = (progress < 0.4f)
                ? Mathf.Lerp(420f, 760f, progress / 0.4f)
                : Mathf.Lerp(760f, 380f, (progress - 0.4f) / 0.6f);

            float vibrato = Mathf.Sin(2 * Mathf.PI * 6f * t) * 15f;
            freq += vibrato;

            phase += 2 * Mathf.PI * freq / sampleRate;

            float envelope = (progress < 0.15f) ? (progress / 0.15f) : ((progress < 0.7f) ? 1f : (1f - (progress - 0.7f) / 0.3f));

            float fundamental = Mathf.Sin(phase);
            float harmonic = Mathf.Sin(phase * 2f) * 0.35f;
            samples[i] = (fundamental + harmonic) * envelope * 0.32f;
        }

        AudioClip clip = AudioClip.Create("CachedMeowSFX", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Tiếng chuông vàng tạo 1 lần duy nhất
    /// </summary>
    private AudioClip GenerateRealisticCoinChimeClip()
    {
        int sampleRate = 44100;
        float duration = 0.38f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float f1 = 2093.00f; // C7
        float f2 = 2637.02f; // E7

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-7f * t);
            float tone1 = Mathf.Sin(2 * Mathf.PI * f1 * t);
            float tone2 = Mathf.Sin(2 * Mathf.PI * f2 * t) * 0.7f;
            samples[i] = (tone1 + tone2) * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("CachedCoinChimeSFX", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Tạo vòng lặp nhạc nền Lofi Cafe êm dịu 4 hợp âm (Cmaj7 - Am7 - Dm7 - G7)
    /// </summary>
    private AudioClip GenerateProceduralLofiBGM()
    {
        int sampleRate = 44100;
        float chordDuration = 2.5f;
        float totalDuration = chordDuration * 4f; // 10 giây loop
        int sampleCount = (int)(sampleRate * totalDuration);
        float[] samples = new float[sampleCount];

        float[][] chords = new float[][]
        {
            new float[] { 261.63f, 329.63f, 392.00f, 493.88f }, // Cmaj7
            new float[] { 220.00f, 261.63f, 329.63f, 392.00f }, // Am7
            new float[] { 293.66f, 349.23f, 440.00f, 523.25f }, // Dm7
            new float[] { 196.00f, 246.94f, 293.66f, 349.23f }  // G7
        };

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int chordIndex = Mathf.FloorToInt(t / chordDuration) % 4;
            float chordTime = t % chordDuration;
            float chordProgress = chordTime / chordDuration;

            float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(chordProgress));

            float chordSample = 0f;
            float[] notes = chords[chordIndex];
            foreach (float freq in notes)
            {
                chordSample += Mathf.Sin(2 * Mathf.PI * freq * chordTime) * 0.05f;
            }

            samples[i] = chordSample * envelope;
        }

        AudioClip bgmClip = AudioClip.Create("ProceduralLofiCafeBGM", sampleCount, 1, sampleRate, false);
        bgmClip.SetData(samples, 0);
        return bgmClip;
    }

    private AudioClip GenerateProceduralBeepClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(1f - (float)i / sampleCount);
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("CachedBeep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
