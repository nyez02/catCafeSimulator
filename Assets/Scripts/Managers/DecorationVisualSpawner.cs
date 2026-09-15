using System.Collections.Generic;
using UnityEngine;

public class DecorationVisualSpawner : MonoBehaviour
{
    public static DecorationVisualSpawner Instance { get; private set; }

    [System.Serializable]
    public class DecorationVisualMapping
    {
        public string decorId;
        public GameObject sceneObject;
        public GameObject prefab;
        public Transform spawnPoint;
        [HideInInspector] public GameObject spawnedInstance;
    }

    [Header("Visual Mappings")]
    public List<DecorationVisualMapping> mappings = new List<DecorationVisualMapping>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (DecorationManager.Instance != null)
        {
            DecorationManager.Instance.OnDecorationUnlocked += HandleDecorationUnlocked;
            RefreshAllDecorations();
        }
    }

    private void OnDestroy()
    {
        if (DecorationManager.Instance != null)
        {
            DecorationManager.Instance.OnDecorationUnlocked -= HandleDecorationUnlocked;
        }
    }

    public void RefreshAllDecorations()
    {
        if (DecorationManager.Instance == null) return;

        foreach (var mapping in mappings)
        {
            bool isUnlocked = DecorationManager.Instance.IsUnlocked(mapping.decorId);
            SetDecorationVisible(mapping, isUnlocked, false);
        }
    }

    private void HandleDecorationUnlocked(DecorationItem item)
    {
        if (item == null) return;

        var mapping = mappings.Find(m => m.decorId == item.id);
        if (mapping != null)
        {
            SetDecorationVisible(mapping, true, true);
        }
        else
        {
            // Tự động tìm kiếm đối tượng trong Scene có tên tương tự
            TryAutoActivateSceneObject(item.id);
        }

        // Thông báo Toast & hiệu ứng
        ToastManager.Instance?.ShowToast($"Nội thất mới: {item.itemName} đã được bày biện trong quán! ✨", "🛋️", new Color(0.3f, 0.6f, 0.2f, 0.95f));
    }

    private void SetDecorationVisible(DecorationVisualMapping mapping, bool visible, bool playVfx)
    {
        if (mapping.sceneObject != null)
        {
            mapping.sceneObject.SetActive(visible);
            if (visible && playVfx && CatVFXManager.Instance != null)
            {
                CatVFXManager.Instance.PlayConfetti(mapping.sceneObject.transform.position + Vector3.up * 0.5f);
                SoundManager.Instance?.PlayCoin();
            }
        }
        else if (mapping.prefab != null)
        {
            if (visible)
            {
                if (mapping.spawnedInstance == null)
                {
                    Vector3 pos = mapping.spawnPoint != null ? mapping.spawnPoint.position : Vector3.zero;
                    Quaternion rot = mapping.spawnPoint != null ? mapping.spawnPoint.rotation : Quaternion.identity;
                    mapping.spawnedInstance = Instantiate(mapping.prefab, pos, rot);
                    if (playVfx && CatVFXManager.Instance != null)
                    {
                        CatVFXManager.Instance.PlayConfetti(pos + Vector3.up * 0.5f);
                        SoundManager.Instance?.PlayCoin();
                    }
                }
            }
            else
            {
                if (mapping.spawnedInstance != null)
                {
                    Destroy(mapping.spawnedInstance);
                    mapping.spawnedInstance = null;
                }
            }
        }
    }

    private void TryAutoActivateSceneObject(string decorId)
    {
        // Fallback: Tìm các vật thể 3D ẩn trong scene
        string searchKeyword = decorId.ToLower();
        var allTransforms = FindObjectsOfType<Transform>(true);
        foreach (var t in allTransforms)
        {
            if (t.gameObject.name.ToLower().Contains(searchKeyword) && !t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
                CatVFXManager.Instance?.PlayConfetti(t.position + Vector3.up * 0.5f);
                break;
            }
        }
    }
}
