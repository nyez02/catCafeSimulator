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

        // 2. Gắn đầy đủ các Manager
        MoneyManager moneyMgr = GetOrAddComponent<MoneyManager>(managersObj);
        SaveManager saveMgr = GetOrAddComponent<SaveManager>(managersObj);
        UIManager uiMgr = GetOrAddComponent<UIManager>(managersObj);
        GameManager gameMgr = GetOrAddComponent<GameManager>(managersObj);
        LevelManager levelMgr = GetOrAddComponent<LevelManager>(managersObj);
        CatManager catMgr = GetOrAddComponent<CatManager>(managersObj);
        TableManager tableMgr = GetOrAddComponent<TableManager>(managersObj);
        ShopManager shopMgr = GetOrAddComponent<ShopManager>(managersObj);
        SoundManager soundMgr = GetOrAddComponent<SoundManager>(managersObj);
        CustomerManager customerMgr = GetOrAddComponent<CustomerManager>(managersObj);
        LuckyPiggyBank piggyBank = GetOrAddComponent<LuckyPiggyBank>(managersObj);
        AchievementManager achMgr = GetOrAddComponent<AchievementManager>(managersObj);
        DailyRewardManager dailyMgr = GetOrAddComponent<DailyRewardManager>(managersObj);
        TutorialManager tutMgr = GetOrAddComponent<TutorialManager>(managersObj);
        ObjectPoolManager poolMgr = GetOrAddComponent<ObjectPoolManager>(managersObj);
        PerformanceManager perfMgr = GetOrAddComponent<PerformanceManager>(managersObj);
        PlatformManager platMgr = GetOrAddComponent<PlatformManager>(managersObj);

        // 3. Tự động nạp Prefab Mèo & Giống Mèo
        GameObject defaultCat = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LittleFriends-CartoonAnimals-Lite/Prefabs/LittleCat_Idle.prefab");
        if (defaultCat != null)
        {
            catMgr.defaultCatPrefab = defaultCat;
        }

        CatData meoTamThe = AssetDatabase.LoadAssetAtPath<CatData>("Assets/MeoTamThe.asset");
        if (meoTamThe != null && !catMgr.availableBreeds.Contains(meoTamThe))
        {
            catMgr.availableBreeds.Add(meoTamThe);
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
                    // Tạo một SitPoint trước mặt bàn nếu chưa có
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

        // 7. Đánh dấu Scene đã sửa đổi và lưu
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"===> [CatCafeSetupTool] HOÀN TẤT THÀNH CÔNG! Đã gắn kết 17 Managers, {customerMgr.customerPrefabs.Count} mẫu khách hàng, {levelMgr.allLevels.Count} màn chơi và cấu hình {tablesConfigured} bàn cafe.");
        EditorUtility.DisplayDialog("Cat Cafe Simulator", $"Thiết lập thành công 100%!\n- Đã gắn toàn bộ 17 Managers trên MANAGERS\n- Đã nạp {customerMgr.customerPrefabs.Count} mẫu khách hàng\n- Đã nạp {levelMgr.allLevels.Count} màn chơi\n- Đã kết nối {tablesConfigured} bàn cafe trong quán.\n\nBây giờ bạn có thể bấm Play để chơi ngay!", "Tuyệt vời!");
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
