using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;
    public CanvasGroup canvasGroup;

    private readonly List<string> cozyTips = new List<string>()
    {
        "🐾 Mẹo: Cho mèo ăn no sẽ giúp khách tip nhiều tiền gấp đôi!",
        "🐷 Mẹo: Nhớ đập Hũ Hụi Mèo Tài Lộc khi hũ đầy ắp tiền nhé!",
        "☕ Mẹo: Thuê Mèo Nhân Viên để tự động hoá việc phục vụ và pha chế.",
        "👑 Mẹo: Khách VIP hào phóng chi trả gấp 3 lần và mang lại nhiều EXP.",
        "🛋️ Mẹo: Mua nội thất Decor trong shop để nhận các buff nội tại vĩnh viễn.",
        "☁️ Mẹo: Nhấn 'Đồng bộ đám mây' trong Cài Đặt để không bao giờ mất dữ liệu!",
        "🎨 Mẹo: Nhấn nút Theme để đổi không gian quán: Cozy Cafe, Lofi Đêm, Pastel Hoa Anh Đào!"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureUIComponents();
    }

    private void EnsureUIComponents()
    {
        if (loadingPanel == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var cObj = new GameObject("LoadingCanvas");
                canvas = cObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 998;
                cObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cObj.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(cObj);
            }

            GameObject panel = new GameObject("LoadingScreenPanel");
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.11f, 0.16f, 1f);

            canvasGroup = panel.AddComponent<CanvasGroup>();

            // Title
            GameObject titleObj = new GameObject("LoadingTitle");
            titleObj.transform.SetParent(panel.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🐱 TIỆM CÀ PHÊ MÈO COZY";
            titleText.fontSize = 28f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(1f, 0.85f, 0.5f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

            // Tip
            GameObject tipObj = new GameObject("LoadingTip");
            tipObj.transform.SetParent(panel.transform, false);
            tipText = tipObj.AddComponent<TextMeshProUGUI>();
            tipText.fontSize = 18f;
            tipText.color = Color.white;
            tipText.alignment = TextAlignmentOptions.Center;
            tipObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            tipObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 60);

            // Progress text
            GameObject progObj = new GameObject("ProgressText");
            progObj.transform.SetParent(panel.transform, false);
            progressText = progObj.AddComponent<TextMeshProUGUI>();
            progressText.fontSize = 20f;
            progressText.color = new Color(1f, 0.9f, 0.3f);
            progressText.alignment = TextAlignmentOptions.Center;
            progObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -90);

            loadingPanel = panel;
            loadingPanel.SetActive(false);
        }
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.AsyncLoadRoutine(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator AsyncLoadRoutine(string sceneName)
    {
        EnsureUIComponents();
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            // Chọn ngẫu nhiên 1 lời khuyên dễ thương
            if (tipText != null && cozyTips.Count > 0)
            {
                tipText.text = cozyTips[Random.Range(0, cozyTips.Count)];
            }
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (!op.isDone)
        {
            // Tiến trình tải cảnh (0.0 -> 0.9 trong Unity)
            float realProgress = Mathf.Clamp01(op.progress / 0.9f);
            fakeProgress = Mathf.MoveTowards(fakeProgress, realProgress, Time.unscaledDeltaTime * 1.5f);

            if (progressBar != null) progressBar.value = fakeProgress;
            if (progressText != null) progressText.text = $"Đang mở cửa tiệm... {Mathf.RoundToInt(fakeProgress * 100)}%";

            if (fakeProgress >= 0.99f)
            {
                yield return new WaitForSecondsRealtime(0.3f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        // Fade out
        if (canvasGroup != null)
        {
            float fadeElapsed = 0f;
            while (fadeElapsed < 0.4f)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / 0.4f);
                yield return null;
            }
        }

        if (loadingPanel != null) loadingPanel.SetActive(false);
    }
}
