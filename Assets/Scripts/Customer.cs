using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    private NavMeshAgent agent;
    private CustomerSpot targetSpot;
    private bool hasArrived = false;

    public GameObject burgerRecipeUI;
    public GameObject salmonRecipeUI;

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
    public float waitTimeLimit = 20f;
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
            agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
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
        DetectAndResolveStuck();

        // 檢查等待計時器
        if (isWaiting && !isLeaving)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeLimit)
            {
                // 等待超時，顧客離開
                OnWaitTimeout();
            }
        }

        if (isFollowingWaypoints)
        {
            bool shouldProceed = !agent.pathPending &&
                               agent.remainingDistance <= agent.stoppingDistance + 0.5f &&
                               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);

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
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance &&
                (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
            {
                OnReachedSpot();
            }
        }

        if (isLeaving && !isFollowingWaypoints)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= leaveDistanceThreshold &&
                (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
            {
                CustomerManager.Instance.RemoveCustomer(this);
            }
        }
    }

    private void OnReachedSpot()
    {
        hasArrived = true;
        Debug.Log($"[Customer] 已到達座位: {targetSpot.name}");
        targetSpot.OnCustomerArrived();
        ShowCorrectRecipe();

        // 開始等待計時
        isWaiting = true;
        waitTimer = 0f;
    }

    private void ShowCorrectRecipe()
    {
        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(false);
        if (salmonRecipeUI != null) salmonRecipeUI.SetActive(false);

        if (expectedMealIndex == 0)
        {
            if (burgerRecipeUI != null) burgerRecipeUI.SetActive(true);
        }
        else if (expectedMealIndex == 1)
        {
            if (salmonRecipeUI != null) salmonRecipeUI.SetActive(true);
        }
    }

    public void OnFoodServed(bool isCorrect)
    {
        // 停止等待計時器
        isWaiting = false;

        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(false);
        if (salmonRecipeUI != null) salmonRecipeUI.SetActive(false);

        if (isCorrect)
        {
            data.money += 200;
        }
        else
        {
            data.money -= 100;
            if (data.money < 0) data.money = 0;
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
        agent.SetDestination(leavePos);
        isLeaving = true;
    }

    public void SetDestinationToLeavePointWithWaypoints(Vector3 leavePos, List<Vector3> waypoints)
    {
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
        if (assignedSpot != null)
        {
            assignedSpot.ReleaseSpot();
            assignedSpot = null;
        }
    }
}
