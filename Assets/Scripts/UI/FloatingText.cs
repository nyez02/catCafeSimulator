using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1.2f;
    public float fadeDuration = 1.5f;

    private TextMeshPro tmpText;
    private Color startColor;
    private float timer;

    public void Setup(string message, Color color)
    {
        tmpText = GetComponent<TextMeshPro>();
        if (tmpText == null)
        {
            tmpText = gameObject.AddComponent<TextMeshPro>();
            tmpText.fontSize = 4;
            tmpText.alignment = TextAlignmentOptions.Center;
        }

        tmpText.text = message;
        tmpText.color = color;
        startColor = color;
        timer = 0f;
    }

    private void Update()
    {
        // Bay lên
        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

        // Luôn quay mặt về Camera
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        // Mờ dần
        timer += Time.deltaTime;
        float progress = timer / fadeDuration;
        if (tmpText != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, progress);
            tmpText.color = c;
        }

        if (timer >= fadeDuration)
        {
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.ReturnFloatingText(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
