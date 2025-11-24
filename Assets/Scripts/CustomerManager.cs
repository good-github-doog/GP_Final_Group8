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

    [Header("Door Waypoints")]
    public Transform doorOuterPoint;
    public Transform doorInnerPoint;

    [Header("Panic Points")]
    public List<Transform> panicPoints = new List<Transform>();
    public int minPanicStops = 2;
    public int maxPanicStops = 4;
    public float panicMoveSpeed = 5f;
    public float panicPointOffset = 2f;

    private Dictionary<Transform, int> panicPointUsageCount = new Dictionary<Transform, int>();

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    private float spawnTimer = 0f;
    private bool isFull = false;
    private int panicCustomerCount = 0;

    [Header("Input Settings")]
    public KeyCode spotAKey = KeyCode.Alpha1;
    public KeyCode spotBKey = KeyCode.Alpha2;
    public KeyCode spotCKey = KeyCode.Alpha3;
    public KeyCode spotDKey = KeyCode.Alpha4;

    [Header("Reward Settings")]
    public string rewardIngredientName = "pork";
    public int rewardAmount = 1;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        HandleKeyInput();

        if (isFull) return;
        if (panicCustomerCount > 0) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            TrySpawnCustomer();
            spawnTimer = 0f;
        }
    }

    private void HandleKeyInput()
    {
        if (Input.GetKeyDown(spotAKey) && customerSpots.Count > 0)
        {
            ServeCustomerAtSpot(0);
        }

        if (Input.GetKeyDown(spotBKey) && customerSpots.Count > 1)
        {
            ServeCustomerAtSpot(1);
        }

        if (Input.GetKeyDown(spotCKey) && customerSpots.Count > 2)
        {
            ServeCustomerAtSpot(2);
        }

        if (Input.GetKeyDown(spotDKey) && customerSpots.Count > 3)
        {
            ServeCustomerAtSpot(3);
        }
    }

    private void ServeCustomerAtSpot(int spotIndex)
    {
        CustomerSpot spot = customerSpots[spotIndex];

        if (spot.IsOccupied && spot.CurrentCustomer != null)
        {
            Customer servedCustomer = spot.CurrentCustomer;
            Destroy(servedCustomer.gameObject);

            AddIngredientReward();

            int panicCount = 0;
            float delayIncrement = 0.5f;

            for (int i = 0; i < customerSpots.Count; i++)
            {
                if (i != spotIndex && customerSpots[i].IsOccupied && customerSpots[i].CurrentCustomer != null)
                {
                    Customer otherCustomer = customerSpots[i].CurrentCustomer;
                    float delay = panicCount * delayIncrement;
                    StartCoroutine(RotateAndLeaveCustomerWithDelay(otherCustomer, delay));
                    panicCount++;
                }
            }

            panicCustomerCount += panicCount;
        }
    }

    private void AddIngredientReward()
    {
        var existingIngredient = data.inbag.Find(x => x.name == rewardIngredientName);

        if (existingIngredient != null)
        {
            existingIngredient.quantity += rewardAmount;
        }
        else
        {
            data.ingreds_data newIngredient = new data.ingreds_data(rewardIngredientName);
            newIngredient.quantity = rewardAmount;
            data.inbag.Add(newIngredient);
        }

        IngredientManager ingredientManager = FindFirstObjectByType<IngredientManager>();
        if (ingredientManager != null)
        {
            ingredientManager.RefreshSlots();
        }
    }

    private void TrySpawnCustomer()
    {
        CustomerSpot availableSpot = FindAvailableSpot();

        if (availableSpot == null)
        {
            isFull = true;
            return;
        }

        SpawnCustomer(availableSpot);
    }

    private void SpawnCustomer(CustomerSpot spot)
    {
        GameObject customerObj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        Customer customer = customerObj.GetComponent<Customer>();

        if (doorOuterPoint != null)
        {
            List<Vector3> waypoints = new List<Vector3>();
            waypoints.Add(doorOuterPoint.position);
            customer.SetDestinationWithWaypoints(spot, waypoints);
        }
        else
        {
            customer.SetDestination(spot);
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

    public void MoveCustomerToLeavePoint(Customer customer)
    {
        if (leavePoint == null)
        {
            Debug.LogError("Leave Point 未設定！");
            return;
        }

        if (doorInnerPoint != null)
        {
            List<Vector3> waypoints = new List<Vector3>();
            waypoints.Add(doorInnerPoint.position);
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
                panicCustomerCount--;
                if (panicCustomerCount <= 0)
                {
                    panicCustomerCount = 0;
                }
            }

            Destroy(customer.gameObject);
        }

        isFull = false;
    }

    public void NotifyCustomerLeft()
    {
        isFull = false;
    }

    private System.Collections.IEnumerator RotateAndLeaveCustomerWithDelay(Customer customer, float delay)
    {
        yield return new WaitForSeconds(delay);

        UnityEngine.AI.NavMeshAgent agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            int priority = 50 - Mathf.RoundToInt(delay * 10);
            agent.avoidancePriority = Mathf.Clamp(priority, 10, 99);
        }

        yield return StartCoroutine(RotateAndLeaveCustomer(customer));
    }

    private System.Collections.IEnumerator RotateAndLeaveCustomer(Customer customer)
    {
        customer.SetPanicking(true);

        UnityEngine.AI.NavMeshAgent agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("顧客沒有 NavMeshAgent，無法移動！");
            yield break;
        }

        float originalSpeed = agent.speed;

        agent.updateRotation = false;
        customer.transform.Rotate(0, 180, 0);
        yield return new WaitForSeconds(0.3f);
        agent.updateRotation = true;

        if (panicPoints.Count > 0)
        {
            agent.speed = panicMoveSpeed;

            int panicStopCount = Random.Range(minPanicStops, maxPanicStops + 1);
            panicStopCount = Mathf.Min(panicStopCount, panicPoints.Count);

            List<Transform> selectedPoints = SelectLeastUsedPanicPoints(panicStopCount);

            foreach (Transform point in selectedPoints)
            {
                IncrementPanicPointUsage(point);
            }

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                Transform panicPoint = selectedPoints[i];
                Vector3 targetPosition = GetRandomOffsetPosition(panicPoint.position);

                agent.SetDestination(targetPosition);

                yield return new WaitUntil(() =>
                    !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance + 0.5f
                );

                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }

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

        MoveCustomerToLeavePoint(customer);
    }

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

    private List<Transform> SelectLeastUsedPanicPoints(int count)
    {
        foreach (Transform point in panicPoints)
        {
            if (!panicPointUsageCount.ContainsKey(point))
            {
                panicPointUsageCount[point] = 0;
            }
        }

        List<Transform> sortedPoints = new List<Transform>(panicPoints);
        sortedPoints.Sort((a, b) => panicPointUsageCount[a].CompareTo(panicPointUsageCount[b]));

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
            if (panicPointUsageCount[point] < 0)
            {
                panicPointUsageCount[point] = 0;
            }
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
