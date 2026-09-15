#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CozyCafeMapBuilder : Editor
{
    private const string ROOT_NAME = "Cafe_Interior_Environment";

    [MenuItem("Cat Cafe/🏰 1-Click Xây Dựng Map Quán Cafe Hoàn Chỉnh (Full Cozy Map)", false, 2)]
    public static void BuildCozyCafeMapMenu()
    {
        BuildCozyCafeMap();
        EditorUtility.DisplayDialog("Cat Cafe Simulator", 
            "Đã xây dựng thành công Đại Bản Đồ Quán Cafe 3D Siêu Lớn & Hoành Tráng (Grand Luxury Cafe)!\n\n" +
            "🌟 Kích thước mở rộng khổng lồ: 44m x 32m (Gấp đôi diện tích!)\n" +
            "☕ Quầy Bar & Bếp chữ U đồ sộ (2 Máy pha cà phê, 2 máy POS, khay bánh cookie, kệ ly, bảng menu)\n" +
            "🪑 12 Bàn cafe đầy đủ ghế ngồi, TableSeat, SitPoint và FoodPoint cho khách\n" +
            "🐱 Thiên Đường Mèo Mở Rộng: 2 Thảm nhung lớn, 3 ghế bành sofa êm, 4 bát thức ăn mèo\n" +
            "📚 Góc Đọc Sách Thư Thái: Kệ sách dài, tranh nghệ thuật, chậu cây cảnh xanh\n" +
            "🚪 Sảnh đón khách thảm đỏ sang trọng từ cửa vào\n" +
            "💡 Hệ thống đèn chiếu sáng ấm cúng (Cozy Golden Lighting)\n" +
            "🎥 Camera góc nhìn bao quát toàn cảnh mượt mà theo chân nhân vật", "Tuyệt vời!");
    }

    public static GameObject BuildCozyCafeMap()
    {
        Undo.IncrementCurrentGroup();
        string undoName = "Build Grand Luxury Cozy Cafe Map";
        Undo.SetCurrentGroupName(undoName);

        Debug.Log("===> [CozyCafeMapBuilder] Đang khởi tạo Đại Bản Đồ Quán Cafe 44m x 32m...");

        // 1. Dọn dẹp sàn phẳng cũ
        GameObject oldPlane = GameObject.Find("Plane");
        if (oldPlane != null)
        {
            oldPlane.SetActive(false);
        }

        GameObject root = GameObject.Find(ROOT_NAME);
        if (root != null)
        {
            Undo.DestroyObjectImmediate(root);
        }
        root = new GameObject(ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(root, undoName);

        // Load Material của gói Interior
        Material cartoonMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Mnostva Art/FREE_Interiors_2/Materials/Cartoon_Mat.mat");
        if (cartoonMat == null)
        {
            cartoonMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            cartoonMat.color = new Color(0.85f, 0.72f, 0.58f);
        }

        // Material Tường Màu Kem Pastel Ấm
        Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wallMat.color = new Color(0.96f, 0.93f, 0.88f);
        wallMat.name = "GrandCozyWall_Mat";

        // Material Sàn Gỗ Parquet Sang Trọng
        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.74f, 0.53f, 0.36f);
        floorMat.name = "GrandFloor_Wood_Mat";

        // Material Thảm Đỏ Lối Vào
        Material redCarpetMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        redCarpetMat.color = new Color(0.82f, 0.28f, 0.30f);
        redCarpetMat.name = "WelcomeCarpet_Mat";

        // ==========================================
        // 1. SÀN PHÒNG ĐẠI KHÔNG GIAN (44m x 32m)
        // ==========================================
        GameObject floorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floorObj.name = "Floor_GrandCozyWood";
        floorObj.transform.SetParent(root.transform);
        floorObj.transform.position = new Vector3(0f, -0.25f, 0f);
        floorObj.transform.localScale = new Vector3(44f, 0.5f, 32f);
        floorObj.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

        // Thảm đỏ hoàng gia trải dài từ cửa đón khách vào trung tâm quán
        GameObject grandEntranceRug = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grandEntranceRug.name = "Grand_Welcome_Carpet";
        grandEntranceRug.transform.SetParent(root.transform);
        grandEntranceRug.transform.position = new Vector3(0f, 0.02f, -8f);
        grandEntranceRug.transform.localScale = new Vector3(5f, 0.04f, 15f);
        grandEntranceRug.GetComponent<MeshRenderer>().sharedMaterial = redCarpetMat;

        // ==========================================
        // 2. TƯỜNG BAO QUANH PHÒNG CAO 5.2m
        // ==========================================
        GameObject wallsGroup = new GameObject("Walls");
        wallsGroup.transform.SetParent(root.transform);

        float wallH = 5.2f;
        float halfH = wallH / 2f;

        // Tường sau & 2 bên
        CreateWall("Wall_Back", wallsGroup.transform, new Vector3(0f, halfH, 16f), new Vector3(44f, wallH, 0.8f), wallMat);
        CreateWall("Wall_Left", wallsGroup.transform, new Vector3(-22f, halfH, 0f), new Vector3(0.8f, wallH, 32f), wallMat);
        CreateWall("Wall_Right", wallsGroup.transform, new Vector3(22f, halfH, 0f), new Vector3(0.8f, wallH, 32f), wallMat);

        // Tường trước chừa sảnh đón khách rộng 8m ở giữa
        CreateWall("Wall_Front_Left", wallsGroup.transform, new Vector3(-15f, halfH, -16f), new Vector3(14f, wallH, 0.8f), wallMat);
        CreateWall("Wall_Front_Right", wallsGroup.transform, new Vector3(15f, halfH, -16f), new Vector3(14f, wallH, 0.8f), wallMat);
        CreateWall("Door_Arch_Header", wallsGroup.transform, new Vector3(0f, 4.4f, -16f), new Vector3(8f, 1.6f, 0.8f), wallMat);

        // ==========================================
        // 3. ĐẠI QUẦY BAR & BẾP PHA CHẾ CHỮ U (GRAND COFFEE BAR)
        // ==========================================
        GameObject barGroup = new GameObject("Grand_Bar_Kitchen");
        barGroup.transform.SetParent(root.transform);

        // Quầy trước (Order & Thu ngân)
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cabinet_1.prefab", barGroup.transform, new Vector3(-16f, 0f, 8f), Quaternion.Euler(0, 0, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cabinet_2.prefab", barGroup.transform, new Vector3(-13.5f, 0f, 8f), Quaternion.Euler(0, 0, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cabinet_1.prefab", barGroup.transform, new Vector3(-11f, 0f, 8f), Quaternion.Euler(0, 0, 0));

        // Quầy cánh hông
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cabinet_2.prefab", barGroup.transform, new Vector3(-9.5f, 0f, 10.5f), Quaternion.Euler(0, 90, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cabinet_1.prefab", barGroup.transform, new Vector3(-9.5f, 0f, 13f), Quaternion.Euler(0, 90, 0));

        // Quầy hậu cần pha chế sát tường
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cabinet_1.prefab", barGroup.transform, new Vector3(-16f, 0f, 15f), Quaternion.Euler(0, 180, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cabinet_2.prefab", barGroup.transform, new Vector3(-13.5f, 0f, 15f), Quaternion.Euler(0, 180, 0));

        // Thiết bị pha chế & Bánh ngọt
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Coffee_Machine_1.prefab", barGroup.transform, new Vector3(-16f, 0.95f, 14.8f), Quaternion.Euler(0, 180, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Coffee_Machine_1.prefab", barGroup.transform, new Vector3(-13.5f, 0.95f, 14.8f), Quaternion.Euler(0, 180, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cafe_Cash_Register_1.prefab", barGroup.transform, new Vector3(-16f, 0.95f, 7.8f), Quaternion.Euler(0, 0, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cafe_Cash_Register_1.prefab", barGroup.transform, new Vector3(-11f, 0.95f, 7.8f), Quaternion.Euler(0, 0, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cookie_Stand_1.prefab", barGroup.transform, new Vector3(-13.5f, 0.95f, 7.8f), Quaternion.identity);
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Cup_Stand_1.prefab", barGroup.transform, new Vector3(-9.5f, 0.95f, 10.5f), Quaternion.Euler(0, 90, 0));
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Bottle_with_Coffee_1.prefab", barGroup.transform, new Vector3(-9.5f, 0.95f, 12.5f), Quaternion.identity);
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Bottle_with_Coffee_2.prefab", barGroup.transform, new Vector3(-9.5f, 0.95f, 13.2f), Quaternion.identity);
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Board_1.prefab", barGroup.transform, new Vector3(-14f, 3.2f, 15.6f), Quaternion.identity);

        // ==========================================
        // 4. HỆ THỐNG 12 BÀN CAFE KHÁCH NGỒI (12 TABLES)
        // ==========================================
        GameObject tablesGroup = new GameObject("Cafe_Tables");
        tablesGroup.transform.SetParent(root.transform);

        // Bố trí 12 bàn thành 2 cánh trái/phải rộng rãi
        Vector3[] tablePositions = new Vector3[]
        {
            // Cánh Trái (Bàn 1 - 6)
            new Vector3(-15f, 0f, 2f),
            new Vector3(-15f, 0f, -4f),
            new Vector3(-15f, 0f, -10f),
            new Vector3(-8f, 0f, 2f),
            new Vector3(-8f, 0f, -4f),
            new Vector3(-8f, 0f, -10f),

            // Cánh Phải (Bàn 7 - 12)
            new Vector3(8f, 0f, 2f),
            new Vector3(8f, 0f, -4f),
            new Vector3(8f, 0f, -10f),
            new Vector3(15f, 0f, 2f),
            new Vector3(15f, 0f, -4f),
            new Vector3(15f, 0f, -10f)
        };

        for (int i = 0; i < tablePositions.Length; i++)
        {
            int tableIndex = i + 1;
            Vector3 pos = tablePositions[i];

            GameObject tableObj = SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Table_1.prefab", tablesGroup.transform, pos, Quaternion.identity);
            if (tableObj == null)
            {
                tableObj = SpawnPrefab("Assets/Interior Asset/Prefabs/Table.prefab", tablesGroup.transform, pos, Quaternion.identity);
            }
            if (tableObj != null)
            {
                tableObj.name = "Table_" + tableIndex;

                // 2 Ghế 2 bên bàn
                SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Chair_1.prefab", tableObj.transform, pos + new Vector3(-1.15f, 0f, 0f), Quaternion.Euler(0, 90, 0));
                SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Chair_1.prefab", tableObj.transform, pos + new Vector3(1.15f, 0f, 0f), Quaternion.Euler(0, -90, 0));

                // Gắn TableSeat
                TableSeat seat = tableObj.GetComponent<TableSeat>();
                if (seat == null) seat = tableObj.AddComponent<TableSeat>();
                seat.tableId = tableIndex;
                seat.isUnlocked = (tableIndex <= 4); // 4 bàn đầu mở sẵn, các bàn sau mở qua Shop

                // SitPoint
                Transform sit = tableObj.transform.Find("SitPoint");
                if (sit == null)
                {
                    GameObject sitObj = new GameObject("SitPoint");
                    sitObj.transform.SetParent(tableObj.transform);
                    sitObj.transform.position = pos + new Vector3(-1.15f, 0.4f, 0f);
                    seat.sitPoint = sitObj.transform;
                }
                else seat.sitPoint = sit;

                // FoodPoint
                Transform foodP = tableObj.transform.Find("FoodPoint");
                if (foodP == null)
                {
                    GameObject foodPObj = new GameObject("FoodPoint");
                    foodPObj.transform.SetParent(tableObj.transform);
                    foodPObj.transform.position = pos + new Vector3(0f, 0.85f, 0f);
                    seat.foodPoint = foodPObj.transform;
                }
                else seat.foodPoint = foodP;
            }
        }

        // ==========================================
        // 5. THIÊN ĐƯỜNG MÈO HOÀNG GIA (CAT PARADISE ZONE)
        // ==========================================
        GameObject catZoneGroup = new GameObject("Grand_Cat_Paradise");
        catZoneGroup.transform.SetParent(root.transform);

        // 2 Thảm tròn nhung lớn
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Carpet_1.prefab", catZoneGroup.transform, new Vector3(14f, 0.02f, 10f), Quaternion.identity);
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Carpet_1.prefab", catZoneGroup.transform, new Vector3(18f, 0.02f, 10f), Quaternion.identity);

        // 3 Ghế sofa bành êm cho mèo nằm ngủ
        SpawnPrefab("Assets/Interior Asset/Prefabs/ArmChair.prefab", catZoneGroup.transform, new Vector3(12.5f, 0f, 13f), Quaternion.Euler(0, -30, 0));
        SpawnPrefab("Assets/Interior Asset/Prefabs/ArmChair.prefab", catZoneGroup.transform, new Vector3(16f, 0f, 14f), Quaternion.Euler(0, 0, 0));
        SpawnPrefab("Assets/Interior Asset/Prefabs/ArmChair.prefab", catZoneGroup.transform, new Vector3(19.5f, 0f, 13f), Quaternion.Euler(0, 30, 0));

        // 4 Bát thức ăn cho mèo (FoodBowl)
        CreateFoodBowl(catZoneGroup.transform, new Vector3(13f, 0f, 7.5f), "FoodBowl_1");
        CreateFoodBowl(catZoneGroup.transform, new Vector3(14.5f, 0f, 7.5f), "FoodBowl_2");
        CreateFoodBowl(catZoneGroup.transform, new Vector3(17f, 0f, 7.5f), "FoodBowl_3");
        CreateFoodBowl(catZoneGroup.transform, new Vector3(18.5f, 0f, 7.5f), "FoodBowl_4");

        // Cây cảnh xanh khu vực mèo
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Plant_1.prefab", catZoneGroup.transform, new Vector3(20.5f, 0f, 14.5f), Quaternion.identity);
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Plant_1.prefab", catZoneGroup.transform, new Vector3(11.5f, 0f, 14.5f), Quaternion.identity);

        // ==========================================
        // 6. GÓC ĐỌC SÁCH & TRANG TRÍ (READING & DECOR)
        // ==========================================
        GameObject decorGroup = new GameObject("Reading_And_Decor");
        decorGroup.transform.SetParent(root.transform);

        // Kệ sách dài áp tường bên phải
        SpawnPrefab("Assets/Interior Asset/Prefabs/Shelf.prefab", decorGroup.transform, new Vector3(21.2f, 0f, 3f), Quaternion.Euler(0, -90, 0));
        SpawnPrefab("Assets/Interior Asset/Prefabs/Shelf.prefab", decorGroup.transform, new Vector3(21.2f, 0f, -3f), Quaternion.Euler(0, -90, 0));
        SpawnPrefab("Assets/Interior Asset/Prefabs/Shelf.prefab", decorGroup.transform, new Vector3(21.2f, 0f, -9f), Quaternion.Euler(0, -90, 0));

        // Ghế đọc sách thư giãn
        SpawnPrefab("Assets/Interior Asset/Prefabs/ArmChair.prefab", decorGroup.transform, new Vector3(19f, 0f, -6f), Quaternion.Euler(0, -90, 0));

        // Chậu cây cảnh lớn chào đón ở 4 góc phòng và lối ra vào
        SpawnPrefab("Assets/Interior Asset/Prefabs/Pot2.prefab", decorGroup.transform, new Vector3(-20.5f, 0f, -14.5f), Quaternion.identity);
        SpawnPrefab("Assets/Interior Asset/Prefabs/Pot2.prefab", decorGroup.transform, new Vector3(20.5f, 0f, -14.5f), Quaternion.identity);
        SpawnPrefab("Assets/Interior Asset/Prefabs/Pot2.prefab", decorGroup.transform, new Vector3(-4.5f, 0f, -14.5f), Quaternion.identity);
        SpawnPrefab("Assets/Interior Asset/Prefabs/Pot2.prefab", decorGroup.transform, new Vector3(4.5f, 0f, -14.5f), Quaternion.identity);

        // Tranh treo tường nghệ thuật
        SpawnPrefab("Assets/Interior Asset/Prefabs/Pic1.prefab", decorGroup.transform, new Vector3(2f, 3.2f, 15.6f), Quaternion.identity);
        SpawnPrefab("Assets/Interior Asset/Prefabs/Pic2.prefab", decorGroup.transform, new Vector3(6f, 3.2f, 15.6f), Quaternion.identity);

        // Biển hiệu hạt cà phê phát sáng
        SpawnPrefab("Assets/Mnostva Art/FREE_Interiors_2/Prefabs/Cafe_Coffee_Bean_Sign_1.prefab", decorGroup.transform, new Vector3(-21.6f, 3.5f, 5f), Quaternion.Euler(0, 90, 0));

        // ==========================================
        // 7. HỆ THỐNG ÁNH SÁNG ẤM CÚNG TOÀN DIỆN
        // ==========================================
        GameObject lightsGroup = new GameObject("Grand_Lights");
        lightsGroup.transform.SetParent(root.transform);

        // Nắng vàng ấm
        Light dirLight = FindFirstObjectByType<Light>();
        if (dirLight != null && dirLight.type == LightType.Directional)
        {
            dirLight.color = new Color(1f, 0.95f, 0.88f);
            dirLight.intensity = 1.15f;
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // Đèn ấm quầy bar
        CreatePointLight(lightsGroup.transform, "Light_Bar", new Vector3(-13.5f, 4f, 11f), new Color(1f, 0.82f, 0.55f), 1.8f, 14f);
        // Đèn ấm khu mèo
        CreatePointLight(lightsGroup.transform, "Light_CatZone", new Vector3(16f, 4f, 11f), new Color(1f, 0.88f, 0.65f), 1.8f, 14f);
        // Đèn ấm cánh trái bàn ăn
        CreatePointLight(lightsGroup.transform, "Light_LeftDining", new Vector3(-11f, 4.2f, -4f), new Color(1f, 0.85f, 0.60f), 1.7f, 16f);
        // Đèn ấm cánh phải bàn ăn
        CreatePointLight(lightsGroup.transform, "Light_RightDining", new Vector3(11f, 4.2f, -4f), new Color(1f, 0.85f, 0.60f), 1.7f, 16f);
        // Đèn ấm sảnh vào thảm đỏ
        CreatePointLight(lightsGroup.transform, "Light_Entrance", new Vector3(0f, 4f, -10f), new Color(1f, 0.80f, 0.50f), 1.5f, 12f);

        // ==========================================
        // 8. ĐẶT NHÂN VẬT & MÈO & CAMERA
        // ==========================================
        GameObject player = GameObject.Find("Characters ") ?? GameObject.Find("Characters");
        if (player != null)
        {
            player.transform.position = new Vector3(-12f, 0f, 6f);
            player.transform.rotation = Quaternion.Euler(0, 180, 0);

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
                if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
                camFollow.player = player.transform;
                camFollow.offset = new Vector3(0f, 15f, -14f); // Tầm nhìn bao quát cao rộng
                camFollow.smoothSpeed = 7f;
            }
        }

        // Đặt mèo vào thiên đường mèo
        GameObject cat1 = GameObject.Find("cat");
        if (cat1 != null) cat1.transform.position = new Vector3(14f, 0f, 10f);

        GameObject cat2 = GameObject.Find("cat2");
        if (cat2 != null) cat2.transform.position = new Vector3(17f, 0f, 11f);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("===> [CozyCafeMapBuilder] THÀNH CÔNG RỰC RỠ! Đã mở rộng đại bản đồ 44m x 32m!");
        return root;
    }

    private static void CreateWall(string name, Transform parent, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = size;
        wall.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static GameObject SpawnPrefab(string path, Transform parent, Vector3 pos, Quaternion rot)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return null;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        if (instance != null)
        {
            instance.transform.position = pos;
            instance.transform.rotation = rot;
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Cafe Object");
        }
        return instance;
    }

    private static void CreateFoodBowl(Transform parent, Vector3 pos, string name)
    {
        GameObject bowlObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bowlObj.name = name;
        bowlObj.transform.SetParent(parent);
        bowlObj.transform.position = pos + new Vector3(0f, 0.08f, 0f);
        bowlObj.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);

        Material bowlMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bowlMat.color = new Color(0.2f, 0.6f, 0.9f);
        bowlObj.GetComponent<MeshRenderer>().sharedMaterial = bowlMat;

        // Visual thức ăn bên trong
        GameObject foodObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        foodObj.name = "FoodMesh";
        foodObj.transform.SetParent(bowlObj.transform);
        foodObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        foodObj.transform.localScale = new Vector3(0.85f, 0.5f, 0.85f);
        Material foodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        foodMat.color = new Color(0.6f, 0.35f, 0.15f);
        foodObj.GetComponent<MeshRenderer>().sharedMaterial = foodMat;

        FoodBowl bowl = bowlObj.AddComponent<FoodBowl>();
        bowl.foodVisual = foodObj;
        bowl.FillFood();
    }

    private static void CreatePointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.SetParent(parent);
        lightObj.transform.position = pos;

        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.Soft;
    }
}
#endif
