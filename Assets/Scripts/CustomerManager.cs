using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Prefab and Spawn")]
    public GameObject customerPrefab;
    public Transform spawnPoint;

    [Header("Customer Spots")]
    public List<CustomerSpot> customerSpots = new List<CustomerSpot>();

    [Header("Leave Point")]
    public Transform leavePoint;

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    private float spawnTimer = 0f;

    [Header("Input Settings")]
    public KeyCode spotAKey = KeyCode.A;
    public KeyCode spotBKey = KeyCode.B;
    public KeyCode spotCKey = KeyCode.C;
    public KeyCode spotDKey = KeyCode.D;

    [Header("Reward Settings")]
    public string rewardIngredientName = "pork"; // 獎勵的食材名稱
    public int rewardAmount = 1; // 每次獎勵的數量


    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 若滿座就不 spawn
        if (isFull) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            TrySpawnCustomer();
            spawnTimer = 0f;
        }
        HandleKeyInput();
    }

    private void HandleKeyInput()
    {
        // 檢查 A 鍵 - 對應 Spot 0
        if (Input.GetKeyDown(spotAKey) && customerSpots.Count > 0)
        {
            ServeCustomerAtSpot(0);
        }

        // 檢查 B 鍵 - 對應 Spot 1
        if (Input.GetKeyDown(spotBKey) && customerSpots.Count > 1)
        {
            ServeCustomerAtSpot(1);
        }

        // 檢查 C 鍵 - 對應 Spot 2
        if (Input.GetKeyDown(spotCKey) && customerSpots.Count > 2)
        {
            ServeCustomerAtSpot(2);
        }

        // 檢查 D 鍵 - 對應 Spot 3
        if (Input.GetKeyDown(spotDKey) && customerSpots.Count > 3)
        {
            ServeCustomerAtSpot(3);
        }
    }

    private void ServeCustomerAtSpot(int spotIndex)
    {
        CustomerSpot spot = customerSpots[spotIndex];

        if (spot.IsOccupied && spot.currentCustomer != null)
        {
            Debug.Log($"服務了 Spot {spotIndex} 的顧客！");

            // 移除顧客
            Customer customer = spot.currentCustomer;
            Destroy(customer.gameObject);

            // 給予獎勵
            AddIngredientReward();
        }
        else
        {
            Debug.Log($"Spot {spotIndex} 沒有顧客！");
        }
    }

    private void AddIngredientReward()
    {
        // 檢查 data.inbag 中是否已有該食材
        var existingIngredient = data.inbag.Find(x => x.name == rewardIngredientName);

        if (existingIngredient != null)
        {
            // 如果已存在，增加數量
            existingIngredient.quantity += rewardAmount;
            Debug.Log($"增加 {rewardIngredientName} x{rewardAmount}，現在總數：{existingIngredient.quantity}");
        }
        else
        {
            // 如果不存在，創建新的
            data.ingreds_data newIngredient = new data.ingreds_data(rewardIngredientName);
            newIngredient.quantity = rewardAmount;
            data.inbag.Add(newIngredient);
            Debug.Log($"獲得新食材 {rewardIngredientName} x{rewardAmount}");
        }

        // ⭐ 通知 IngredientManager 更新 UI（如果在同一場景）
        IngredientManager ingredientManager = FindObjectOfType<IngredientManager>();
        if (ingredientManager != null)
        {
            ingredientManager.RefreshSlots();
            Debug.Log("IngredientManager 已更新食材槽 UI");
        }
    }

    // ---------------------- Spawn 客人 ----------------------

    private void TrySpawnCustomer()
    {
        CustomerSpot availableSpot = FindAvailableSpot();

        if (availableSpot == null)
        {
            //Debug.Log("No available spots for customer!");
            return;
        }

        SpawnCustomer(availableSpot);
    }

    private void SpawnCustomer(CustomerSpot spot)
    {
        GameObject customerObj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        Customer customer = customerObj.GetComponent<Customer>();

        customer.SetDestination(spot);
    }

    private CustomerSpot FindAvailableSpot()
    {
        foreach (CustomerSpot spot in customerSpots)
        {
            if (!spot.IsOccupied)
            {
                return spot;
            }
        }
        return null;
    }

    // ---------------------- 離開邏輯 ----------------------

    public void MoveCustomerToLeavePoint(Customer customer)
    {
        if (leavePoint == null)
        {
            Debug.LogError("Leave Point 未設定！");
            return;
        }

        customer.SetDestinationToLeavePoint(leavePoint.position);
    }

    public void RemoveCustomer(Customer customer)
    {
        if (customer != null)
            Destroy(customer.gameObject);

        // ⭐ 一位客人離開 → 一定有座位空出 → 不再滿座
        isFull = false;
    }

    // 給 Customer.cs 用的接口，告訴 Manager 有客人離開座位
    public void NotifyCustomerLeft()
    {
        isFull = false;
    }
}