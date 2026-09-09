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
    }

    private void Start()
    {
        PlayBGM();
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = 0.35f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = 0.75f;
        }
    }

    public void PlayBGM()
    {
        if (backgroundMusic != null && bgmSource != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.Play();
        }
        else if (bgmSource != null)
        {
            // Tự động tạo bản nhạc nền Lofi Cafe thư giãn
            bgmSource.clip = GenerateProceduralLofiBGM();
            bgmSource.Play();
        }
    }

    public void PlayCoin()
    {
        if (coinSound != null)
        {
            sfxSource?.PlayOneShot(coinSound);
        }
        else
        {
            // Âm thanh chuông vàng 2 tầng sóng ngân vang trong trẻo
            PlayRealisticCoinChime();
        }
    }

    public void PlayCatMeow()
    {
        if (meowSound != null)
        {
            sfxSource?.PlayOneShot(meowSound);
        }
        else
        {
            // Thuật toán tổng hợp tiếng mèo meow uốn lượn cao độ chân thật
            PlayRealisticCatMeow();
        }
    }

    public void PlayClick()
    {
        if (clickSound != null)
        {
            sfxSource?.PlayOneShot(clickSound);
        }
        else
        {
            PlayProceduralBeep(523.25f, 0.04f); // Note C5 click
        }
    }

    /// <summary>
    /// Thuật toán mô phỏng tiếng kêu mèo con 'Meo-w' với dải tần uốn lượn tự nhiên
    /// </summary>
    private void PlayRealisticCatMeow()
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

            // Đường cong cao độ: 420Hz -> 760Hz -> 380Hz
            float freq;
            if (progress < 0.4f)
            {
                freq = Mathf.Lerp(420f, 760f, progress / 0.4f);
            }
            else
            {
                freq = Mathf.Lerp(760f, 380f, (progress - 0.4f) / 0.6f);
            }

            // Rung giọng mèo nhẹ (vibrato 6Hz)
            float vibrato = Mathf.Sin(2 * Mathf.PI * 6f * t) * 15f;
            freq += vibrato;

            // Tích hợp pha tần số
            phase += 2 * Mathf.PI * freq / sampleRate;

            // Khung biên độ mượt (attack - sustain - decay)
            float envelope;
            if (progress < 0.15f)
            {
                envelope = progress / 0.15f;
            }
            else if (progress < 0.7f)
            {
                envelope = 1f;
            }
            else
            {
                envelope = 1f - (progress - 0.7f) / 0.3f;
            }

            // Hài âm formant để giọng kêu ấm
            float fundamental = Mathf.Sin(phase);
            float harmonic = Mathf.Sin(phase * 2f) * 0.35f;
            samples[i] = (fundamental + harmonic) * envelope * 0.32f;
        }

        AudioClip clip = AudioClip.Create("MeowSFX", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        sfxSource?.PlayOneShot(clip);
    }

    /// <summary>
    /// Tiếng chuông vàng 2 nốt hòa âm ngân vang
    /// </summary>
    private void PlayRealisticCoinChime()
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
            float envelope = Mathf.Exp(-7f * t); // Decay hàm mũ ngân vang
            float tone1 = Mathf.Sin(2 * Mathf.PI * f1 * t);
            float tone2 = Mathf.Sin(2 * Mathf.PI * f2 * t) * 0.7f;
            samples[i] = (tone1 + tone2) * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("CoinChimeSFX", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        sfxSource?.PlayOneShot(clip);
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

        // Tần số các nốt trong 4 hợp âm Lofi ấm áp
        float[][] chords = new float[][]
        {
            new float[] { 261.63f, 329.63f, 392.00f, 493.88f }, // Cmaj7 (C4, E4, G4, B4)
            new float[] { 220.00f, 261.63f, 329.63f, 392.00f }, // Am7 (A3, C4, E4, G4)
            new float[] { 293.66f, 349.23f, 440.00f, 523.25f }, // Dm7 (D4, F4, A4, C5)
            new float[] { 196.00f, 246.94f, 293.66f, 349.23f }  // G7 (G3, B3, D4, F4)
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

    private void PlayProceduralBeep(float frequency, float duration)
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

        AudioClip clip = AudioClip.Create("Beep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        sfxSource?.PlayOneShot(clip);
    }
}
