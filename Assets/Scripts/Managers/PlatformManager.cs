using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance { get; private set; }

    [Header("Testing & Emulation")]
    public bool simulateMobileInEditor = true; // Cho phép thử nghiệm cảm ứng mobile ngay trong Unity Editor

    [Header("UI References to Toggle")]
    public GameObject mobileControlsRoot; // Chứa VirtualJoystick và các nút cảm ứng ngón cái
    public GameObject pcControlsHintRoot; // Chứa gợi ý phím WASD / Click chuột

    public bool IsMobile { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DetectPlatform();
    }

    private void Start()
    {
        ApplyPlatformUI();
    }

    private void DetectPlatform()
    {
#if UNITY_EDITOR
        IsMobile = simulateMobileInEditor;
#elif UNITY_ANDROID || UNITY_IOS
        IsMobile = true;
#else
        IsMobile = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
#endif
    }

    public void ApplyPlatformUI()
    {
        if (mobileControlsRoot != null)
        {
            mobileControlsRoot.SetActive(IsMobile);
        }

        if (pcControlsHintRoot != null)
        {
            pcControlsHintRoot.SetActive(!IsMobile);
        }

        // Đảm bảo Virtual Joystick được kích hoạt đúng
        if (VirtualJoystick.Instance != null)
        {
            VirtualJoystick.Instance.SetVisible(IsMobile);
        }
    }

    public void ToggleMobileSimulation(bool enableMobile)
    {
        IsMobile = enableMobile;
        ApplyPlatformUI();
    }
}
