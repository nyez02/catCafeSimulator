using UnityEngine;
using UnityEngine.UI;

namespace CatCafe.Social
{
    public class ChatBubble : MonoBehaviour
    {
        private Transform targetTransform;
        private Vector3 offset = new Vector3(0, 2.2f, 0);
        private Text messageText;
        private CanvasGroup canvasGroup;
        private float lifeTimer = 3.5f;

        public static void ShowBubble(Transform target, string message)
        {
            if (target == null || string.IsNullOrEmpty(message)) return;

            // Tìm xem target đã có bubble chưa, nếu có thì tận dụng
            var existing = target.GetComponentInChildren<ChatBubble>();
            if (existing != null)
            {
                existing.SetMessage(message);
                return;
            }

            GameObject bubbleObj = new GameObject("ChatBubble_World");
            bubbleObj.transform.SetParent(target, false);
            bubbleObj.transform.localPosition = new Vector3(0, 2.2f, 0);

            var bubble = bubbleObj.AddComponent<ChatBubble>();
            bubble.SetupUI(target, message);
        }

        private void SetupUI(Transform target, string msg)
        {
            targetTransform = target;

            // Setup World Space Canvas
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;

            RectTransform rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240, 70);
            rt.localScale = Vector3.one * 0.012f; // Tỉ lệ hợp lý trong không gian 3D

            canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Background panel
            GameObject bgObj = new GameObject("Bubble_BG");
            bgObj.transform.SetParent(transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.14f, 0.22f, 0.88f); // Dark glassmorphism

            // Text
            GameObject txtObj = new GameObject("Bubble_Text");
            txtObj.transform.SetParent(bgObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(10, 6);
            txtRt.offsetMax = new Vector2(-10, -6);

            messageText = txtObj.AddComponent<Text>();
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (messageText.font == null) messageText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            messageText.fontSize = 18;
            messageText.color = Color.white;
            messageText.alignment = TextAnchor.MiddleCenter;

            SetMessage(msg);
        }

        public void SetMessage(string msg)
        {
            if (messageText != null)
            {
                messageText.text = msg;
            }
            lifeTimer = 4.0f;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one * 0.012f;
        }

        private void LateUpdate()
        {
            // Luôn xoay mặt về Camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                transform.rotation = cam.transform.rotation;
            }

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 1.0f && canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(lifeTimer);
            }

            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
