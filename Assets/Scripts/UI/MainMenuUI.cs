using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject dailyRewardPanel;
    public GameObject levelSelectPanel;

    [Header("Daily Reward References")]
    public TextMeshProUGUI streakText;
    public TextMeshProUGUI todayRewardText;
    public Button claimDailyButton;

    [Header("Audio Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        CloseAllPanels();

        if (bgmSlider != null && SoundManager.Instance != null && SoundManager.Instance.bgmSource != null)
        {
            bgmSlider.value = SoundManager.Instance.bgmSource.volume;
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null && SoundManager.Instance != null && SoundManager.Instance.sfxSource != null)
        {
            sfxSlider.value = SoundManager.Instance.sfxSource.volume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    public void OnPlayButtonClicked()
    {
        SoundManager.Instance?.PlayClick();
        SceneManager.LoadScene("CafeScene");
    }

    public void OnOpenLevelSelectClicked()
    {
        SoundManager.Instance?.PlayClick();
        CloseAllPanels();
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    public void OnOpenDailyRewardClicked()
    {
        SoundManager.Instance?.PlayClick();
        CloseAllPanels();
        if (dailyRewardPanel != null)
        {
            dailyRewardPanel.SetActive(true);
            RefreshDailyRewardUI();
        }
    }

    public void OnClaimDailyRewardClicked()
    {
        if (DailyRewardManager.Instance != null)
        {
            DailyRewardManager.Instance.ClaimDailyReward();
            RefreshDailyRewardUI();
        }
    }

    private void RefreshDailyRewardUI()
    {
        if (DailyRewardManager.Instance != null)
        {
            if (streakText != null)
            {
                streakText.text = $"Chuỗi Đăng Nhập: Ngày {DailyRewardManager.Instance.currentStreak + 1}/7";
            }

            var reward = DailyRewardManager.Instance.GetRewardForDay(DailyRewardManager.Instance.currentStreak);
            if (todayRewardText != null && reward != null)
            {
                todayRewardText.text = $"Phần Thưởng: {reward.rewardName}";
            }

            if (claimDailyButton != null)
            {
                claimDailyButton.interactable = DailyRewardManager.Instance.CanClaimToday();
            }
        }
    }

    public void OnOpenSettingsClicked()
    {
        SoundManager.Instance?.PlayClick();
        CloseAllPanels();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnBGMVolumeChanged(float val)
    {
        if (SoundManager.Instance != null && SoundManager.Instance.bgmSource != null)
        {
            SoundManager.Instance.bgmSource.volume = val;
        }
    }

    public void OnSFXVolumeChanged(float val)
    {
        if (SoundManager.Instance != null && SoundManager.Instance.sfxSource != null)
        {
            SoundManager.Instance.sfxSource.volume = val;
        }
    }

    public void CloseAllPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (dailyRewardPanel != null) dailyRewardPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    public void OnExitGameClicked()
    {
        SoundManager.Instance?.PlayClick();
        SaveManager.Instance?.SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
