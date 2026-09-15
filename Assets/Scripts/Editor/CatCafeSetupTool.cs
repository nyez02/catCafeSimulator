#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CatCafeSetupTool : Editor
{
    [MenuItem("Cat Cafe/🚀 1-Click Auto Setup Scene", false, 1)]
    public static void AutoSetupActiveScene()
    {
        Undo.IncrementCurrentGroup();
        string undoName = "1-Click Auto Setup Cat Cafe";
        Undo.SetCurrentGroupName(undoName);

        Debug.Log("===> [CatCafeSetupTool] Bắt đầu tự động cấu hình Cat Cafe Simulator...");

        // 1. Tìm hoặc tạo GameObject MANAGERS
        GameObject managersObj = GameObject.Find("MANAGERS");
        if (managersObj == null)
        {
            managersObj = new GameObject("MANAGERS");
            Undo.RegisterCreatedObjectUndo(managersObj, undoName);
        }

        // 2. Gắn đầy đủ toàn bộ hệ sinh thái Managers (22 Managers)
        MoneyManager moneyMgr = GetOrAddComponent<MoneyManager>(managersObj);
        SaveManager saveMgr = GetOrAddComponent<SaveManager>(managersObj);
        UIManager uiMgr = GetOrAddComponent<UIManager>(managersObj);
        GameManager gameMgr = GetOrAddComponent<GameManager>(managersObj);
        LevelManager levelMgr = GetOrAddComponent<LevelManager>(managersObj);
        CatManager catMgr = GetOrAddComponent<CatManager>(managersObj);
        TableManager tableMgr = GetOrAddComponent<TableManager>(managersObj);
        ShopManager shopMgr = GetOrAddComponent<ShopManager>(managersObj);
        SoundManager soundMgr = GetOrAddComponent<SoundManager>(managersObj);
        if (soundMgr.backgroundMusic == null) soundMgr.backgroundMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_lofi.wav");
        if (soundMgr.meowSound == null) soundMgr.meowSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/cat_meow.wav");
        if (soundMgr.coinSound == null) soundMgr.coinSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/coin.wav");
        if (soundMgr.clickSound == null) soundMgr.clickSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/click.wav");
        CustomerManager customerMgr = GetOrAddComponent<CustomerManager>(managersObj);
        LuckyPiggyBank piggyBank = GetOrAddComponent<LuckyPiggyBank>(managersObj);
        AchievementManager achMgr = GetOrAddComponent<AchievementManager>(managersObj);
        DailyRewardManager dailyMgr = GetOrAddComponent<DailyRewardManager>(managersObj);
        TutorialManager tutMgr = GetOrAddComponent<TutorialManager>(managersObj);
        ObjectPoolManager poolMgr = GetOrAddComponent<ObjectPoolManager>(managersObj);
        PerformanceManager perfMgr = GetOrAddComponent<PerformanceManager>(managersObj);
        PlatformManager platMgr = GetOrAddComponent<PlatformManager>(managersObj);

        // Các Manager nâng cao mới:
        FirebaseManager firebaseMgr = GetOrAddComponent<FirebaseManager>(managersObj);
        AdsManager adsMgr = GetOrAddComponent<AdsManager>(managersObj);
        DecorationManager decorMgr = GetOrAddComponent<DecorationManager>(managersObj);
        DecorationVisualSpawner decorSpawner = GetOrAddComponent<DecorationVisualSpawner>(managersObj);
        DailyQuestManager questMgr = GetOrAddComponent<DailyQuestManager>(managersObj);
        CatVFXManager vfxMgr = GetOrAddComponent<CatVFXManager>(managersObj);
        UIThemeManager themeMgr = GetOrAddComponent<UIThemeManager>(managersObj);
        StaffManager staffMgr = GetOrAddComponent<StaffManager>(managersObj);
        ToastManager toastMgr = GetOrAddComponent<ToastManager>(managersObj);
        LoadingScreenUI loadingUI = GetOrAddComponent<LoadingScreenUI>(managersObj);

        // Social & Multiplayer Managers
        CatCafe.Social.FriendManager friendMgr = GetOrAddComponent<CatCafe.Social.FriendManager>(managersObj);
        CatCafe.Social.ChatManager chatMgr = GetOrAddComponent<CatCafe.Social.ChatManager>(managersObj);
        CatCafe.Social.GiftManager giftMgr = GetOrAddComponent<CatCafe.Social.GiftManager>(managersObj);
        CatCafe.Maps.CafeMapThemeManager mapThemeMgr = GetOrAddComponent<CatCafe.Maps.CafeMapThemeManager>(managersObj);

        // Kết nối tham chiếu UI
        if (uiMgr != null)
        {
            uiMgr.decorationShopUI = GetOrAddComponent<DecorationShopUI>(managersObj);
            uiMgr.dailyQuestUI = GetOrAddComponent<DailyQuestUI>(managersObj);
            uiMgr.leaderboardUI = GetOrAddComponent<LeaderboardUI>(managersObj);
            uiMgr.staffShopUI = GetOrAddComponent<StaffShopUI>(managersObj);
            uiMgr.settingsUI = GetOrAddComponent<SettingsUI>(managersObj);

            // Thiết lập và khởi tạo Modern HUD cho Canvas
            uiMgr.EnsureModernHUD();
        }

        // 3. Tự động nạp Prefab Mèo & Toàn bộ danh mục giống mèo
        GameObject defaultCat = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LittleFriends-CartoonAnimals-Lite/Prefabs/LittleCat_Idle.prefab");
        if (defaultCat != null)
        {
            catMgr.defaultCatPrefab = defaultCat;
        }

        catMgr.availableBreeds.Clear();
        string[] catDataGuids = AssetDatabase.FindAssets("t:CatData", new[] { "Assets" });
        foreach (string guid in catDataGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CatData cat = AssetDatabase.LoadAssetAtPath<CatData>(path);
            if (cat != null && !catMgr.availableBreeds.Contains(cat))
            {
                catMgr.availableBreeds.Add(cat);
            }
        }

        // 4. Tự động nạp Khách Hàng (Customer Prefabs)
        string[] customerGuids = AssetDatabase.FindAssets("Character_ t:Prefab", new[] { "Assets/FREE/Pack_FREE_PartyCharacters/Prefabs" });
        customerMgr.customerPrefabs.Clear();
        foreach (string guid in customerGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                customerMgr.customerPrefabs.Add(prefab);
            }
        }
        if (customerMgr.customerPrefabs.Count > 0)
        {
            customerMgr.defaultCustomerPrefab = customerMgr.customerPrefabs[0];
        }

        // 5. Tự động nạp các Màn chơi (LevelData)
        levelMgr.allLevels.Clear();
        string[] levelGuids = AssetDatabase.FindAssets("Level t:LevelData", new[] { "Assets" });
        foreach (string guid in levelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelData lvl = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (lvl != null)
            {
                levelMgr.allLevels.Add(lvl);
            }
        }
        if (levelMgr.allLevels.Count > 0)
        {
            levelMgr.currentLevel = levelMgr.allLevels[0];
        }

        // 6. Tự động quét và gắn TableSeat cho tất cả bàn Table trong scene
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int tablesConfigured = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("table"))
            {
                TableSeat seat = GetOrAddComponent<TableSeat>(obj);
                if (seat.sitPoint == null)
                {
                    Transform existingSit = obj.transform.Find("SitPoint");
                    if (existingSit == null)
                    {
                        GameObject sitObj = new GameObject("SitPoint");
                        sitObj.transform.SetParent(obj.transform);
                        sitObj.transform.localPosition = new Vector3(0, 0, -1.2f);
                        seat.sitPoint = sitObj.transform;
                    }
                    else
                    {
                        seat.sitPoint = existingSit;
                    }
                }
                tablesConfigured++;
            }
        }

        // 7. Tự động xây dựng Map Quán Cafe 3D hoàn chỉnh
        CozyCafeMapBuilder.BuildCozyCafeMap();

        // 8. Đảm bảo EditorBuildSettings chứa đủ MainMenu và CafeScene
        EnsureBuildScenes();

        // 8. Đánh dấu Scene đã sửa đổi và lưu
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"===> [CatCafeSetupTool] THÀNH CÔNG RỰC RỠ! Đã gắn kết toàn bộ Managers, {catMgr.availableBreeds.Count} giống mèo, {customerMgr.customerPrefabs.Count} mẫu khách hàng, {levelMgr.allLevels.Count} màn chơi và {tablesConfigured} bàn cafe.");
        EditorUtility.DisplayDialog("Cat Cafe Simulator", $"Thiết lập toàn diện thành công 100%!\n- Đã gắn toàn bộ Managers, Spawners, Toasts & Staff trên MANAGERS\n- Đã nạp {catMgr.availableBreeds.Count} giống mèo mới\n- Đã nạp {customerMgr.customerPrefabs.Count} mẫu khách hàng (bao gồm VIP)\n- Đã nạp {levelMgr.allLevels.Count} màn chơi\n- Đã cấu hình {tablesConfigured} bàn cafe\n- Đã kiểm tra danh sách Scenes trong Build Settings.\n\nBây giờ bạn có thể bấm Play để trải nghiệm ngay lập tức!", "Tuyệt vời!");
    }

    private static void EnsureBuildScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] requiredPaths = new string[] { "Assets/Scenes/MainMenu.unity", "Assets/CafeScene.unity" };

        bool changed = false;
        foreach (string path in requiredPaths)
        {
            if (!scenes.Exists(s => s.path == path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                changed = true;
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T comp = target.GetComponent<T>();
        if (comp == null)
        {
            comp = Undo.AddComponent<T>(target);
        }
        return comp;
    }
}
#endif
