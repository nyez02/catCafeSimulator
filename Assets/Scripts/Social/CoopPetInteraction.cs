using UnityEngine;
using CatCafe.Social;

namespace CatCafe.Social
{
    /// <summary>
    /// Cho phép người chơi tương tác với mèo (vuốt ve, cho ăn) cả ở quán mình và quán bạn bè.
    /// </summary>
    public class CoopPetInteraction : MonoBehaviour
    {
        [Header("Settings")]
        public string catName = "Miu Con";
        public float interactionRadius = 2.5f;

        private Transform playerTransform;

        private void Start()
        {
            var player = FindFirstObjectByType<PlayerMove>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void OnMouseDown()
        {
            InteractWithCat();
        }

        public void InteractWithCat()
        {
            if (playerTransform == null)
            {
                var player = FindFirstObjectByType<PlayerMove>();
                if (player != null) playerTransform = player.transform;
            }

            // Kiểm tra khoảng cách người chơi tới mèo
            if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > 4.5f)
            {
                ToastManager.Instance?.ShowToast("Hãy lại gần bé mèo hơn để vuốt ve nhé! 🐾", "🐱");
                return;
            }

            bool isVisiting = FriendManager.Instance != null && FriendManager.Instance.isVisitingFriend;

            if (isVisiting)
            {
                // Vuốt ve mèo quán bạn bè
                FriendManager.Instance.RecordVisitorInteraction(catName, "pet");
                ChatBubble.ShowBubble(transform, "Meo meo~ Cảm ơn bạn đã vuốt ve mình! 💕");
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingText("💖 +1 Tình Bạn!", transform.position + Vector3.up * 1.5f, Color.magenta);
                }
            }
            else
            {
                // Vuốt ve mèo quán mình
                DailyQuestManager.Instance?.AddQuestProgress("quest_pet", 1);
                ChatBubble.ShowBubble(transform, "Purrr~ Mèo thích lắm! 💖");
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingText("💕 Hạnh phúc +10!", transform.position + Vector3.up * 1.5f, Color.yellow);
                }
            }

            SoundManager.Instance?.PlayCatMeow();
        }
    }
}
