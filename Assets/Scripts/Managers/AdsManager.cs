using System;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Ads Settings")]
    public bool isAdsEnabled = true;
    public bool isTestMode = true;

    public event Action OnAdStarted;
    public event Action<bool> OnAdFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Hiển thị quảng cáo tặng thưởng (Rewarded Video)
    /// </summary>
    /// <param name="placementTag">Vị trí xem (x2_offline, fill_hui, free_food, etc.)</param>
    /// <param name="onRewardGranted">Callback khi xem xong quảng cáo</param>
    /// <param name="onAdFailed">Callback khi quảng cáo tải lỗi hoặc bị bỏ qua</param>
    public void ShowRewardedVideo(string placementTag, Action onRewardGranted, Action onAdFailed = null)
    {
        if (!isAdsEnabled)
        {
            Debug.LogWarning("[AdsManager] Quảng cáo hiện đang tắt.");
            onAdFailed?.Invoke();
            return;
        }

        Debug.Log($"[AdsManager] Đang phát video quảng cáo: {placementTag}");
        OnAdStarted?.Invoke();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClick();
        }

        // Mô phỏng xem quảng cáo (Trong Unity Editor / thiết bị thử nghiệm)
        // Hiển thị thông báo nhẹ và trao thưởng sau 0.5s
        if (UIManager.Instance != null)
        {
            Camera cam = Camera.main;
            Vector3 promptPos = cam != null ? cam.transform.position + cam.transform.forward * 2f : Vector3.zero;
            UIManager.Instance.ShowFloatingText("🎬 Đang xem quảng cáo...", promptPos, Color.yellow);
        }

        // Kích hoạt nhận thưởng sau khi quảng cáo kết thúc
        InvokeReward(onRewardGranted);
    }

    private void InvokeReward(Action onRewardGranted)
    {
        onRewardGranted?.Invoke();
        OnAdFinished?.Invoke(true);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoin();
        }

        Debug.Log("[AdsManager] Người chơi đã xem xong quảng cáo và nhận thưởng thành công!");
    }
}
