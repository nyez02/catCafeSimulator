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
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        StartCoroutine(SpawnCustomerRoutine());
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

        ai.Initialize(spawnPoint);
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
