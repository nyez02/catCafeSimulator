using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }

    [Header("UI References (Optional)")]
    public Canvas targetCanvas;
    public GameObject toastContainer;
    public TextMeshProUGUI toastText;
    public Image toastBackground;
    public Image toastIcon;

    private readonly Queue<ToastData> toastQueue = new Queue<ToastData>();
    private bool isShowing = false;

    private struct ToastData
    {
        public string message;
        public string icon;
        public Color bgColor;
        public float duration;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureUIComponents();
    }

    private void EnsureUIComponents()
    {
        if (targetCanvas == null)
        {
            // Tìm Canvas hiện tại hoặc tạo một Canvas phủ chuyên biệt
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                GameObject canvasObj = new GameObject("ToastCanvas");
                targetCanvas = canvasObj.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 999; // Luôn hiển thị trên cùng
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObj);
            }
        }

        if (toastContainer == null)
        {
            // Tự động xây dựng Toast UI Container đẹp mắt dạng pill lơ lửng
            GameObject containerObj = new GameObject("ToastPill");
            containerObj.transform.SetParent(targetCanvas.transform, false);

            RectTransform rect = containerObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -40f);
            rect.sizeDelta = new Vector2(480f, 60f);

            toastBackground = containerObj.AddComponent<Image>();
            toastBackground.color = new Color(0.12f, 0.12f, 0.18f, 0.95f); // Deep cozy dark

            var outline = containerObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.8f, 0.4f, 0.6f);
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject textObj = new GameObject("ToastText");
            textObj.transform.SetParent(containerObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 5f);
            textRect.offsetMax = new Vector2(-20f, -5f);

            toastText = textObj.AddComponent<TextMeshProUGUI>();
            toastText.alignment = TextAlignmentOptions.Center;
            toastText.fontSize = 20f;
            toastText.color = Color.white;
            toastText.fontStyle = FontStyles.Bold;

            toastContainer = containerObj;
            toastContainer.SetActive(false);
        }
    }

    public void ShowToast(string message, string icon = "🐾", Color? bgColor = null, float duration = 2.5f)
    {
        Color col = bgColor ?? new Color(0.12f, 0.15f, 0.22f, 0.95f);
        toastQueue.Enqueue(new ToastData
        {
            message = message,
            icon = icon,
            bgColor = col,
            duration = duration
        });

        if (!isShowing)
        {
            StartCoroutine(ProcessToastQueue());
        }
    }

    private IEnumerator ProcessToastQueue()
    {
        isShowing = true;

        while (toastQueue.Count > 0)
        {
            ToastData data = toastQueue.Dequeue();
            EnsureUIComponents();

            if (toastContainer != null && toastText != null)
            {
                toastText.text = $"{data.icon} {data.message}";
                if (toastBackground != null) toastBackground.color = data.bgColor;

                toastContainer.SetActive(true);
                RectTransform rect = toastContainer.GetComponent<RectTransform>();

                // Hiệu ứng trượt xuống êm ái (Slide in)
                float elapsed = 0f;
                float animDuration = 0.25f;
                Vector2 startPos = new Vector2(0f, 80f);
                Vector2 endPos = new Vector2(0f, -40f);

                while (elapsed < animDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
                    if (rect != null) rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                    yield return null;
                }

                if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();

                // Chờ thời gian hiển thị
                yield return new WaitForSecondsRealtime(data.duration);

                // Hiệu ứng trượt lên biến mất (Slide out)
                elapsed = 0f;
                while (elapsed < animDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
                    if (rect != null) rect.anchoredPosition = Vector2.Lerp(endPos, startPos, t);
                    yield return null;
                }

                toastContainer.SetActive(false);
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }

        isShowing = false;
    }
}
