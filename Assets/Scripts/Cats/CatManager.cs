using System.Collections.Generic;
using UnityEngine;

public class CatManager : MonoBehaviour
{
    public static CatManager Instance { get; private set; }

    [Header("Cats in Cafe")]
    [SerializeField] private List<CatAI> activeCats = new List<CatAI>();

    [Header("Cat Catalogs")]
    public List<CatData> availableBreeds = new List<CatData>();
    public GameObject defaultCatPrefab;
    public Transform catSpawnPoint;

    public System.Action OnCatCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (activeCats.Count == 0)
        {
            activeCats.AddRange(FindObjectsByType<CatAI>(FindObjectsSortMode.None));
        }
    }

    public void RegisterCat(CatAI cat)
    {
        if (!activeCats.Contains(cat))
        {
            activeCats.Add(cat);
            OnCatCountChanged?.Invoke();
        }
    }

    public void UnregisterCat(CatAI cat)
    {
        if (activeCats.Contains(cat))
        {
            activeCats.Remove(cat);
            OnCatCountChanged?.Invoke();
        }
    }

    public int GetCatCount() => activeCats.Count;

    public List<CatAI> GetAllCats() => activeCats;

    /// <summary>
    /// Cho tất cả mèo ăn bằng cách làm đầy các bát thức ăn và nạp dinh dưỡng
    /// </summary>
    public void FeedAllCats()
    {
        foreach (var bowl in FoodBowl.AllBowls)
        {
            if (bowl != null)
            {
                bowl.FillFood();
            }
        }

        foreach (var cat in activeCats)
        {
            if (cat != null)
            {
                cat.hunger = Mathf.Min(100f, cat.hunger + 30f);
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCatMeow();
        }
    }

    /// <summary>
    /// Sinh ra một con mèo mới từ CatData
    /// </summary>
    public CatAI SpawnCat(CatData data)
    {
        GameObject prefabToUse = (data != null && data.catPrefab != null) ? data.catPrefab : defaultCatPrefab;

        if (prefabToUse == null)
        {
            Debug.LogWarning("Không có prefab để sinh mèo mới!");
            return null;
        }

        Vector3 spawnPos = catSpawnPoint != null ? catSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = catSpawnPoint != null ? catSpawnPoint.rotation : Quaternion.identity;

        GameObject catObj = Instantiate(prefabToUse, spawnPos, spawnRot);
        CatAI catAI = catObj.GetComponent<CatAI>();
        if (catAI == null)
        {
            catAI = catObj.AddComponent<CatAI>();
        }

        if (data != null)
        {
            catAI.catData = data;
            catAI.catName = data.catName;
        }

        RegisterCat(catAI);
        return catAI;
    }

    /// <summary>
    /// Lấy danh sách tên tất cả các con mèo hiện có để lưu game
    /// </summary>
    public List<string> GetOwnedCatNames()
    {
        List<string> names = new List<string>();
        foreach (var cat in activeCats)
        {
            if (cat != null)
            {
                names.Add(cat.catName);
            }
        }
        return names;
    }
}
