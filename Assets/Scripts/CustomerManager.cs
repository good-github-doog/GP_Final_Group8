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

    private bool isFull = false;   // ⭐ 是否滿座

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
    }

    // ---------------------- Spawn 客人 ----------------------

    private void TrySpawnCustomer()
    {
        CustomerSpot availableSpot = FindAvailableSpot();

        if (availableSpot == null)
        {
            Debug.Log("No available spots for customer!");
            isFull = true; // ⭐ 設為滿座
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