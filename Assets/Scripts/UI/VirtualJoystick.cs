using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static VirtualJoystick Instance { get; private set; }

    [Header("Joystick Visual Components")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;

    [Header("Settings")]
    public float handleRange = 65f;
    public bool isFloatingJoystick = false; // Tự động nhảy tới vị trí ngón tay chạm

    private Vector2 inputVector = Vector2.zero;
    private Canvas parentCanvas;
    private Vector2 defaultBackgroundPosition;

    public Vector2 Direction => inputVector;
    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        parentCanvas = GetComponentInParent<Canvas>();
        if (joystickBackground != null)
        {
            defaultBackgroundPosition = joystickBackground.anchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isFloatingJoystick && joystickBackground != null && parentCanvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            joystickBackground.anchoredPosition = localPoint;
        }

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (joystickBackground == null || joystickHandle == null) return;

        Camera cam = (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera) 
            ? parentCanvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            cam,
            out Vector2 position))
        {
            // Chuẩn hóa vị trí tương đối với bán kính joystick
            float maxRadius = joystickBackground.sizeDelta.x / 2f;
            if (maxRadius <= 0) maxRadius = handleRange;

            position.x = (position.x / maxRadius);
            position.y = (position.y / maxRadius);

            inputVector = new Vector2(position.x, position.y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Di chuyển núm gạt (handle)
            joystickHandle.anchoredPosition = new Vector2(
                inputVector.x * (maxRadius * 0.8f),
                inputVector.y * (maxRadius * 0.8f)
            );
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }

        if (isFloatingJoystick && joystickBackground != null)
        {
            joystickBackground.anchoredPosition = defaultBackgroundPosition;
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
