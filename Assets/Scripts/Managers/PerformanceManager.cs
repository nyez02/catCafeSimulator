using UnityEngine;
using UnityEngine.Rendering;

public enum GraphicsTier
{
    Low,
    Medium,
    High
}

public class PerformanceManager : MonoBehaviour
{
    public static PerformanceManager Instance { get; private set; }

    [Header("Frame Rate & Sync")]
    public int targetFPS = 60;
    public bool showFPSCounter = false;

    [Header("Current Preset")]
    public GraphicsTier currentTier = GraphicsTier.Medium;

    private float fpsTimer;
    private int frameCount;
    private float currentFps;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyPerformanceSettings();
    }

    private void ApplyPerformanceSettings()
    {
        // 1. Khóa tốc độ khung hình ở 60 FPS để game chuyển động mượt mà và không hao pin quá mức
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;

        // 2. Tối ưu thời gian xử lý vật lý
        Time.fixedDeltaTime = 0.02f; // 50hz chuẩn cho vật lý, nhẹ cho CPU

        // 3. Tối ưu theo cấu hình thiết bị
        if (Application.isMobilePlatform)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep; // Không tắt màn hình khi đang chơi
            SetGraphicsQuality(GraphicsTier.Medium);
        }
        else
        {
            SetGraphicsQuality(GraphicsTier.High);
        }
    }

    private void Update()
    {
        if (showFPSCounter)
        {
            frameCount++;
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer >= 0.5f)
            {
                currentFps = frameCount / fpsTimer;
                frameCount = 0;
                fpsTimer = 0f;
            }
        }
    }

    public void SetGraphicsQuality(GraphicsTier tier)
    {
        currentTier = tier;

        switch (tier)
        {
            case GraphicsTier.Low:
                // Máy yếu / Tiết kiệm pin: Tắt bóng động
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 20f;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.globalTextureMipmapLimit = 1; // Giảm nhẹ độ phân giải texture để tiết kiệm VRAM
                break;

            case GraphicsTier.Medium:
                // Cân bằng cho đa số điện thoại và laptop
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.shadowDistance = 35f;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                QualitySettings.globalTextureMipmapLimit = 0;
                break;

            case GraphicsTier.High:
                // PC mạnh: Đồ họa tối đa
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.shadowDistance = 50f;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.globalTextureMipmapLimit = 0;
                break;
        }

        Debug.Log($"[PerformanceManager] Graphics Quality set to: {tier}");
    }

    private void OnGUI()
    {
        if (showFPSCounter)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 22;
            style.normal.textColor = currentFps >= 55 ? Color.green : (currentFps >= 30 ? Color.yellow : Color.red);
            GUI.Label(new Rect(20, 20, 200, 40), $"FPS: {currentFps:0.0}", style);
        }
    }
}
