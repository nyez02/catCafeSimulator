using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Virtual Joystick cho Mobile & Touchscreen.
/// Hỗ trợ kéo thả ngón tay/chuột mượt mà, tự động ẩn trên PC nếu muốn.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public static VirtualJoystick Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private RectTransform handleRect;

    [Header("Settings")]
    [SerializeField] private float handleRange = 70f;
    [SerializeField] private bool autoHideOnStandalone = false;

    private Vector2 inputVector = Vector2.zero;
    private Canvas parentCanvas;

    public Vector2 InputDirection => inputVector;
    public Vector2 Direction => inputVector;
    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;
    public bool IsActive => inputVector.sqrMagnitude > 0.001f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        parentCanvas = GetComponentInParent<Canvas>();
        if (backgroundRect == null) backgroundRect = GetComponent<RectTransform>();
        if (handleRect == null && transform.childCount > 0)
        {
            handleRect = transform.GetChild(0) as RectTransform;
        }

        // Tự động kiểm tra nếu là PC thuần thì có thể để mờ hoặc ẩn
        if (autoHideOnStandalone && Application.platform == RuntimePlatform.WindowsPlayer)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (backgroundRect == null || handleRect == null) return;

        Camera cam = (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? parentCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, eventData.position, cam, out Vector2 localPoint))
        {
            // Chuẩn hóa tọa độ theo kích thước background
            Vector2 size = backgroundRect.sizeDelta;
            float radius = handleRange > 0 ? handleRange : (size.x * 0.5f);

            inputVector = Vector2.ClampMagnitude(localPoint / radius, 1f);
            handleRect.anchoredPosition = inputVector * radius;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.zero;
        }
    }

    private void OnDisable()
    {
        inputVector = Vector2.zero;
        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.zero;
        }
    }
}
