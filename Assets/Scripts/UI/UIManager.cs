using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Top HUD - Currencies & Progress")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI catCountText;
    public TextMeshProUGUI tableCountText;

    [Header("Shift & Rush Hour HUD")]
    public TextMeshProUGUI shiftTimerText;
    public GameObject rushHourBanner;

    [Header("Lucky Piggy Bank (Hũ Hụi) HUD")]
    public TextMeshProUGUI huiBalanceText;
    public Image huiFillImage;
    public Button breakHuiButton;

    [Header("Cross-Platform & Mobile Controls")]
    public SafeAreaFitter safeAreaFitter;
    public VirtualJoystick virtualJoystick;
    public GameObject mobileActionButtonsGroup;

    [Header("Offline Earnings Popup (Thu Nhập Vắng Nhà)")]
    public GameObject offlineEarningsPanel;
    public TextMeshProUGUI offlineEarningsText;
    public TextMeshProUGUI offlineTimeText;
    private float cachedOfflineMoney = 0f;

    [Header("Tutorial Box (Hướng Dẫn Tân Thủ)")]
    public GameObject tutorialBox;
    public TextMeshProUGUI tutorialInstructionText;

    [Header("Modals & Panels")]
    public GameObject shopPanel;
    public GameObject pausePanel;
    public GameObject shiftSummaryPanel;
    public CatAlbumUI catAlbumUI;

    [Header("Shop References")]
    public TextMeshProUGUI unlockTablePriceText;

    [Header("Summary Popup References")]
    public TextMeshProUGUI summaryStarsText;
    public TextMeshProUGUI summaryRevenueText;
    public TextMeshProUGUI summaryGemsText;

    [Header("World Space Prefabs")]
    public GameObject floatingTextPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 1. Lắng nghe Tiền & Kim Cương
        if (MoneyManager.Instance != null)
        {
            UpdateMoneyUI(MoneyManager.Instance.CurrentMoney);
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyUI;
        }

        if (GameManager.Instance != null)
        {
            UpdateGemsUI(GameManager.Instance.pawGems);
            GameManager.Instance.OnGemsChanged += UpdateGemsUI;
        }

        // 2. Lắng nghe Mèo & Bàn
        if (CatManager.Instance != null)
        {
            UpdateCatCountUI();
            CatManager.Instance.OnCatCountChanged += UpdateCatCountUI;
        }

        if (TableManager.Instance != null)
        {
            UpdateTableCountUI();
            TableManager.Instance.OnTableUpdated += UpdateTableCountUI;
        }

        // 3. Lắng nghe Hũ Hụi Mèo Tài Lộc
        if (LuckyPiggyBank.Instance != null)
        {
            UpdateHuiUI(LuckyPiggyBank.Instance.currentBalance, LuckyPiggyBank.Instance.maxCapacity);
            LuckyPiggyBank.Instance.OnHuiBalanceChanged += UpdateHuiUI;
        }

        // 4. Lắng nghe Ca làm việc
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnShiftTimeUpdated += UpdateShiftTimerUI;
            LevelManager.Instance.OnRushHourToggled += SetRushHourBanner;
            LevelManager.Instance.OnShiftCompleted += ShowShiftSummary;
            UpdateLevelUI();
        }

        // 5. Lắng nghe Hướng dẫn tân thủ
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialStepChanged += UpdateTutorialUI;
            TutorialManager.Instance.OnTutorialFinished += CloseTutorialUI;
            if (TutorialManager.Instance.isTutorialActive)
            {
                UpdateTutorialUI(TutorialManager.Instance.currentStep, "Chào mừng bạn! Hãy bắt đầu làm quen với quán nhé.");
            }
        }

        // 6. Cấu hình giao diện di động
        ConfigureMobileControls();

        // Đóng các panel mặc định
        if (shopPanel != null) shopPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (shiftSummaryPanel != null) shiftSummaryPanel.SetActive(false);
        if (rushHourBanner != null) rushHourBanner.SetActive(false);
        if (offlineEarningsPanel != null) offlineEarningsPanel.SetActive(false);
    }

    private void ConfigureMobileControls()
    {
        bool isMobile = false;
        if (PlatformManager.Instance != null)
        {
            isMobile = PlatformManager.Instance.IsMobile;
        }
        else
        {
            isMobile = Application.isMobilePlatform;
        }

        if (virtualJoystick != null)
        {
            virtualJoystick.SetVisible(isMobile);
        }

        if (mobileActionButtonsGroup != null)
        {
            mobileActionButtonsGroup.SetActive(isMobile);
        }

        if (safeAreaFitter != null)
        {
            safeAreaFitter.ApplySafeArea();
        }
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
        if (GameManager.Instance != null) GameManager.Instance.OnGemsChanged -= UpdateGemsUI;
        if (CatManager.Instance != null) CatManager.Instance.OnCatCountChanged -= UpdateCatCountUI;
        if (TableManager.Instance != null) TableManager.Instance.OnTableUpdated -= UpdateTableCountUI;
        if (LuckyPiggyBank.Instance != null) LuckyPiggyBank.Instance.OnHuiBalanceChanged -= UpdateHuiUI;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnShiftTimeUpdated -= UpdateShiftTimerUI;
            LevelManager.Instance.OnRushHourToggled -= SetRushHourBanner;
            LevelManager.Instance.OnShiftCompleted -= ShowShiftSummary;
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialStepChanged -= UpdateTutorialUI;
            TutorialManager.Instance.OnTutorialFinished -= CloseTutorialUI;
        }
    }

    // --- Offline Idle Earnings Popup ---

    public void ShowOfflineEarningsPopup(float earnedAmount, string timeElapsedStr)
    {
        cachedOfflineMoney = earnedAmount;

        if (offlineEarningsPanel != null)
        {
            offlineEarningsPanel.SetActive(true);

            if (offlineEarningsText != null)
            {
                offlineEarningsText.text = $"+${earnedAmount:0.00}";
            }

            if (offlineTimeText != null)
            {
                offlineTimeText.text = $"Trong {timeElapsedStr} bạn vắng nhà, các bé mèo đã phục vụ khách chăm chỉ!";
            }
        }
    }

    public void OnClaimOfflineEarningsButtonClicked()
    {
        if (cachedOfflineMoney > 0)
        {
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(cachedOfflineMoney);
            }

            if (LuckyPiggyBank.Instance != null)
            {
                LuckyPiggyBank.Instance.AddTipToHui(cachedOfflineMoney * 0.15f);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCoin();
            }

            ShowFloatingText($"Đã nhận +${cachedOfflineMoney:0.00}!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.green);
            cachedOfflineMoney = 0f;
        }

        if (offlineEarningsPanel != null)
        {
            offlineEarningsPanel.SetActive(false);
        }
    }

    // --- Tutorial UI ---

    public void UpdateTutorialUI(TutorialStep step, string message)
    {
        if (tutorialBox != null)
        {
            tutorialBox.SetActive(step != TutorialStep.Completed && step != TutorialStep.NotStarted);
        }

        if (tutorialInstructionText != null)
        {
            tutorialInstructionText.text = message;
        }
    }

    public void CloseTutorialUI()
    {
        if (tutorialBox != null)
        {
            tutorialBox.SetActive(false);
        }
    }

    // --- HUD Updaters ---

    public void UpdateMoneyUI(float currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${currentMoney:0.00}";
        }
    }

    public void UpdateGemsUI(int gems)
    {
        if (gemsText != null)
        {
            gemsText.text = $"🐾 {gems}";
        }
    }

    public void UpdateCatCountUI()
    {
        if (catCountText != null && CatManager.Instance != null)
        {
            catCountText.text = $"Mèo: {CatManager.Instance.GetCatCount()}";
        }
    }

    public void UpdateTableCountUI()
    {
        if (tableCountText != null && TableManager.Instance != null)
        {
            tableCountText.text = $"Bàn: {TableManager.Instance.GetUnlockedTableCount()}/{TableManager.Instance.GetTotalTableCount()}";
        }
    }

    public void UpdateLevelUI()
    {
        if (levelText != null && LevelManager.Instance != null)
        {
            levelText.text = $"Lv.{LevelManager.Instance.cafeLevel}";
        }
    }

    public void UpdateHuiUI(float balance, float capacity)
    {
        if (huiBalanceText != null)
        {
            huiBalanceText.text = $"Hũ: ${balance:0}/${capacity:0}";
        }

        if (huiFillImage != null && capacity > 0)
        {
            huiFillImage.fillAmount = Mathf.Clamp01(balance / capacity);
        }
    }

    public void UpdateShiftTimerUI(float current, float total)
    {
        if (shiftTimerText != null)
        {
            float remaining = Mathf.Max(0, total - current);
            int minutes = Mathf.FloorToInt(remaining / 60F);
            int seconds = Mathf.FloorToInt(remaining % 60F);
            shiftTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void SetRushHourBanner(bool active)
    {
        if (rushHourBanner != null)
        {
            rushHourBanner.SetActive(active);
        }
    }

    // --- Buttons & Interactivity ---

    public void OnBreakHuiButtonClicked()
    {
        if (LuckyPiggyBank.Instance != null)
        {
            LuckyPiggyBank.Instance.BreakHuiBank();
        }
    }

    public void OnServeCoffeeButtonClicked()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(15f);
            ShowFloatingText("+$15", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.green);
        }

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.AddProgress("serve_10", 1);
            AchievementManager.Instance.AddProgress("serve_50", 1);
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnCoffeeServed();
        }
    }

    public void OnMobileQuickPetButtonClicked()
    {
        if (CatManager.Instance != null)
        {
            var cats = CatManager.Instance.GetAllCats();
            if (cats.Count > 0)
            {
                CatAI nearestCat = cats[0];
                nearestCat.Pet();
                AchievementManager.Instance?.AddProgress("pet_10", 1);
                AchievementManager.Instance?.AddProgress("pet_50", 1);
            }
        }
    }

    public void OnFeedAllCatsButtonClicked()
    {
        if (CatManager.Instance != null)
        {
            CatManager.Instance.FeedAllCats();
            ShowFloatingText("Đã cho tất cả mèo ăn! 🐟", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.cyan);
        }
    }

    public void OnToggleShopButtonClicked()
    {
        if (shopPanel != null)
        {
            bool isOpen = !shopPanel.activeSelf;
            shopPanel.SetActive(isOpen);
            UpdateShopPrices();

            if (isOpen && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnShopOpened();
            }
        }
    }

    public void OnToggleCatAlbumButtonClicked()
    {
        if (catAlbumUI != null)
        {
            catAlbumUI.ToggleAlbum();
        }
    }

    public void OnPauseButtonClicked()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            GameManager.Instance?.PauseGame();
        }
    }

    public void OnResumeButtonClicked()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            GameManager.Instance?.ResumeGame();
        }
    }

    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f;
        SaveManager.Instance?.SaveGame();
        SceneManager.LoadScene("MainMenu");
    }

    // --- Shift Summary Popup ---

    public void ShowShiftSummary(int stars, float revenue)
    {
        if (shiftSummaryPanel != null)
        {
            shiftSummaryPanel.SetActive(true);

            if (summaryStarsText != null)
            {
                string starsDisplay = "";
                for (int i = 0; i < stars; i++) starsDisplay += "⭐ ";
                if (stars == 0) starsDisplay = "Chưa đạt sao nào :(";
                summaryStarsText.text = starsDisplay;
            }

            if (summaryRevenueText != null)
            {
                summaryRevenueText.text = $"Doanh Thu Ca: ${revenue:0.00}";
            }

            if (summaryGemsText != null && LevelManager.Instance != null && LevelManager.Instance.currentLevel != null)
            {
                int gemsEarned = stars > 0 ? LevelManager.Instance.currentLevel.completionGems : 0;
                summaryGemsText.text = $"+{gemsEarned} 🐾 Kim Cương";
            }
        }
    }

    public void OnRestartShiftButtonClicked()
    {
        if (shiftSummaryPanel != null) shiftSummaryPanel.SetActive(false);
        if (LevelManager.Instance != null && LevelManager.Instance.currentLevel != null)
        {
            LevelManager.Instance.StartShift(LevelManager.Instance.currentLevel);
        }
    }

    // --- Shop Actions ---

    public void OnBuyTableButtonClicked()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.BuyTableUnlock();
            UpdateShopPrices();
            AchievementManager.Instance?.AddProgress("tables_4", 1);
        }
    }

    public void OnBuyCatButtonClicked()
    {
        if (ShopManager.Instance != null && CatManager.Instance != null && CatManager.Instance.availableBreeds.Count > 0)
        {
            CatData breed = CatManager.Instance.availableBreeds[0];
            ShopManager.Instance.BuyCat(breed);
        }
    }

    public void OnBuyPremiumFoodButtonClicked()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.BuyPremiumCatFood();
        }
    }

    private void UpdateShopPrices()
    {
        if (unlockTablePriceText != null && ShopManager.Instance != null)
        {
            unlockTablePriceText.text = $"Mở Bàn Mới (${ShopManager.Instance.tableUnlockCost})";
        }
    }

    public void ShowFloatingText(string message, Vector3 worldPosition, Color color)
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.GetFloatingText(message, worldPosition, color);
            return;
        }

        GameObject textObj;
        if (floatingTextPrefab != null)
        {
            textObj = Instantiate(floatingTextPrefab, worldPosition, Quaternion.identity);
        }
        else
        {
            textObj = new GameObject("FloatingTextInstance");
            textObj.transform.position = worldPosition;
        }

        FloatingText ft = textObj.GetComponent<FloatingText>();
        if (ft == null)
        {
            ft = textObj.AddComponent<FloatingText>();
        }

        ft.Setup(message, color);
    }
}
