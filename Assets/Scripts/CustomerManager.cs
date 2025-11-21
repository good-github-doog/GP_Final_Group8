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
    [Tooltip("門外側參考點（顧客進入時會先經過此點）")]
    public Transform doorOuterPoint;

    [Tooltip("門內側參考點（顧客離開時會先經過此點）")]
    public Transform doorInnerPoint;

    [Header("Panic Points")]
    public List<Transform> panicPoints = new List<Transform>(); // 顧客慌亂時亂跑的位置
    public int minPanicStops = 2; // 最少跑幾個點
    public int maxPanicStops = 4; // 最多跑幾個點
    public float panicMoveSpeed = 5f; // 慌亂時的移動速度（比正常快）
    public float panicPointOffset = 2f; // 慌亂點的隨機偏移範圍，避免顧客撞在一起

    // 追蹤正在使用的慌亂點
    private Dictionary<Transform, int> panicPointUsageCount = new Dictionary<Transform, int>();

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    private float spawnTimer = 0f;

    // 追蹤正在慌亂離開的顧客數量
    private int panicCustomerCount = 0;
    [Header("Input Settings")]
    public KeyCode spotAKey = KeyCode.Alpha1;
    public KeyCode spotBKey = KeyCode.Alpha2;
    public KeyCode spotCKey = KeyCode.Alpha3;
    public KeyCode spotDKey = KeyCode.Alpha4;

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

        // 如果有顧客正在慌亂離開，暫停生成新顧客
        if (panicCustomerCount > 0)
        {
            HandleKeyInput();
            return;
        }

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
        // 檢查 1 鍵 - 對應 Spot 0
        if (Input.GetKeyDown(spotAKey) && customerSpots.Count > 0)
        {
            ServeCustomerAtSpot(0);
        }

        // 檢查 2 鍵 - 對應 Spot 1
        if (Input.GetKeyDown(spotBKey) && customerSpots.Count > 1)
        {
            ServeCustomerAtSpot(1);
        }

        // 檢查 3 鍵 - 對應 Spot 2
        if (Input.GetKeyDown(spotCKey) && customerSpots.Count > 2)
        {
            ServeCustomerAtSpot(2);
        }

        // 檢查 4 鍵 - 對應 Spot 3
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
            Debug.Log($"服務了 Spot {spotIndex} 的顧客！");

            // 移除被服務的顧客
            Customer servedCustomer = spot.CurrentCustomer;
            Destroy(servedCustomer.gameObject);

            // 給予獎勵
            AddIngredientReward();

            // 讓其他所有顧客離開（添加時間差避免擁擠）
            int panicCount = 0;
            float delayIncrement = 0.5f; // 每個顧客之間的時間間隔

            for (int i = 0; i < customerSpots.Count; i++)
            {
                if (i != spotIndex && customerSpots[i].IsOccupied && customerSpots[i].CurrentCustomer != null)
                {
                    Customer otherCustomer = customerSpots[i].CurrentCustomer;
                    Debug.Log($"讓 Spot {i} 的顧客離開");

                    // 每個顧客延遲不同的時間開始離開
                    float delay = panicCount * delayIncrement;
                    StartCoroutine(RotateAndLeaveCustomerWithDelay(otherCustomer, delay));

                    panicCount++;
                }
            }

            // 增加慌亂顧客計數
            panicCustomerCount += panicCount;
            Debug.Log($"[CustomerManager] {panicCount} 位顧客將陸續慌亂離開（間隔 {delayIncrement}秒），當前慌亂顧客總數：{panicCustomerCount}");
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

        // 如果有設置門的路徑點，使用路徑點系統
        if (doorOuterPoint != null)
        {
            List<Vector3> waypoints = new List<Vector3>();
            waypoints.Add(doorOuterPoint.position); // 先走到門外側參考點
            customer.SetDestinationWithWaypoints(spot, waypoints);
            Debug.Log("[CustomerManager] 顧客將先經過門外側參考點");
        }
        else
        {
            // 沒有設置路徑點，直接前往目的地
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

    // ---------------------- 離開邏輯 ----------------------

    public void MoveCustomerToLeavePoint(Customer customer)
    {
        if (leavePoint == null)
        {
            Debug.LogError("Leave Point 未設定！");
            return;
        }

        // 如果有設置門內側參考點，讓顧客先經過該點再離開
        if (doorInnerPoint != null)
        {
            List<Vector3> waypoints = new List<Vector3>();
            waypoints.Add(doorInnerPoint.position); // 先走到門內側參考點
            customer.SetDestinationToLeavePointWithWaypoints(leavePoint.position, waypoints);
            Debug.Log("[CustomerManager] 顧客離開時將先經過門內側參考點");
        }
        else
        {
            // 沒有設置路徑點，直接離開
            customer.SetDestinationToLeavePoint(leavePoint.position);
        }
    }

    public void RemoveCustomer(Customer customer)
    {
        if (customer != null)
        {
            // 如果這是慌亂的顧客，減少計數
            if (customer.IsPanicking())
            {
                panicCustomerCount--;
                Debug.Log($"[CustomerManager] 慌亂顧客已離開，剩餘慌亂顧客：{panicCustomerCount}");

                // 如果所有慌亂顧客都離開了，可以重新開始生成
                if (panicCustomerCount <= 0)
                {
                    panicCustomerCount = 0;
                    Debug.Log("[CustomerManager] 所有慌亂顧客已離開，恢復生成新顧客");
                }
            }

            Destroy(customer.gameObject);
        }

        // ⭐ 一位客人離開 → 一定有座位空出 → 不再滿座
        isFull = false;
    }

    // 給 Customer.cs 用的接口，告訴 Manager 有客人離開座位
    public void NotifyCustomerLeft()
    {
        isFull = false;
    }

    // ---------------------- 慌亂並離開 ----------------------

    // 帶延遲的慌亂離開（避免多個顧客同時離開造成擁擠）
    private System.Collections.IEnumerator RotateAndLeaveCustomerWithDelay(Customer customer, float delay)
    {
        // 等待指定的延遲時間
        yield return new WaitForSeconds(delay);

        // 設置避障優先級：先離開的顧客優先級更高（數字越小優先級越高）
        // 這樣後面的顧客會自動避開前面的顧客，不會擠在一起
        UnityEngine.AI.NavMeshAgent agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            // 根據延遲時間設置優先級：延遲越短（越早離開）優先級越高
            int priority = 50 - Mathf.RoundToInt(delay * 10); // 延遲0秒=優先級50，0.5秒=45，1秒=40...
            agent.avoidancePriority = Mathf.Clamp(priority, 10, 99); // 限制在10-99之間
            Debug.Log($"[CustomerManager] 設置顧客避障優先級：{agent.avoidancePriority}（延遲 {delay}秒）");
        }

        // 開始正常的慌亂離開流程
        yield return StartCoroutine(RotateAndLeaveCustomer(customer));
    }

    private System.Collections.IEnumerator RotateAndLeaveCustomer(Customer customer)
    {
        // 標記顧客為慌亂狀態
        customer.SetPanicking(true);

        // 取得 NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("顧客沒有 NavMeshAgent，無法移動！");
            yield break;
        }

        // 保存原始速度
        float originalSpeed = agent.speed;

        // 1. 旋轉表示驚嚇
        agent.updateRotation = false;
        customer.transform.Rotate(0, 180, 0);
        yield return new WaitForSeconds(0.3f);
        agent.updateRotation = true;

        // 2. 如果有設置慌亂點，就讓顧客慌亂亂跑
        if (panicPoints.Count > 0)
        {
            // 設置慌亂時的移動速度（更快）
            agent.speed = panicMoveSpeed;

            // 隨機選擇要跑幾個點
            int panicStopCount = Random.Range(minPanicStops, maxPanicStops + 1);
            panicStopCount = Mathf.Min(panicStopCount, panicPoints.Count);

            Debug.Log($"顧客開始慌亂！將跑 {panicStopCount} 個點");

            // 選擇使用次數最少的慌亂點，避免客人都跑到同一個點
            List<Transform> selectedPoints = SelectLeastUsedPanicPoints(panicStopCount);

            // 標記這些點正在使用
            foreach (Transform point in selectedPoints)
            {
                IncrementPanicPointUsage(point);
            }

            // 依次移動到選擇的慌亂點
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                Transform panicPoint = selectedPoints[i];

                // 添加隨機偏移，避免多個客人擠在同一個位置
                Vector3 targetPosition = GetRandomOffsetPosition(panicPoint.position);

                agent.SetDestination(targetPosition);

                // 等待到達慌亂點
                yield return new WaitUntil(() =>
                    !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance + 0.5f
                );

                // 在這個點短暫停留
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }

            // 釋放這些點的使用計數
            foreach (Transform point in selectedPoints)
            {
                DecrementPanicPointUsage(point);
            }

            // 恢復原始速度
            agent.speed = originalSpeed;
        }
        else
        {
            // 如果沒有設置慌亂點，就等待一下
            Debug.LogWarning("未設置慌亂點，顧客將直接離開");
            yield return new WaitForSeconds(1.0f);
        }

        // 3. 最後前往離開點
        MoveCustomerToLeavePoint(customer);
    }

    // 打亂列表順序的輔助方法
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

    // ---------------------- Panic Points 管理方法 ----------------------

    // 選擇使用次數最少的慌亂點
    private List<Transform> SelectLeastUsedPanicPoints(int count)
    {
        // 初始化使用計數
        foreach (Transform point in panicPoints)
        {
            if (!panicPointUsageCount.ContainsKey(point))
            {
                panicPointUsageCount[point] = 0;
            }
        }

        // 按使用次數排序，選擇使用次數最少的點
        List<Transform> sortedPoints = new List<Transform>(panicPoints);
        sortedPoints.Sort((a, b) => panicPointUsageCount[a].CompareTo(panicPointUsageCount[b]));

        // 取前 count 個點
        List<Transform> selectedPoints = new List<Transform>();
        for (int i = 0; i < Mathf.Min(count, sortedPoints.Count); i++)
        {
            selectedPoints.Add(sortedPoints[i]);
        }

        // 打亂順序，讓客人不會按照固定順序跑
        ShuffleList(selectedPoints);

        return selectedPoints;
    }

    // 增加慌亂點的使用計數
    private void IncrementPanicPointUsage(Transform point)
    {
        if (!panicPointUsageCount.ContainsKey(point))
        {
            panicPointUsageCount[point] = 0;
        }
        panicPointUsageCount[point]++;
    }

    // 減少慌亂點的使用計數
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

    // 在目標位置周圍添加隨機偏移
    private Vector3 GetRandomOffsetPosition(Vector3 originalPosition)
    {
        // 在 XZ 平面上添加隨機偏移（保持 Y 軸不變）
        float randomX = Random.Range(-panicPointOffset, panicPointOffset);
        float randomZ = Random.Range(-panicPointOffset, panicPointOffset);

        return new Vector3(
            originalPosition.x + randomX,
            originalPosition.y,
            originalPosition.z + randomZ
        );
    }
}