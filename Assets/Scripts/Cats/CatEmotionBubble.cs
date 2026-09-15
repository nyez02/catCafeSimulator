using TMPro;
using UnityEngine;

public class CatEmotionBubble : MonoBehaviour
{
    private TextMeshPro tmpText;
    private CatAI catAI;
    private Camera mainCam;
    private float updateTimer = 0f;

    private void Awake()
    {
        catAI = GetComponentInParent<CatAI>();
        EnsureTextMesh();
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void EnsureTextMesh()
    {
        tmpText = GetComponent<TextMeshPro>();
        if (tmpText == null)
        {
            tmpText = gameObject.AddComponent<TextMeshPro>();
            tmpText.fontSize = 3.5f;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.outlineWidth = 0.2f;
            tmpText.outlineColor = Color.black;
        }
    }

    private void LateUpdate()
    {
        // Luôn xoay mặt về camera
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        }

        // Cập nhật biểu cảm cảm xúc mỗi 0.5s để tối ưu hiệu năng
        updateTimer += Time.deltaTime;
        if (updateTimer >= 0.5f)
        {
            updateTimer = 0f;
            RefreshEmotion();
        }
    }

    public void RefreshEmotion()
    {
        if (catAI == null || tmpText == null) return;

        switch (catAI.currentState)
        {
            case CatState.Sleeping:
                tmpText.text = "💤";
                tmpText.color = new Color(0.7f, 0.8f, 1f);
                break;

            case CatState.Eating:
                tmpText.text = "🍖";
                tmpText.color = Color.yellow;
                break;

            case CatState.Greeting:
                tmpText.text = "❤️";
                tmpText.color = Color.magenta;
                break;

            default:
                if (catAI.hunger < 30f)
                {
                    tmpText.text = "🐟"; // Đói bụng
                    tmpText.color = new Color(1f, 0.6f, 0.2f);
                }
                else if (catAI.happiness >= 80f)
                {
                    tmpText.text = "✨"; // Hạnh phúc
                    tmpText.color = Color.green;
                }
                else
                {
                    tmpText.text = ""; // Bình thường
                }
                break;
        }
    }
}
