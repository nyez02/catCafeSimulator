using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CatCafe.Social;

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
    public LeaderboardUI leaderboardUI;
    public DecorationShopUI decorationShopUI;
    public DailyQuestUI dailyQuestUI;
    public StaffShopUI staffShopUI;
    public SettingsUI settingsUI;

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
        EnsureModernHUD();

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

            Vector3 promptPos = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2f : Vector3.zero;
            ShowFloatingText($"Đã nhận +${cachedOfflineMoney:0.00}!", promptPos, Color.green);
            cachedOfflineMoney = 0f;
        }

        if (offlineEarningsPanel != null)
        {
            offlineEarningsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Người chơi xem video quảng cáo nhận thưởng để X2 thu nhập vắng nhà
    /// </summary>
    public void OnClaimDoubleOfflineEarningsWithAdClicked()
    {
        if (cachedOfflineMoney <= 0)
        {
            if (offlineEarningsPanel != null) offlineEarningsPanel.SetActive(false);
            return;
        }

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedVideo("x2_offline_earnings", () =>
            {
                float doubleAmount = cachedOfflineMoney * 2f;
                if (MoneyManager.Instance != null)
                {
                    MoneyManager.Instance.AddMoney(doubleAmount);
                }

                if (LuckyPiggyBank.Instance != null)
                {
                    LuckyPiggyBank.Instance.AddTipToHui(doubleAmount * 0.20f);
                }

                ShowFloatingText($"🎬 X2 THƯỞNG: +${doubleAmount:0.00}!", Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2f : Vector3.zero, Color.yellow);
                cachedOfflineMoney = 0f;

                if (offlineEarningsPanel != null)
                {
                    offlineEarningsPanel.SetActive(false);
                }
            });
        }
        else
        {
            // Dự phòng nếu AdsManager chưa gắn
            OnClaimOfflineEarningsButtonClicked();
        }
    }

    /// <summary>
    /// Xem quảng cáo để bơm thêm 50% tiền vào Hũ Hụi Mèo Tài Lộc
    /// </summary>
    public void OnWatchAdForHuiBoostClicked()
    {
        if (LuckyPiggyBank.Instance == null) return;

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedVideo("hui_boost", () =>
            {
                float boostAmount = LuckyPiggyBank.Instance.maxCapacity * 0.5f;
                LuckyPiggyBank.Instance.AddTipToHui(boostAmount);
                ShowFloatingText($"🐷 Đã nạp +${boostAmount:0} vào Hũ Hụi!", Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2f : Vector3.zero, Color.cyan);
            });
        }
    }

    /// <summary>
    /// Xem quảng cáo để lập tức cho toàn bộ mèo ăn no 100% và vui vẻ 100%
    /// </summary>
    public void OnWatchAdForFreeCatFeastClicked()
    {
        if (CatManager.Instance == null) return;

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedVideo("free_cat_feast", () =>
            {
                CatManager.Instance.FeedAllCats();
                foreach (var cat in CatManager.Instance.GetAllCats())
                {
                    if (cat != null)
                    {
                        cat.hunger = 100f;
                        cat.happiness = 100f;
                    }
                }
                ShowFloatingText("🐾 Bữa Tiệc Mèo Hoàng Gia: Mèo no nê & Hạnh phúc 100%!", Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2f : Vector3.zero, Color.yellow);
            });
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
            Vector3 promptPos = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2.5f : Vector3.zero;
            ShowFloatingText("+$15", promptPos, Color.green);
        }

        LevelManager.Instance?.AddExp(10);
        DailyQuestManager.Instance?.AddQuestProgress("quest_serve", 1);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoin();
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
            Vector3 promptPos = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2.5f : Vector3.zero;
            ShowFloatingText("Đã cho tất cả mèo ăn! 🐟", promptPos, Color.cyan);
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

    public void OnOpenLeaderboardButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        if (leaderboardUI != null)
        {
            leaderboardUI.OpenLeaderboard();
        }
        else if (LeaderboardUI.Instance != null)
        {
            LeaderboardUI.Instance.OpenLeaderboard();
        }
    }

    public void OnToggleDecorationShopButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        if (decorationShopUI != null)
        {
            decorationShopUI.OpenShop();
        }
        else if (DecorationShopUI.Instance != null)
        {
            DecorationShopUI.Instance.OpenShop();
        }
    }

    public void OnToggleDailyQuestButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        if (dailyQuestUI != null)
        {
            dailyQuestUI.OpenQuestPanel();
        }
        else if (DailyQuestUI.Instance != null)
        {
            DailyQuestUI.Instance.OpenQuestPanel();
        }
    }

    public void OnToggleThemeButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        if (UIThemeManager.Instance != null)
        {
            UIThemeManager.Instance.CycleNextTheme();
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

    public void OnToggleStaffShopButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        if (staffShopUI != null)
        {
            staffShopUI.OpenShop();
        }
        else if (StaffShopUI.Instance != null)
        {
            StaffShopUI.Instance.OpenShop();
        }
    }

    public void OnOpenSettingsButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        if (settingsUI != null)
        {
            settingsUI.OpenSettings();
        }
        else if (SettingsUI.Instance != null)
        {
            SettingsUI.Instance.OpenSettings();
        }
    }

    public void OnOpenFriendsButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        SocialUI.Show("friends");
    }

    public void OnOpenChatButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        SocialUI.Show("chat");
    }

    public void OnOpenMailboxButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        SocialUI.Show("mailbox");
    }

    public void OnOpenMapSelectionButtonClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        CatCafe.UI.MapSelectionUI.Show();
    }

    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f;
        SaveManager.Instance?.SaveGame();
        LoadingScreenUI.LoadScene("MainMenu");
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

    /// <summary>
    /// Tự động kiểm tra và khởi tạo toàn bộ HUD hiện đại (Glassmorphism Top Bar, Right Action Bar, Bottom Widget) nếu thiếu
    /// </summary>
    public void EnsureModernHUD()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 1. Chuẩn hóa CanvasScaler theo chuẩn 1920x1080 Responsive
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 2. Thu gọn và làm đẹp nút "Phục vụ Cafe"
        Transform serveBtnTrans = canvas.transform.Find("Phục vụ Cafe");
        if (serveBtnTrans != null)
        {
            RectTransform rt = serveBtnTrans.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 35f);
                rt.sizeDelta = new Vector2(260f, 68f);
            }
            Image btnImg = serveBtnTrans.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.color = new Color(0.98f, 0.42f, 0.54f, 1f); // Hồng đào tươi sáng
            }
            TextMeshProUGUI btnTxt = serveBtnTrans.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTxt != null)
            {
                btnTxt.text = "☕ Phục Vụ Cafe";
                btnTxt.fontSize = 26;
                btnTxt.fontStyle = FontStyles.Bold;
                btnTxt.color = Color.white;
            }
            Button btn = serveBtnTrans.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnServeCoffeeButtonClicked);
                btn.onClick.AddListener(OnServeCoffeeButtonClicked);
            }
        }

        // Nút Chạy Nước Rút (Sprint Button) cho Mobile / Touch
        Transform existingSprintBtn = canvas.transform.Find("Sprint_Button");
        if (existingSprintBtn == null)
        {
            GameObject sprintObj = new GameObject("Sprint_Button");
            sprintObj.transform.SetParent(canvas.transform, false);

            RectTransform srt = sprintObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0f);
            srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.anchoredPosition = new Vector2(195f, 35f);
            srt.sizeDelta = new Vector2(110f, 68f);

            Image simg = sprintObj.AddComponent<Image>();
            simg.color = new Color(0.95f, 0.55f, 0.2f, 0.95f);
            sprintObj.AddComponent<SprintButton>();

            GameObject stxtObj = new GameObject("Text");
            stxtObj.transform.SetParent(sprintObj.transform, false);
            RectTransform stxtRt = stxtObj.AddComponent<RectTransform>();
            stxtRt.anchorMin = Vector2.zero;
            stxtRt.anchorMax = Vector2.one;
            stxtRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI stxt = stxtObj.AddComponent<TextMeshProUGUI>();
            stxt.text = "⚡ Chạy";
            stxt.fontSize = 22;
            stxt.fontStyle = FontStyles.Bold;
            stxt.color = Color.white;
            stxt.alignment = TextAlignmentOptions.Center;
        }

        // 3. Xây dựng Top HUD Glassmorphism nếu chưa có
        Transform existingTopHUD = canvas.transform.Find("TopHUD_Bar");
        if (existingTopHUD == null)
        {
            GameObject topHUDObj = new GameObject("TopHUD_Bar");
            topHUDObj.transform.SetParent(canvas.transform, false);

            RectTransform topRt = topHUDObj.AddComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0.02f, 1f);
            topRt.anchorMax = new Vector2(0.98f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = new Vector2(0f, -15f);
            topRt.sizeDelta = new Vector2(0f, 65f);

            Image topBg = topHUDObj.AddComponent<Image>();
            topBg.color = new Color(0.10f, 0.12f, 0.16f, 0.85f); // Glassmorphism Dark

            HorizontalLayoutGroup hlg = topHUDObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Xử lý di dời hoặc tạo mới moneyText
            if (moneyText == null)
            {
                Transform oldMoney = canvas.transform.Find("MoneyText");
                if (oldMoney != null)
                {
                    moneyText = oldMoney.GetComponent<TextMeshProUGUI>();
                    oldMoney.SetParent(topHUDObj.transform, false);
                }
            }
            if (moneyText == null)
            {
                moneyText = CreateHUDPill(topHUDObj.transform, "MoneyPill", "💰 $0.00", new Color(0.98f, 0.80f, 0.30f));
            }
            else
            {
                moneyText.transform.SetParent(topHUDObj.transform, false);
                StyleExistingTMP(moneyText, "💰 $0.00", new Color(0.98f, 0.80f, 0.30f));
            }

            if (gemsText == null) gemsText = CreateHUDPill(topHUDObj.transform, "GemsPill", "💎 0 Kim Cương", new Color(0.4f, 0.78f, 1f));
            if (levelText == null) levelText = CreateHUDPill(topHUDObj.transform, "LevelPill", "☕ Cấp 1", new Color(1f, 0.65f, 0.38f));
            if (catCountText == null) catCountText = CreateHUDPill(topHUDObj.transform, "CatsPill", "🐱 0 Mèo", new Color(0.92f, 0.52f, 0.88f));
            if (tableCountText == null) tableCountText = CreateHUDPill(topHUDObj.transform, "TablesPill", "🪑 0 Bàn", new Color(0.55f, 0.88f, 0.55f));
            if (shiftTimerText == null) shiftTimerText = CreateHUDPill(topHUDObj.transform, "TimerPill", "⏰ 00:00", new Color(1f, 0.45f, 0.45f));
        }

        // 4. Xây dựng Right Action Menu (Thanh nút tính năng bên phải)
        Transform existingRightMenu = canvas.transform.Find("RightActionBar");
        if (existingRightMenu == null)
        {
            GameObject rightMenuObj = new GameObject("RightActionBar");
            rightMenuObj.transform.SetParent(canvas.transform, false);

            RectTransform rmRt = rightMenuObj.AddComponent<RectTransform>();
            rmRt.anchorMin = new Vector2(1f, 0.5f);
            rmRt.anchorMax = new Vector2(1f, 0.5f);
            rmRt.pivot = new Vector2(1f, 0.5f);
            rmRt.anchoredPosition = new Vector2(-20f, 20f);
            rmRt.sizeDelta = new Vector2(195f, 560f);

            VerticalLayoutGroup vlg = rightMenuObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.MiddleRight;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            CreateActionButton(rightMenuObj.transform, "🗺️ Đổi Màn / Map", OnOpenMapSelectionButtonClicked, new Color(0.35f, 0.72f, 0.60f, 0.95f));
            CreateActionButton(rightMenuObj.transform, "👥 Bạn Bè", OnOpenFriendsButtonClicked, new Color(0.28f, 0.65f, 0.95f, 0.95f));
            CreateActionButton(rightMenuObj.transform, "💬 Trò Chuyện", OnOpenChatButtonClicked, new Color(0.2f, 0.82f, 0.65f, 0.95f));
            CreateActionButton(rightMenuObj.transform, "📫 Hòm Thư", OnOpenMailboxButtonClicked, new Color(0.95f, 0.45f, 0.35f, 0.95f));
            CreateActionButton(rightMenuObj.transform, "🏆 Bảng Xếp Hạng", OnOpenLeaderboardButtonClicked, new Color(0.95f, 0.65f, 0.15f, 0.9f));
            CreateActionButton(rightMenuObj.transform, "📜 Nhiệm Vụ", OnToggleDailyQuestButtonClicked, new Color(0.2f, 0.6f, 0.9f, 0.9f));
            CreateActionButton(rightMenuObj.transform, "🛋️ Cửa Hàng Decor", OnToggleDecorationShopButtonClicked, new Color(0.7f, 0.4f, 0.85f, 0.9f));
            CreateActionButton(rightMenuObj.transform, "👔 Nhân Viên", OnToggleStaffShopButtonClicked, new Color(0.3f, 0.75f, 0.5f, 0.9f));
            CreateActionButton(rightMenuObj.transform, "🎨 Đổi Giao Diện", OnToggleThemeButtonClicked, new Color(0.9f, 0.4f, 0.6f, 0.9f));
            CreateActionButton(rightMenuObj.transform, "⚙️ Cài Đặt", OnOpenSettingsButtonClicked, new Color(0.4f, 0.5f, 0.6f, 0.9f));
            CreateActionButton(rightMenuObj.transform, "⏸️ Tạm Dừng", OnPauseButtonClicked, new Color(0.85f, 0.3f, 0.3f, 0.9f));
        }

        // 5. Xây dựng Virtual Joystick cảm ứng cho Mobile/Touch
        if (FindFirstObjectByType<VirtualJoystick>() == null)
        {
            GameObject joyObj = new GameObject("Virtual_Joystick");
            joyObj.transform.SetParent(canvas.transform, false);

            RectTransform joyRt = joyObj.AddComponent<RectTransform>();
            joyRt.anchorMin = new Vector2(0f, 0f);
            joyRt.anchorMax = new Vector2(0f, 0f);
            joyRt.pivot = new Vector2(0.5f, 0.5f);
            joyRt.anchoredPosition = new Vector2(130f, 210f);
            joyRt.sizeDelta = new Vector2(130f, 130f);

            Image joyBg = joyObj.AddComponent<Image>();
            joyBg.color = new Color(0.12f, 0.15f, 0.24f, 0.55f);

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(joyObj.transform, false);
            RectTransform hRt = handleObj.AddComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(60f, 60f);
            Image hImg = handleObj.AddComponent<Image>();
            hImg.color = new Color(0.98f, 0.85f, 0.4f, 0.85f);

            virtualJoystick = joyObj.AddComponent<VirtualJoystick>();
        }

        // 5. Xây dựng Widget Hũ Hụi Mèo Tài Lộc (Góc dưới bên trái)
        Transform existingHui = canvas.transform.Find("PiggyBank_Widget");
        if (existingHui == null)
        {
            GameObject huiObj = new GameObject("PiggyBank_Widget");
            huiObj.transform.SetParent(canvas.transform, false);

            RectTransform huiRt = huiObj.AddComponent<RectTransform>();
            huiRt.anchorMin = new Vector2(0f, 0f);
            huiRt.anchorMax = new Vector2(0f, 0f);
            huiRt.pivot = new Vector2(0f, 0f);
            huiRt.anchoredPosition = new Vector2(25f, 25f);
            huiRt.sizeDelta = new Vector2(230f, 95f);

            Image huiBg = huiObj.AddComponent<Image>();
            huiBg.color = new Color(0.12f, 0.14f, 0.18f, 0.88f);

            VerticalLayoutGroup vlg = huiObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 8, 8);
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Tiêu đề hũ
            GameObject titleObj = new GameObject("HuiTitle");
            titleObj.transform.SetParent(huiObj.transform, false);
            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "🏺 Hũ Hụi Mèo Tài Lộc";
            titleTxt.fontSize = 15;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = new Color(1f, 0.85f, 0.4f);
            titleTxt.alignment = TextAlignmentOptions.Center;

            // Số dư hũ
            GameObject balObj = new GameObject("HuiBalance");
            balObj.transform.SetParent(huiObj.transform, false);
            huiBalanceText = balObj.AddComponent<TextMeshProUGUI>();
            huiBalanceText.text = "$0.00 / $150.00";
            huiBalanceText.fontSize = 14;
            huiBalanceText.color = Color.white;
            huiBalanceText.alignment = TextAlignmentOptions.Center;

            // Nút đập hũ
            GameObject breakBtnObj = new GameObject("BreakHuiButton");
            breakBtnObj.transform.SetParent(huiObj.transform, false);
            RectTransform breakRt = breakBtnObj.AddComponent<RectTransform>();
            breakRt.sizeDelta = new Vector2(0f, 30f);
            Image breakImg = breakBtnObj.AddComponent<Image>();
            breakImg.color = new Color(0.92f, 0.35f, 0.25f, 0.95f);
            breakHuiButton = breakBtnObj.AddComponent<Button>();
            breakHuiButton.onClick.AddListener(OnBreakHuiButtonClicked);

            GameObject breakTxtObj = new GameObject("Text");
            breakTxtObj.transform.SetParent(breakBtnObj.transform, false);
            TextMeshProUGUI breakTxt = breakTxtObj.AddComponent<TextMeshProUGUI>();
            breakTxt.text = "💥 ĐẬP HŨ NHẬN THƯỞNG";
            breakTxt.fontSize = 13;
            breakTxt.fontStyle = FontStyles.Bold;
            breakTxt.color = Color.white;
            breakTxt.alignment = TextAlignmentOptions.Center;
            RectTransform btRt = breakTxtObj.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.sizeDelta = Vector2.zero;
        }
    }

    private TextMeshProUGUI CreateHUDPill(Transform parent, string name, string defaultText, Color textColor)
    {
        GameObject pill = new GameObject(name);
        pill.transform.SetParent(parent, false);

        RectTransform rt = pill.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(165f, 46f);

        Image bg = pill.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.22f, 0.28f, 0.85f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(pill.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    private void StyleExistingTMP(TextMeshProUGUI tmp, string defaultText, Color textColor)
    {
        if (tmp == null) return;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = tmp.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(165f, 46f);
        }
    }

    private void CreateActionButton(Transform parent, string title, UnityEngine.Events.UnityAction action, Color btnColor)
    {
        GameObject btnObj = new GameObject("Btn_" + title);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(175f, 45f);

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(action);

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = title;
        tmp.fontSize = 15;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }
}
