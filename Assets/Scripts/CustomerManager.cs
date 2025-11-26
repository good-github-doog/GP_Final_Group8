using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static CustomerManager Instance;

    // ==================== SPAWN CONFIGURATION ====================
    [Header("Spawn Configuration")]
    public List<GameObject> customerPrefabs = new List<GameObject>();
    public Transform spawnPoint;
    public List<CustomerSpot> customerSpots = new List<CustomerSpot>();
    public float spawnInterval = 3f;

    private float spawnTimer = 0f;
    private bool isAllSpotsFull = false;

    // ==================== MOVEMENT CONFIGURATION ====================
    [Header("Movement Configuration")]
    public Transform leavePoint;
    public Transform doorOuterPoint;  // Waypoint when entering
    public Transform doorInnerPoint;  // Waypoint when leaving

    // ==================== PANIC SYSTEM CONFIGURATION ====================
    [Header("Panic System Configuration")]
    public List<Transform> panicPoints = new List<Transform>();
    public int minPanicStops = 2;
    public int maxPanicStops = 4;
    public float panicMoveSpeed = 5f;
    public float panicPointOffset = 2f;

    private Dictionary<Transform, int> panicPointUsageCount = new Dictionary<Transform, int>();
    private int currentPanicCustomerCount = 0;

    // ==================== INPUT CONFIGURATION ====================
    [Header("Input Configuration")]
    public KeyCode spotAKey = KeyCode.Alpha1;
    public KeyCode spotBKey = KeyCode.Alpha2;
    public KeyCode spotCKey = KeyCode.Alpha3;
    public KeyCode spotDKey = KeyCode.Alpha4;

    // ==================== UNITY LIFECYCLE ====================
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandlePlayerInput();
        HandleCustomerSpawning();
    }

    // ==================== INPUT HANDLING ====================
    private void HandlePlayerInput()
    {
        if (Input.GetKeyDown(spotAKey) && customerSpots.Count > 0)
        {
            KillCustomerAtSpot(0);
        }

        if (Input.GetKeyDown(spotBKey) && customerSpots.Count > 1)
        {
            KillCustomerAtSpot(1);
        }

        if (Input.GetKeyDown(spotCKey) && customerSpots.Count > 2)
        {
            KillCustomerAtSpot(2);
        }

        if (Input.GetKeyDown(spotDKey) && customerSpots.Count > 3)
        {
            KillCustomerAtSpot(3);
        }
    }

    // ==================== CUSTOMER INTERACTION ====================
    private void KillCustomerAtSpot(int spotIndex)
    {
        CustomerSpot spot = customerSpots[spotIndex];

        if (spot.IsOccupied && spot.CurrentCustomer != null)
        {
            Customer customer = spot.CurrentCustomer;
            KillCustomer(customer, spotIndex);
        }
    }

    private void KillCustomer(Customer customer, int spotIndex)
    {
        CustomerType customerType = customer.GetCustomerType();

        Debug.Log($"[CustomerManager] Customer killed by player. Type: {customerType}");

        GiveKillReward(customerType);

        Destroy(customer.gameObject);

        // Increment kill counter
        data.killCountToday++;
        Debug.Log($"[CustomerManager] Kill count today: {data.killCountToday}");

        // Check if 3 kills reached - switch to Home scene
        if (data.killCountYesterday + data.killCountToday >= 3)
        {
            Debug.Log("[CustomerManager] 3 kills reached! Resetting game data and switching to Home scene...");
            data.money = 1000;
            data.inbag.Clear();
            data.killCountToday = 0;
            data.killCountYesterday = 0;
            SceneManager.LoadScene("Home");
            return;
        }

        TriggerPanicForOtherCustomers(spotIndex);
    }

    private void GiveKillReward(CustomerType customerType)
    {
        string ingredientName = "";

        switch (customerType)
        {
            case CustomerType.Cow:
                ingredientName = "beef";
                break;
            case CustomerType.Pig:
                ingredientName = "pork";
                break;
            case CustomerType.Salmon:
                ingredientName = "salmon";
                break;
        }

        if (string.IsNullOrEmpty(ingredientName)) return;

        var existingIngredient = data.inbag.Find(x => x.name == ingredientName);

        if (existingIngredient != null)
        {
            existingIngredient.quantity += 1;
        }
        else
        {
            data.ingreds_data newIngredient = new data.ingreds_data(ingredientName)
            {
                quantity = 1
            };
            data.inbag.Add(newIngredient);
        }

        RefreshInventoryUI();

        Debug.Log($"[CustomerManager] Reward given: {ingredientName} x1");
    }

    private void RefreshInventoryUI()
    {
        IngredientManager ingredientManager = FindFirstObjectByType<IngredientManager>();
        ingredientManager?.RefreshSlots();
    }

    private void TriggerPanicForOtherCustomers(int excludeSpotIndex)
    {
        int panicCount = 0;
        float delayIncrement = 0.5f;

        for (int i = 0; i < customerSpots.Count; i++)
        {
            if (i != excludeSpotIndex && customerSpots[i].IsOccupied && customerSpots[i].CurrentCustomer != null)
            {
                Customer otherCustomer = customerSpots[i].CurrentCustomer;
                float delay = panicCount * delayIncrement;
                StartCoroutine(StartPanicSequenceWithDelay(otherCustomer, delay));
                panicCount++;
            }
        }

        currentPanicCustomerCount += panicCount;
        Debug.Log($"[CustomerManager] Customer killed. {panicCount} other customers triggered to panic.");
    }

    // ==================== SPAWN SYSTEM ====================
    private void HandleCustomerSpawning()
    {
        if (isAllSpotsFull) return;
        if (currentPanicCustomerCount > 0) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            TrySpawnNewCustomer();
            spawnTimer = 0f;
        }
    }

    private void TrySpawnNewCustomer()
    {
        CustomerSpot availableSpot = FindAvailableSpot();

        if (availableSpot == null)
        {
            isAllSpotsFull = true;
            return;
        }

        SpawnCustomerAtSpot(availableSpot);
    }

    private void SpawnCustomerAtSpot(CustomerSpot targetSpot)
    {
        if (customerPrefabs == null || customerPrefabs.Count == 0)
        {
            Debug.LogError("[CustomerManager] No customer prefabs assigned!");
            return;
        }

        GameObject selectedPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
        GameObject customerObject = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        if (doorOuterPoint != null)
        {
            List<Vector3> waypoints = new List<Vector3> { doorOuterPoint.position };
            customer.SetDestinationWithWaypoints(targetSpot, waypoints);
        }
        else
        {
            customer.SetDestination(targetSpot);
        }
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

    // ==================== MOVEMENT SYSTEM ====================
    public void MoveCustomerToLeavePoint(Customer customer)
    {
        if (leavePoint == null)
        {
            Debug.LogError("[CustomerManager] Leave Point not set!");
            return;
        }

        if (doorInnerPoint != null)
        {
            List<Vector3> waypoints = new List<Vector3> { doorInnerPoint.position };
            customer.SetDestinationToLeavePointWithWaypoints(leavePoint.position, waypoints);
        }
        else
        {
            customer.SetDestinationToLeavePoint(leavePoint.position);
        }
    }

    public void RemoveCustomer(Customer customer)
    {
        if (customer != null)
        {
            if (customer.IsPanicking())
            {
                currentPanicCustomerCount--;
                currentPanicCustomerCount = Mathf.Max(0, currentPanicCustomerCount);
            }

            Destroy(customer.gameObject);
        }

        isAllSpotsFull = false;
    }

    public void NotifyCustomerLeft()
    {
        isAllSpotsFull = false;
    }

    // ==================== PANIC SYSTEM ====================
    private System.Collections.IEnumerator StartPanicSequenceWithDelay(Customer customer, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Set navigation priority based on delay
        UnityEngine.AI.NavMeshAgent agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            int priority = 50 - Mathf.RoundToInt(delay * 10);
            agent.avoidancePriority = Mathf.Clamp(priority, 10, 99);
        }

        yield return StartCoroutine(ExecutePanicBehavior(customer));
    }

    private System.Collections.IEnumerator ExecutePanicBehavior(Customer customer)
    {
        if (customer == null)
        {
            yield break;
        }

        customer.SetPanicking(true);

        UnityEngine.AI.NavMeshAgent agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("[CustomerManager] Customer has no NavMeshAgent!");
            yield break;
        }

        float originalSpeed = agent.speed;

        // Rotate customer 180 degrees
        agent.updateRotation = false;
        customer.transform.Rotate(0, 180, 0);
        yield return new WaitForSeconds(0.3f);

        if (customer == null || agent == null) yield break;

        agent.updateRotation = true;

        // Move through panic points
        if (panicPoints.Count > 0)
        {
            agent.speed = panicMoveSpeed;

            int numberOfStops = Random.Range(minPanicStops, maxPanicStops + 1);
            numberOfStops = Mathf.Min(numberOfStops, panicPoints.Count);

            List<Transform> selectedPoints = SelectLeastUsedPanicPoints(numberOfStops);

            // Reserve panic points
            foreach (Transform point in selectedPoints)
            {
                IncrementPanicPointUsage(point);
            }

            // Visit each panic point
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                if (customer == null || agent == null)
                {
                    // Cleanup panic points before breaking
                    foreach (Transform point in selectedPoints)
                    {
                        DecrementPanicPointUsage(point);
                    }
                    yield break;
                }

                Transform panicPoint = selectedPoints[i];
                Vector3 targetPosition = GetRandomOffsetPosition(panicPoint.position);

                agent.SetDestination(targetPosition);

                yield return new WaitUntil(() =>
                    customer == null ||
                    agent == null ||
                    (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
                );

                if (customer == null || agent == null)
                {
                    // Cleanup panic points before breaking
                    foreach (Transform point in selectedPoints)
                    {
                        DecrementPanicPointUsage(point);
                    }
                    yield break;
                }

                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }

            // Release panic points
            foreach (Transform point in selectedPoints)
            {
                DecrementPanicPointUsage(point);
            }

            agent.speed = originalSpeed;
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        if (customer != null && agent != null)
        {
            MoveCustomerToLeavePoint(customer);
        }
    }

    // ==================== PANIC POINT MANAGEMENT ====================
    private List<Transform> SelectLeastUsedPanicPoints(int count)
    {
        // Initialize usage count for new points
        foreach (Transform point in panicPoints)
        {
            if (!panicPointUsageCount.ContainsKey(point))
            {
                panicPointUsageCount[point] = 0;
            }
        }

        // Sort by usage count
        List<Transform> sortedPoints = new List<Transform>(panicPoints);
        sortedPoints.Sort((a, b) => panicPointUsageCount[a].CompareTo(panicPointUsageCount[b]));

        // Select least used points
        List<Transform> selectedPoints = new List<Transform>();
        for (int i = 0; i < Mathf.Min(count, sortedPoints.Count); i++)
        {
            selectedPoints.Add(sortedPoints[i]);
        }

        ShuffleList(selectedPoints);

        return selectedPoints;
    }

    private void IncrementPanicPointUsage(Transform point)
    {
        if (!panicPointUsageCount.ContainsKey(point))
        {
            panicPointUsageCount[point] = 0;
        }
        panicPointUsageCount[point]++;
    }

    private void DecrementPanicPointUsage(Transform point)
    {
        if (panicPointUsageCount.ContainsKey(point))
        {
            panicPointUsageCount[point]--;
            panicPointUsageCount[point] = Mathf.Max(0, panicPointUsageCount[point]);
        }
    }

    // ==================== UTILITY FUNCTIONS ====================
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private Vector3 GetRandomOffsetPosition(Vector3 originalPosition)
    {
        float randomX = Random.Range(-panicPointOffset, panicPointOffset);
        float randomZ = Random.Range(-panicPointOffset, panicPointOffset);

        return new Vector3(
            originalPosition.x + randomX,
            originalPosition.y,
            originalPosition.z + randomZ
        );
    }
}
