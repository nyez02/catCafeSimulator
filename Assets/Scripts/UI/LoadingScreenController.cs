using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public Image progressFill;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;
    public Button playButton;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float fakeLoadDuration = 2.2f;
    public bool autoEnterGameWhenReady = true;

    private readonly List<string> cozyTips = new List<string>()
    {
        "🐾 Mẹo: Cho mèo ăn no sẽ giúp khách tip nhiều tiền gấp đôi!",
        "🐷 Mẹo: Nhớ đập Hũ Hụi Mèo Tài Lộc khi hũ đầy ắp tiền nhé!",
        "☕ Mẹo: Thuê Mèo Nhân Viên để tự động hoá việc phục vụ và pha chế.",
        "👑 Mẹo: Khách VIP hào phóng chi trả gấp 3 lần và mang lại nhiều EXP.",
        "🛋️ Mẹo: Mua nội thất Decor trong shop để nhận các buff nội tại vĩnh viễn.",
        "☁️ Mẹo: Nhấn 'Đồng bộ đám mây' trong Cài Đặt để lưu dữ liệu an toàn!",
        "🎨 Mẹo: Nhấn nút Theme để đổi không gian quán: Cozy Cafe, Lofi Đêm, Pastel!"
    };

    private void Start()
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(false);
            playButton.onClick.AddListener(EnterCafe);
        }

        if (tipText != null && cozyTips.Count > 0)
        {
            tipText.text = cozyTips[Random.Range(0, cozyTips.Count)];
        }

        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        float timer = 0f;
        int tipChangeCounter = 0;

        // Bắt đầu tải ngầm CafeScene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("CafeScene");
        asyncLoad.allowSceneActivation = false;

        while (timer < fakeLoadDuration || asyncLoad.progress < 0.89f)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / fakeLoadDuration);

            // Cập nhật thanh Slider
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            // Cập nhật Image Fill nếu dùng Image
            if (progressFill != null)
            {
                progressFill.fillAmount = progress;
            }

            // Cập nhật chữ phần trăm
            int percent = Mathf.RoundToInt(progress * 100f);
            if (progressText != null)
            {
                if (percent < 40)
                    progressText.text = $"Đang dọn dẹp quán cafe... {percent}%";
                else if (percent < 75)
                    progressText.text = $"Đang cho các chú mèo ăn... {percent}%";
                else if (percent < 99)
                    progressText.text = $"Đang mở cửa đón khách... {percent}%";
                else
                    progressText.text = "Quán cafe đã sẵn sàng! 100%";
            }

            // Đổi mẹo sau 1.2s
            if (timer > 1.2f && tipChangeCounter == 0 && tipText != null && cozyTips.Count > 1)
            {
                tipChangeCounter = 1;
                tipText.text = cozyTips[Random.Range(0, cozyTips.Count)];
            }

            yield return null;
        }

        if (progressText != null)
        {
            progressText.text = "Quán cafe đã sẵn sàng! 100%";
        }

        if (progressBar != null) progressBar.value = 1f;
        if (progressFill != null) progressFill.fillAmount = 1f;

        yield return new WaitForSecondsRealtime(0.4f);

        if (autoEnterGameWhenReady)
        {
            // Tự động mờ dần và vào game
            if (canvasGroup != null)
            {
                float fade = 0f;
                while (fade < 0.4f)
                {
                    fade += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, fade / 0.4f);
                    yield return null;
                }
            }
            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            // Hiện nút vào quán
            if (playButton != null)
            {
                playButton.gameObject.SetActive(true);
            }
        }
    }

    public void EnterCafe()
    {
        SceneManager.LoadScene("CafeScene");
    }
}
