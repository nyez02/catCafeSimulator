using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float rotationSpeed = 12f;

    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // 1. Nhận input từ Bàn phím / Gamepad (PC & Laptop)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 keyInput = new Vector2(h, v);

        // 2. Nhận input từ Cần gạt ảo cảm ứng (Mobile & Tablet)
        Vector2 touchInput = Vector2.zero;
        if (VirtualJoystick.Instance != null && VirtualJoystick.Instance.Direction.sqrMagnitude > 0.0025f)
        {
            touchInput = VirtualJoystick.Instance.Direction;
        }

        // 3. Kết hợp 2 nguồn input (ưu tiên nguồn có lực gạt lớn hơn)
        Vector2 finalInput = (touchInput.sqrMagnitude > keyInput.sqrMagnitude) ? touchInput : keyInput;

        Vector3 moveDirection = new Vector3(finalInput.x, 0, finalInput.y);
        float inputStrength = Mathf.Clamp01(finalInput.magnitude);

        if (inputStrength > 0.05f)
        {
            // Tốc độ thay đổi linh hoạt theo độ gạt cần điều khiển
            float currentSpeed = speed * inputStrength;

            // Di chuyển World Space
            transform.Translate(moveDirection.normalized * (currentSpeed * Time.deltaTime), Space.World);

            // Xoay nhân vật mượt mà theo góc 360 độ
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }
}