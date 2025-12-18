using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;

public enum CustomerType
{
    Apple, Cow, CowL3, Kiwi, Lettuce, Lobster, Pig, PigL3, Pineapple, Tomato
}

public class Customer : MonoBehaviour
{
    private NavMeshAgent agent;
    private CustomerSpot targetSpot;
    private bool hasArrived = false;

    public GameObject burgerRecipeUI;
    public GameObject salmonRecipeUI;
    public TextMeshProUGUI therecipe;
    public GameObject killEffectPrefab;

    [Header("Timer UI")]
    public Slider waitTimerSlider;
    private Image sliderFillImage;


    [Header("Customer Type")]
    public CustomerType customerType = CustomerType.Cow;

    private int expectedMealIndex = -1;

    private Queue<Vector3> waypointQueue = new Queue<Vector3>();
    private Vector3 finalDestination;
    private bool isFollowingWaypoints = false;

    private bool isLeaving = false;
    private bool isPanicking = false;
    private CustomerSpot assignedSpot;
    private FoodArea myFoodArea;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float stuckTimeThreshold = 2f;
    private const float stuckDistanceThreshold = 0.1f;

    [Header("Leave Settings")]
    public float leaveDistanceThreshold = 3.0f;

    [Header("Waiting Settings")]
    public float minWaitTime = 15f;
    public float maxWaitTime = 30f;
    private float waitTimeLimit;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool leftDueToTimeout = false;

    public void SetFoodArea(FoodArea area)
    {
        myFoodArea = area;
    }

    public void SetExpectedMeal(int mealIndex)
    {
        expectedMealIndex = mealIndex;
    }

    public CustomerType GetCustomerType()
    {
        return customerType;
    }

    public void SetPanicking(bool panicking)
    {
        isPanicking = panicking;
    }

    public bool IsPanicking()
    {
        return isPanicking;
    }

    public bool LeftDueToTimeout()
    {
        return leftDueToTimeout;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.avoidancePriority = 50;
            agent.radius = 0.5f;
            agent.stoppingDistance = 0.3f;  // **新增這行**
            agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        // 初始化計時器 Slider 為隱藏狀態
        if (waitTimerSlider != null)
        {
            waitTimerSlider.gameObject.SetActive(false);

            // 獲取 Slider 的 Fill Image 引用
            sliderFillImage = waitTimerSlider.fillRect?.GetComponent<Image>();
        }

        // 初始化 Recipe UI 為隱藏狀態
        if (burgerRecipeUI != null)
        {
            burgerRecipeUI.SetActive(false);
        }
        if (salmonRecipeUI != null)
        {
            salmonRecipeUI.SetActive(false);
        }
    }
    private void DetectAndResolveStuck()
    {
        if (agent == null || !agent.enabled) return;

        // 只在移動中檢測（有路徑且未到達目的地）
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            // 計算移動距離
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            // 如果幾乎沒有移動
            if (distanceMoved < stuckDistanceThreshold)
            {
                stuckTimer += Time.deltaTime;

                // 如果卡住超過閾值時間
                if (stuckTimer >= stuckTimeThreshold)
                {
                    // Debug.LogWarning($"[Customer] 檢測到卡住！嘗試解決... (已卡住 {stuckTimer:F1} 秒)");

                    // 解決方案1：降低避障優先級，讓其他顧客先通過
                    int currentPriority = agent.avoidancePriority;
                    agent.avoidancePriority = 90; // 設為較低優先級
                    // Debug.Log($"[Customer] 降低避障優先級：{currentPriority} → 90，讓其他顧客先通過");

                    // 解決方案2：重新計算路徑
                    Vector3 currentDestination = agent.destination;
                    agent.ResetPath();
                    agent.SetDestination(currentDestination);
                    // Debug.Log($"[Customer] 重新計算路徑到：{currentDestination}");

                    // 重置計時器
                    stuckTimer = 0f;
                }
            }
            else
            {
                // 正在移動，重置計時器
                stuckTimer = 0f;
            }
        }
        else
        {
            // 沒有路徑或已到達，重置計時器
            stuckTimer = 0f;
        }

        // 更新上一幀位置
        lastPosition = transform.position;
    }

    public void SetDestination(CustomerSpot spot)
    {
        assignedSpot = spot;
        targetSpot = spot;
        spot.OccupySpot(this);
        agent.SetDestination(spot.transform.position);
    }

    public void SetDestinationWithWaypoints(CustomerSpot spot, List<Vector3> waypoints)
    {
        assignedSpot = spot;
        targetSpot = spot;
        spot.OccupySpot(this);
        finalDestination = spot.transform.position;

        waypointQueue.Clear();
        foreach (var waypoint in waypoints)
        {
            waypointQueue.Enqueue(waypoint);
        }

        if (waypointQueue.Count > 0)
        {
            isFollowingWaypoints = true;
            Vector3 firstWaypoint = waypointQueue.Dequeue();
            agent.SetDestination(firstWaypoint);
        }
        else
        {
            agent.SetDestination(finalDestination);
        }
    }

    void Update()
    {
        // Check if panic mode is active and customer hasn't arrived at seat yet
        // If customer is outside (not yet seated), remove immediately
        if (data.isPanicMode == 1 && !hasArrived && !isLeaving)
        {
            Debug.Log("[Customer] Panic mode detected! Customer outside restaurant, removing immediately.");

            // Release the assigned spot
            if (assignedSpot != null)
            {
                assignedSpot.ReleaseSpot();
                assignedSpot = null;
                CustomerManager.Instance.NotifyCustomerLeft();
            }

            // Remove immediately (no need to walk to leave point)
            CustomerManager.Instance.RemoveCustomer(this);
            return;
        }

        DetectAndResolveStuck();

        // 檢查等待計時器
        if (isWaiting && !isLeaving)
        {
            waitTimer += Time.deltaTime;

            // 更新 Slider UI (從 1 到 0，表示時間遞減)
            if (waitTimerSlider != null)
            {
                float timeRemaining = 1f - (waitTimer / waitTimeLimit);
                waitTimerSlider.value = timeRemaining;

                // 更新 Slider 顏色：綠色 → 黃色 → 紅色 (平滑漸變)
                if (sliderFillImage != null)
                {
                    Color sliderColor;

                    if (timeRemaining > 0.5f) // 100%-50%: 綠色 → 黃色
                    {
                        // t = 0 (50%) -> 1 (100%)
                        float t = (timeRemaining - 0.5f) / 0.5f;
                        sliderColor = Color.Lerp(Color.yellow, Color.green, t);
                    }
                    else // 50%-0%: 黃色 → 紅色
                    {
                        // t = 0 (0%) -> 1 (50%)
                        float t = timeRemaining / 0.5f;
                        sliderColor = Color.Lerp(Color.red, Color.yellow, t);
                    }

                    sliderFillImage.color = sliderColor;
                }
            }

            if (waitTimer >= waitTimeLimit)
            {
                OnWaitTimeout();
            }
        }

        if (isFollowingWaypoints)
        {
            // 放寬判斷條件 - 添加 agent 有效性檢查
            bool shouldProceed = agent != null && agent.enabled && agent.isOnNavMesh &&
                               !agent.pathPending &&
                               agent.remainingDistance <= agent.stoppingDistance + 0.5f;

            if (shouldProceed)
            {
                if (waypointQueue.Count > 0)
                {
                    Vector3 nextWaypoint = waypointQueue.Dequeue();
                    agent.SetDestination(nextWaypoint);
                }
                else
                {
                    agent.SetDestination(finalDestination);
                    isFollowingWaypoints = false;
                }
            }
        }

        if (!isLeaving && !hasArrived && targetSpot != null && !isFollowingWaypoints)
        {
            // 簡化到達判斷條件 - 添加 agent 有效性檢查
            if (agent != null && agent.enabled && agent.isOnNavMesh &&
                !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
            {
                OnReachedSpot();
            }
        }

        if (isLeaving && !isFollowingWaypoints)
        {
            // 簡化離開判斷條件 - 添加 agent 有效性檢查
            if (agent != null && agent.enabled && agent.isOnNavMesh &&
                !agent.pathPending && agent.remainingDistance <= leaveDistanceThreshold)
            {
                CustomerManager.Instance.RemoveCustomer(this);
            }
        }
    }

    private void OnReachedSpot()
    {
        hasArrived = true;

        // **關鍵：停止 NavMeshAgent**
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        Debug.Log($"[Customer] 已到達座位: {targetSpot.name}");
        targetSpot.OnCustomerArrived();
        ShowCorrectRecipe();

        // 隨機生成等待時間（15-30秒）
        waitTimeLimit = Random.Range(minWaitTime, maxWaitTime);
        Debug.Log($"[Customer] 等待時間設定為: {waitTimeLimit:F1} 秒");

        // 開始等待計時
        isWaiting = true;
        waitTimer = 0f;

        // 顯示並初始化計時器 Slider
        if (waitTimerSlider != null)
        {
            waitTimerSlider.gameObject.SetActive(true);
            waitTimerSlider.value = 1f;

            // 初始化顏色為綠色
            if (sliderFillImage != null)
            {
                sliderFillImage.color = Color.green;
            }
        }
    }

    private void ShowCorrectRecipe()
    {
        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(false);
        if (salmonRecipeUI != null) salmonRecipeUI.SetActive(false);

        MealTable.OrderText.TryGetValue(expectedMealIndex, out string ordering);
        therecipe.text = ordering;
        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(true);
    }

    public void OnFoodServed(bool isCorrect)
    {
        // 停止等待計時器
        isWaiting = false;

        // 隱藏計時器 Slider
        if (waitTimerSlider != null)
        {
            waitTimerSlider.gameObject.SetActive(false);
        }

        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(false);
        if (salmonRecipeUI != null) salmonRecipeUI.SetActive(false);

        if (isCorrect)
        {
            int price = MealTable.GetPrice(expectedMealIndex);
            data.money += price;
            data.incomeServe += price;   // 🔸記錄服務收入
            print("[Customer] 顧客收到正確餐點，獲得收入: " + price);
        }
        else
        {
            data.money -= 100;
            data.penaltyWrong += 100;   // 🔸記錄送錯餐罰款
            if (data.money < 0) data.money = 0;
            print("[Customer] 顧客收到錯誤餐點，扣除罰款: 100");
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeOnce(0.2f, 0.2f);
                Debug.Log("Shake Shake Shake!");
            }
        }

        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null && gm.moneyText != null)
        {
            gm.moneyText.text = "$ " + data.money;
        }

        StartCoroutine(LeaveAfterDelay());
    }

    private void OnWaitTimeout()
    {
        Debug.Log($"[Customer] 等待超時，顧客離開！等待了 {waitTimer:F1} 秒");

        // 標記為超時離開
        leftDueToTimeout = true;
        isWaiting = false;

        // 隱藏計時器 Slider
        if (waitTimerSlider != null)
        {
            waitTimerSlider.gameObject.SetActive(false);
        }

        // 隱藏 UI
        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(false);
        if (salmonRecipeUI != null) salmonRecipeUI.SetActive(false);

        // 清理 food area
        if (myFoodArea != null)
        {
            myFoodArea.ClearFoodOnTable();
            myFoodArea.ClearCustomer();
            myFoodArea = null;
        }

        // 釋放座位
        if (assignedSpot != null)
        {
            assignedSpot.ReleaseSpot();
            assignedSpot = null;
            CustomerManager.Instance.NotifyCustomerLeft();
        }

        // 離開餐廳
        CustomerManager.Instance.MoveCustomerToLeavePoint(this);
    }

    private IEnumerator LeaveAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        if (myFoodArea != null)
        {
            myFoodArea.ClearFoodOnTable();
            myFoodArea.ClearCustomer();
            myFoodArea = null;
        }

        if (assignedSpot != null)
        {
            assignedSpot.ReleaseSpot();
            assignedSpot = null;
            CustomerManager.Instance.NotifyCustomerLeft();
        }

        CustomerManager.Instance.MoveCustomerToLeavePoint(this);
    }

    public void SetDestinationToLeavePoint(Vector3 leavePos)
    {
        agent.isStopped = false;
        agent.SetDestination(leavePos);
        isLeaving = true;
    }

    public void SetDestinationToLeavePointWithWaypoints(Vector3 leavePos, List<Vector3> waypoints)
    {
        agent.isStopped = false;
        isLeaving = true;
        finalDestination = leavePos;

        waypointQueue.Clear();
        foreach (var waypoint in waypoints)
        {
            waypointQueue.Enqueue(waypoint);
        }

        if (waypointQueue.Count > 0)
        {
            isFollowingWaypoints = true;
            Vector3 firstWaypoint = waypointQueue.Dequeue();
            agent.SetDestination(firstWaypoint);
        }
        else
        {
            agent.SetDestination(finalDestination);
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();

        if (assignedSpot != null)
        {
            assignedSpot.ReleaseSpot();
            assignedSpot = null;
        }
    }

    public void PlayKillEffect()
    {
        // 生成破碎或爆炸特效
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;   // 稍微往上
        Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);

        GameObject fx = Instantiate(killEffectPrefab, spawnPos, spawnRot);

        // 🔥 強制 Particle System 重播
        fx.GetComponent<ParticleSystem>().Play();

        print("[Customer] 播放顧客被消滅特效");

        Destroy(gameObject);
    }
}
