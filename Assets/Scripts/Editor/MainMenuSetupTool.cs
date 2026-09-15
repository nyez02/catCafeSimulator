#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuSetupTool : Editor
{
    private const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Cat Cafe/🎬 Thiết Lập Màn Hình Loading & Menu Đẹp (Fix Màn Hình Đen)", false, 3)]
    public static void SetupMainMenuScene()
    {
        // 1. Mở scene MainMenu
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // 2. Dọn dẹp các object thừa hoặc bị mất camera cũ
        GameObject[] rootObjs = scene.GetRootGameObjects();
        foreach (var obj in rootObjs)
        {
            if (obj.name.Contains("Box") || obj.name.Contains("Capsule") || obj.name.Contains("Tutorial") || obj.name.Contains("Secret") || obj.name.Contains("Star"))
            {
                DestroyImmediate(obj);
            }
        }

        // 3. Đảm bảo có Main Camera chuẩn
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.09f, 0.14f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;

        // 4. Đảm bảo có EventSystem
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 5. Tìm hoặc tạo Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            DestroyImmediate(canvas.gameObject);
        }

        GameObject canvasObj = new GameObject("MainMenu_Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // 6. Xây dựng Giao diện Màn hình Loading / MainMenu
        GameObject rootPanel = new GameObject("Loading_Menu_Root");
        rootPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform rootRt = rootPanel.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;

        Image bgImg = rootPanel.AddComponent<Image>();
        bgImg.color = new Color(0.09f, 0.08f, 0.12f, 1f); // Nền tối cafe ấm áp

        CanvasGroup cg = rootPanel.AddComponent<CanvasGroup>();

        // Load Icon chú mèo trong ly cà phê
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/AppIcon.jpg");

        // A. Logo / Icon Mèo
        GameObject logoObj = new GameObject("CatLogo");
        logoObj.transform.SetParent(rootPanel.transform, false);
        RectTransform logoRt = logoObj.AddComponent<RectTransform>();
        logoRt.anchorMin = new Vector2(0.5f, 0.5f);
        logoRt.anchorMax = new Vector2(0.5f, 0.5f);
        logoRt.pivot = new Vector2(0.5f, 0.5f);
        logoRt.anchoredPosition = new Vector2(0f, 160f);
        logoRt.sizeDelta = new Vector2(280f, 280f);

        Image logoImg = logoObj.AddComponent<Image>();
        if (iconSprite != null)
        {
            logoImg.sprite = iconSprite;
        }
        else
        {
            logoImg.color = new Color(1f, 0.75f, 0.8f);
        }

        // B. Tiêu đề Game
        GameObject titleObj = new GameObject("GameTitle");
        titleObj.transform.SetParent(rootPanel.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0f, -30f);
        titleRt.sizeDelta = new Vector2(900f, 65f);

        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "🐱 TIỆM CÀ PHÊ MÈO COZY ☕";
        titleTxt.fontSize = 46f;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = new Color(1f, 0.85f, 0.42f); // Vàng mật ong
        titleTxt.alignment = TextAlignmentOptions.Center;

        // C. Phụ đề
        GameObject subObj = new GameObject("Subtitle");
        subObj.transform.SetParent(rootPanel.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.5f);
        subRt.anchorMax = new Vector2(0.5f, 0.5f);
        subRt.pivot = new Vector2(0.5f, 0.5f);
        subRt.anchoredPosition = new Vector2(0f, -75f);
        subRt.sizeDelta = new Vector2(800f, 40f);

        TextMeshProUGUI subTxt = subObj.AddComponent<TextMeshProUGUI>();
        subTxt.text = "Pha Chế Cà Phê • Chăm Sóc Mèo Cưng • Xây Dựng Tiệm Mơ Ước";
        subTxt.fontSize = 20f;
        subTxt.color = new Color(0.85f, 0.82f, 0.90f);
        subTxt.alignment = TextAlignmentOptions.Center;

        // D. Khung Thanh Loading (Progress Bar)
        GameObject barBgObj = new GameObject("ProgressBar_Background");
        barBgObj.transform.SetParent(rootPanel.transform, false);
        RectTransform barBgRt = barBgObj.AddComponent<RectTransform>();
        barBgRt.anchorMin = new Vector2(0.5f, 0.5f);
        barBgRt.anchorMax = new Vector2(0.5f, 0.5f);
        barBgRt.pivot = new Vector2(0.5f, 0.5f);
        barBgRt.anchoredPosition = new Vector2(0f, -150f);
        barBgRt.sizeDelta = new Vector2(620f, 26f);

        Image barBgImg = barBgObj.AddComponent<Image>();
        barBgImg.color = new Color(0.18f, 0.16f, 0.24f, 1f);

        // Thanh Fill tiến trình
        GameObject barFillObj = new GameObject("ProgressBar_Fill");
        barFillObj.transform.SetParent(barBgObj.transform, false);
        RectTransform barFillRt = barFillObj.AddComponent<RectTransform>();
        barFillRt.anchorMin = Vector2.zero;
        barFillRt.anchorMax = Vector2.one;
        barFillRt.sizeDelta = Vector2.zero;

        Image barFillImg = barFillObj.AddComponent<Image>();
        barFillImg.type = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillAmount = 0f;
        barFillImg.color = new Color(0.98f, 0.44f, 0.56f, 1f); // Hồng đào nổi bật

        // Dòng chữ phần trăm
        GameObject progTxtObj = new GameObject("ProgressText");
        progTxtObj.transform.SetParent(rootPanel.transform, false);
        RectTransform progTxtRt = progTxtObj.AddComponent<RectTransform>();
        progTxtRt.anchorMin = new Vector2(0.5f, 0.5f);
        progTxtRt.anchorMax = new Vector2(0.5f, 0.5f);
        progTxtRt.pivot = new Vector2(0.5f, 0.5f);
        progTxtRt.anchoredPosition = new Vector2(0f, -195f);
        progTxtRt.sizeDelta = new Vector2(600f, 35f);

        TextMeshProUGUI progTxt = progTxtObj.AddComponent<TextMeshProUGUI>();
        progTxt.text = "Đang chuẩn bị quán cà phê... 0%";
        progTxt.fontSize = 19f;
        progTxt.fontStyle = FontStyles.Bold;
        progTxt.color = new Color(1f, 0.90f, 0.55f);
        progTxt.alignment = TextAlignmentOptions.Center;

        // Dòng Mẹo chơi ngẫu nhiên
        GameObject tipObj = new GameObject("TipText");
        tipObj.transform.SetParent(rootPanel.transform, false);
        RectTransform tipRt = tipObj.AddComponent<RectTransform>();
        tipRt.anchorMin = new Vector2(0.5f, 0.5f);
        tipRt.anchorMax = new Vector2(0.5f, 0.5f);
        tipRt.pivot = new Vector2(0.5f, 0.5f);
        tipRt.anchoredPosition = new Vector2(0f, -245f);
        tipRt.sizeDelta = new Vector2(800f, 45f);

        TextMeshProUGUI tipTxt = tipObj.AddComponent<TextMeshProUGUI>();
        tipTxt.text = "🐾 Mẹo: Cho mèo ăn no sẽ giúp khách vui vẻ và tip tiền gấp đôi!";
        tipTxt.fontSize = 16f;
        tipTxt.color = new Color(0.78f, 0.75f, 0.85f);
        tipTxt.alignment = TextAlignmentOptions.Center;

        // Nút Vào Quán (Dự phòng)
        GameObject btnObj = new GameObject("PlayButton");
        btnObj.transform.SetParent(rootPanel.transform, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = new Vector2(0f, -170f);
        btnRt.sizeDelta = new Vector2(280f, 65f);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.98f, 0.44f, 0.56f, 1f);
        Button playBtn = btnObj.AddComponent<Button>();

        GameObject btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        RectTransform btrt = btnTxtObj.AddComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;

        TextMeshProUGUI btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "▶️ VÀO QUÁN CHƠI NGAY";
        btnTxt.fontSize = 22f;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.color = Color.white;
        btnTxt.alignment = TextAlignmentOptions.Center;

        // Gắn controller
        LoadingScreenController controller = rootPanel.AddComponent<LoadingScreenController>();
        controller.progressFill = barFillImg;
        controller.progressText = progTxt;
        controller.tipText = tipTxt;
        controller.playButton = playBtn;
        controller.canvasGroup = cg;
        controller.fakeLoadDuration = 2.2f;
        controller.autoEnterGameWhenReady = true;

        // Lưu lại Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("===> [MainMenuSetupTool] THÀNH CÔNG! Đã tạo Màn hình Loading Screen & Main Menu hoàn chỉnh cho MainMenu.unity!");

        EditorUtility.DisplayDialog("Cat Cafe Simulator", 
            "Đã thiết lập thành công Màn hình Loading Screen & Menu hoàn chỉnh cho game!\n\n" +
            "✅ Đã khắc phục triệt để lỗi màn hình đen (bổ sung Camera & UI Canvas chuẩn)\n" +
            "✅ Đã thêm Logo chú mèo trong ly cafe cực kỳ dễ thương\n" +
            "✅ Đã thêm Thanh Loading tự động nạp từ 0% -> 100%\n" +
            "✅ Đã thêm các câu mẹo ngẫu nhiên về mèo và quán cafe\n" +
            "✅ Tự động chuyển cảnh mượt mà vào CafeScene khi hoàn tất!", "Tuyệt vời!");
    }
}
#endif
