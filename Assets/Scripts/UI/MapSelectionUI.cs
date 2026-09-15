using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CatCafe.Maps;

namespace CatCafe.UI
{
    public class MapSelectionUI : MonoBehaviour
    {
        public static MapSelectionUI Instance { get; private set; }

        [Header("UI Root")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private Transform mapCardsContainer;
        [SerializeField] private Button closeBtn;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUIHierarchyIfNeeded();
        }

        private void Start()
        {
            if (windowRoot != null) windowRoot.SetActive(false);
        }

        public static void Show()
        {
            if (Instance == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject obj = new GameObject("MapSelection_UI_System");
                    obj.transform.SetParent(canvas.transform, false);
                    Instance = obj.AddComponent<MapSelectionUI>();
                    Instance.BuildUIHierarchyIfNeeded();
                }
            }

            if (Instance != null)
            {
                Instance.OpenWindow();
            }
        }

        public void OpenWindow()
        {
            if (windowRoot != null) windowRoot.SetActive(true);
            RefreshMapCards();
        }

        public void CloseWindow()
        {
            if (windowRoot != null) windowRoot.SetActive(false);
        }

        private void RefreshMapCards()
        {
            if (mapCardsContainer == null || CafeMapThemeManager.Instance == null) return;

            foreach (Transform child in mapCardsContainer)
            {
                Destroy(child.gameObject);
            }

            int currentLevel = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : 1;
            string activeMapId = CafeMapThemeManager.Instance.currentMapId;

            foreach (var theme in CafeMapThemeManager.Instance.availableThemes)
            {
                CreateMapCard(theme, currentLevel, activeMapId, mapCardsContainer);
            }
        }

        private void CreateMapCard(CafeMapTheme theme, int currentLevel, string activeMapId, Transform parent)
        {
            GameObject card = new GameObject("MapCard_" + theme.id);
            card.transform.SetParent(parent, false);

            var rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(580, 80);

            var bg = card.AddComponent<Image>();
            bool isCurrent = theme.id == activeMapId;
            bg.color = isCurrent ? new Color(0.20f, 0.35f, 0.50f, 0.95f) : new Color(0.12f, 0.15f, 0.22f, 0.88f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Info Box
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);
            var infoRt = infoObj.AddComponent<RectTransform>();
            infoRt.sizeDelta = new Vector2(380, 64);

            var infoTxt = infoObj.AddComponent<Text>();
            infoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (infoTxt.font == null) infoTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            infoTxt.fontSize = 14;
            infoTxt.color = Color.white;
            infoTxt.alignment = TextAnchor.MiddleLeft;

            bool isUnlocked = currentLevel >= theme.requiredLevel;
            string lockStatus = isUnlocked ? "<color=#81C784>[Đã mở khóa ✓]</color>" : $"<color=#FF7043>[Yêu cầu Cấp {theme.requiredLevel} ☕]</color>";
            infoTxt.text = $"<b>{theme.displayName}</b> {lockStatus}\n<color=#CFD8DC><size=12>{theme.description}</size></color>";

            // Action Button
            GameObject btnObj = new GameObject("ActionBtn");
            btnObj.transform.SetParent(card.transform, false);
            var btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(130, 46);

            var btnImg = btnObj.AddComponent<Image>();
            var btn = btnObj.AddComponent<Button>();

            GameObject btnTxtObj = new GameObject("Text");
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            var txtRt = btnTxtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
            var btnTxt = btnTxtObj.AddComponent<Text>();
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (btnTxt.font == null) btnTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            btnTxt.fontSize = 13;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.color = Color.white;
            btnTxt.alignment = TextAnchor.MiddleCenter;

            if (isCurrent)
            {
                btnImg.color = new Color(0.2f, 0.7f, 0.4f);
                btnTxt.text = "Đang Chọn ✓";
                btn.interactable = false;
            }
            else if (isUnlocked)
            {
                btnImg.color = new Color(0.95f, 0.45f, 0.3f);
                btnTxt.text = "Chọn Bản Đồ ✨";
                btn.onClick.AddListener(() =>
                {
                    CafeMapThemeManager.Instance?.ApplyTheme(theme.id);
                    RefreshMapCards();
                });
            }
            else
            {
                btnImg.color = new Color(0.35f, 0.38f, 0.45f);
                btnTxt.text = "🔒 Chưa Khóa";
                btn.interactable = false;
            }
        }

        private void BuildUIHierarchyIfNeeded()
        {
            if (windowRoot != null) return;

            windowRoot = new GameObject("MapSelection_WindowRoot");
            windowRoot.transform.SetParent(transform, false);
            RectTransform rootRt = windowRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            var dimBg = windowRoot.AddComponent<Image>();
            dimBg.color = new Color(0f, 0f, 0f, 0.65f);

            GameObject panelObj = new GameObject("Center_Dialog");
            panelObj.transform.SetParent(windowRoot.transform, false);
            RectTransform panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(660, 480);

            var panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.09f, 0.11f, 0.18f, 0.96f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.sizeDelta = new Vector2(0, 60);

            var titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (titleTxt.font == null) titleTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 18);
            titleTxt.fontSize = 20;
            titleTxt.color = new Color(1f, 0.85f, 0.4f);
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.text = "🗺️ CHỌN MÀN CHƠI & BẢN ĐỒ QUÁN CAFE ☕";

            // Close Btn
            GameObject closeObj = new GameObject("CloseBtn");
            closeObj.transform.SetParent(panelObj.transform, false);
            RectTransform closeRt = closeObj.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1, 1);
            closeRt.anchorMax = new Vector2(1, 1);
            closeRt.anchoredPosition = new Vector2(-28, -28);
            closeRt.sizeDelta = new Vector2(44, 44);

            var closeImg = closeObj.AddComponent<Image>();
            closeImg.color = new Color(0.8f, 0.2f, 0.2f);
            closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(CloseWindow);

            GameObject closeTxtObj = new GameObject("Text");
            closeTxtObj.transform.SetParent(closeObj.transform, false);
            RectTransform cTxtRt = closeTxtObj.AddComponent<RectTransform>();
            cTxtRt.anchorMin = Vector2.zero;
            cTxtRt.anchorMax = Vector2.one;
            cTxtRt.sizeDelta = Vector2.zero;
            var cTxt = closeTxtObj.AddComponent<Text>();
            cTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cTxt.font == null) cTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            cTxt.fontSize = 18;
            cTxt.color = Color.white;
            cTxt.alignment = TextAnchor.MiddleCenter;
            cTxt.text = "✖";

            // Container for Map Cards
            GameObject scrollObj = new GameObject("ScrollArea");
            scrollObj.transform.SetParent(panelObj.transform, false);
            RectTransform sRt = scrollObj.AddComponent<RectTransform>();
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(25, 20);
            sRt.offsetMax = new Vector2(-25, -70);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            GameObject viewObj = new GameObject("Viewport");
            viewObj.transform.SetParent(scrollObj.transform, false);
            RectTransform vRt = viewObj.AddComponent<RectTransform>();
            vRt.anchorMin = Vector2.zero;
            vRt.anchorMax = Vector2.one;
            vRt.sizeDelta = Vector2.zero;
            viewObj.AddComponent<Mask>().showMaskGraphic = false;
            viewObj.AddComponent<Image>();

            GameObject contObj = new GameObject("Content");
            contObj.transform.SetParent(viewObj.transform, false);
            RectTransform cRt = contObj.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 1);
            cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(0.5f, 1);
            cRt.sizeDelta = new Vector2(0, 360);

            var vlg = contObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = contObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = vRt;
            sr.content = cRt;
            mapCardsContainer = contObj.transform;
        }
    }
}
