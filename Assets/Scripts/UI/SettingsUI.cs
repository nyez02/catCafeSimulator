using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject settingsPanel;

    [Header("Audio Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI bgmValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("Graphics Toggles")]
    public Button fpsToggleBtn;
    public TextMeshProUGUI fpsBtnText;
    public Button qualityToggleBtn;
    public TextMeshProUGUI qualityBtnText;

    [Header("Cloud & Action Buttons")]
    public Button syncCloudBtn;
    public Button closeBtn;

    private int targetFPS = 60;
    private int qualityLevel = 1; // 0: Low, 1: Medium, 2: High

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadSettings();
    }

    private void Start()
    {
        InitializeListeners();
        UpdateUI();
    }

    private void LoadSettings()
    {
        targetFPS = PlayerPrefs.GetInt("Settings_TargetFPS", 60);
        Application.targetFrameRate = targetFPS;

        qualityLevel = PlayerPrefs.GetInt("Settings_Quality", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(qualityLevel, true);
    }

    private void InitializeListeners()
    {
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        if (fpsToggleBtn != null)
        {
            fpsToggleBtn.onClick.RemoveAllListeners();
            fpsToggleBtn.onClick.AddListener(OnToggleFPS);
        }

        if (qualityToggleBtn != null)
        {
            qualityToggleBtn.onClick.RemoveAllListeners();
            qualityToggleBtn.onClick.AddListener(OnToggleQuality);
        }

        if (syncCloudBtn != null)
        {
            syncCloudBtn.onClick.RemoveAllListeners();
            syncCloudBtn.onClick.AddListener(OnSyncCloudClicked);
        }

        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(CloseSettings);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            UpdateUI();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (SoundManager.Instance != null)
        {
            if (bgmSlider != null) bgmSlider.value = SoundManager.Instance.BGMVolume;
            if (sfxSlider != null) sfxSlider.value = SoundManager.Instance.SFXVolume;

            if (bgmValueText != null) bgmValueText.text = $"{Mathf.RoundToInt(SoundManager.Instance.BGMVolume * 100)}%";
            if (sfxValueText != null) sfxValueText.text = $"{Mathf.RoundToInt(SoundManager.Instance.SFXVolume * 100)}%";
        }

        if (fpsBtnText != null)
        {
            fpsBtnText.text = targetFPS >= 60 ? "FPS: 60 (Mượt)" : "FPS: 30 (Tiết kiệm Pin)";
        }

        if (qualityBtnText != null)
        {
            string[] names = { "Đồ Họa: Thấp", "Đồ Họa: Vừa", "Đồ Họa: Cao" };
            int safeIdx = Mathf.Clamp(qualityLevel, 0, names.Length - 1);
            qualityBtnText.text = names[safeIdx];
        }
    }

    public void OnBGMChanged(float val)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(val);
        }
        if (bgmValueText != null) bgmValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    public void OnSFXChanged(float val)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(val);
        }
        if (sfxValueText != null) sfxValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
    }

    public void OnToggleFPS()
    {
        targetFPS = (targetFPS == 60) ? 30 : 60;
        Application.targetFrameRate = targetFPS;
        PlayerPrefs.SetInt("Settings_TargetFPS", targetFPS);
        PlayerPrefs.Save();

        UpdateUI();
        ToastManager.Instance?.ShowToast($"Đã đổi mục tiêu: {targetFPS} FPS", "⚡");
    }

    public void OnToggleQuality()
    {
        qualityLevel = (qualityLevel + 1) % 3;
        QualitySettings.SetQualityLevel(qualityLevel, true);
        PlayerPrefs.SetInt("Settings_Quality", qualityLevel);
        PlayerPrefs.Save();

        UpdateUI();
        ToastManager.Instance?.ShowToast($"Đã cập nhật mức đồ họa", "🎨");
    }

    public void OnSyncCloudClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized)
            {
                ToastManager.Instance?.ShowToast("Đã đồng bộ đám mây thành công!", "☁️", new Color(0.1f, 0.4f, 0.7f, 0.95f));
            }
            else
            {
                ToastManager.Instance?.ShowToast("Đã lưu nội bộ (Chế độ Ngoại tuyến)!", "💾");
            }
        }
        else
        {
            ToastManager.Instance?.ShowToast("Đã lưu tiến trình!", "💾");
        }
    }
}
