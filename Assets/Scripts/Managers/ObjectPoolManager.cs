using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Header("Floating Text Pool")]
    public GameObject floatingTextPrefab;
    public int initialFloatingTextPoolSize = 15;
    private readonly Queue<FloatingText> floatingTextPool = new Queue<FloatingText>();

    [Header("Customer Pool")]
    public int initialCustomerPoolSizePerPrefab = 4;
    private readonly Dictionary<string, Queue<GameObject>> customerPools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFloatingTextPool();
    }

    private void InitializeFloatingTextPool()
    {
        for (int i = 0; i < initialFloatingTextPoolSize; i++)
        {
            FloatingText ft = CreateNewFloatingTextInstance();
            ft.gameObject.SetActive(false);
            floatingTextPool.Enqueue(ft);
        }
    }

    private FloatingText CreateNewFloatingTextInstance()
    {
        GameObject obj;
        if (floatingTextPrefab != null)
        {
            obj = Instantiate(floatingTextPrefab, transform);
        }
        else
        {
            obj = new GameObject("PooledFloatingText");
            obj.transform.SetParent(transform);
        }

        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft == null)
        {
            ft = obj.AddComponent<FloatingText>();
        }

        return ft;
    }

    /// <summary>
    /// Lấy một đối tượng chữ nổi từ Pool để tái sử dụng, triệt tiêu GC Alloc
    /// </summary>
    public FloatingText GetFloatingText(string message, Vector3 worldPos, Color color)
    {
        FloatingText ft = null;

        while (floatingTextPool.Count > 0 && ft == null)
        {
            ft = floatingTextPool.Dequeue();
        }

        if (ft == null)
        {
            ft = CreateNewFloatingTextInstance();
        }

        ft.transform.position = worldPos;
        ft.gameObject.SetActive(true);
        ft.Setup(message, color);
        return ft;
    }

    public void ReturnFloatingText(FloatingText ft)
    {
        if (ft == null) return;

        ft.gameObject.SetActive(false);
        ft.transform.SetParent(transform);
        floatingTextPool.Enqueue(ft);
    }

    /// <summary>
    /// Lấy một khách hàng từ Pool theo prefab
    /// </summary>
    public GameObject GetCustomer(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        string key = prefab.name;
        if (!customerPools.ContainsKey(key))
        {
            customerPools[key] = new Queue<GameObject>();
        }

        Queue<GameObject> pool = customerPools[key];
        GameObject customer = null;

        while (pool.Count > 0 && customer == null)
        {
            customer = pool.Dequeue();
        }

        if (customer == null)
        {
            customer = Instantiate(prefab, position, rotation);
            customer.name = prefab.name; // Giữ nguyên tên để dùng làm key
        }
        else
        {
            customer.transform.position = position;
            customer.transform.rotation = rotation;
            customer.SetActive(true);
        }

        return customer;
    }

    public void ReturnCustomer(GameObject customer)
    {
        if (customer == null) return;

        customer.SetActive(false);
        customer.transform.SetParent(transform);

        string key = customer.name;
        if (!customerPools.ContainsKey(key))
        {
            customerPools[key] = new Queue<GameObject>();
        }

        customerPools[key].Enqueue(customer);
    }
}
