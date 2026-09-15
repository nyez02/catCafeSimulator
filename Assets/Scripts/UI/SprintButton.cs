using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Nút Chạy Nước Rút (Sprint Button) cho Mobile / Touchscreen.
/// Cho phép người chơi nhấn giữ hoặc chạm để nhân vật lướt đi siêu tốc.
/// </summary>
public class SprintButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Image buttonImage;
    private Color normalColor = new Color(0.95f, 0.55f, 0.2f, 0.85f);
    private Color pressedColor = new Color(1f, 0.82f, 0.2f, 1f);

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerMove.isMobileSprintHeld = true;
        if (buttonImage != null) buttonImage.color = pressedColor;
        transform.localScale = Vector3.one * 0.92f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayerMove.isMobileSprintHeld = false;
        if (buttonImage != null) buttonImage.color = normalColor;
        transform.localScale = Vector3.one;
    }

    private void OnDisable()
    {
        PlayerMove.isMobileSprintHeld = false;
        transform.localScale = Vector3.one;
    }
}
