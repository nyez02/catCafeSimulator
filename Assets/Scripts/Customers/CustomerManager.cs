using UnityEngine;
using System.Collections;

public class CustomerManager : MonoBehaviour
{
    [Header("Cài đặt Khách hàng")]
    public GameObject customerPrefab; // Vật thể khách hàng
    public Transform spawnPoint;      // Điểm khách xuất hiện (Cửa ra vào)
    public float spawnInterval = 10f; // Cứ 10 giây có 1 khách

    private void Start()
    {
        // Bắt đầu vòng lặp sinh ra khách hàng liên tục
        if (customerPrefab != null && spawnPoint != null)
        {
            StartCoroutine(SpawnCustomerRoutine());
        }
        else
        {
            Debug.LogWarning("Chưa gắn CustomerPrefab hoặc SpawnPoint trong Inspector!");
        }
    }

    private IEnumerator SpawnCustomerRoutine()
    {
        while (true) // Chạy mãi mãi
        {
            // Sinh ra 1 khách hàng mới tại vị trí cửa ra vào
            GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Lấy script CustomerAI và gọi hàm khởi tạo để truyền vị trí cửa về
            CustomerAI ai = newCustomer.GetComponent<CustomerAI>();
            if (ai != null)
            {
                ai.Initialize(spawnPoint);
            }
            
            // Chờ 1 khoảng thời gian rồi mới sinh ra khách tiếp theo
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
