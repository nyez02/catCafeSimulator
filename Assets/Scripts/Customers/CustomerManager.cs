using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Customer Settings")]
    public GameObject defaultCustomerPrefab;
    public List<GameObject> customerPrefabs = new List<GameObject>();
    public Transform spawnPoint;
    public float spawnInterval = 8f;
    public int maxConcurrentCustomers = 6;

    private int currentCustomerCount = 0;

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
        EnsureSpawnPoint();
        EnsureCustomerPrefabs();
        StartCoroutine(SpawnCustomerRoutine());
    }

    private void EnsureSpawnPoint()
    {
        if (spawnPoint == null || spawnPoint == transform)
        {
            GameObject entranceObj = GameObject.Find("Entrance_SpawnPoint");
            if (entranceObj == null)
            {
                entranceObj = new GameObject("Entrance_SpawnPoint");
                entranceObj.transform.position = new Vector3(0f, 0.05f, -15.5f);
            }
            spawnPoint = entranceObj.transform;
        }
    }

    private void EnsureCustomerPrefabs()
    {
        if (customerPrefabs.Count == 0 && defaultCustomerPrefab == null)
        {
            // Tự động tìm kiếm các prefab nhân vật khách trong Resources hoặc gói PartyCharacters
            var allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var p in allPrefabs)
            {
                if (p.name.StartsWith("Character_") && !p.name.Contains("Clone") && p.scene.name == null)
                {
                    if (!customerPrefabs.Contains(p)) customerPrefabs.Add(p);
                }
            }
            if (customerPrefabs.Count > 0)
            {
                defaultCustomerPrefab = customerPrefabs[0];
            }
        }
    }

    private IEnumerator SpawnCustomerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Chỉ sinh khách nếu chưa vượt quá số lượng tối đa và có bàn hoặc sắp có bàn
            if (currentCustomerCount < maxConcurrentCustomers)
            {
                SpawnCustomer();
            }
        }
    }

    public void SpawnCustomer()
    {
        GameObject prefabToSpawn = GetRandomCustomerPrefab();
        if (prefabToSpawn == null || spawnPoint == null) return;

        GameObject customerObj = null;
        if (ObjectPoolManager.Instance != null)
        {
            customerObj = ObjectPoolManager.Instance.GetCustomer(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            customerObj = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }

        currentCustomerCount++;

        CustomerAI ai = customerObj.GetComponent<CustomerAI>();
        if (ai == null)
        {
            ai = customerObj.AddComponent<CustomerAI>();
        }

        bool isVIP = Random.value < 0.15f; // 15% cơ hội xuất hiện khách VIP
        ai.Initialize(spawnPoint, isVIP);

        if (isVIP && UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText("👑 KHÁCH VIP ĐÃ ĐẾN!", spawnPoint.position + Vector3.up * 2.2f, Color.yellow);
        }
    }

    private GameObject GetRandomCustomerPrefab()
    {
        if (customerPrefabs != null && customerPrefabs.Count > 0)
        {
            return customerPrefabs[Random.Range(0, customerPrefabs.Count)];
        }
        return defaultCustomerPrefab;
    }

    public void OnCustomerLeft()
    {
        currentCustomerCount = Mathf.Max(0, currentCustomerCount - 1);
    }
}
