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

    // 路徑點系統
    private Queue<Vector3> waypointQueue = new Queue<Vector3>();
    private Vector3 finalDestination;
    private bool isFollowingWaypoints = false;

    private bool isLeaving = false;
    private bool isPanicking = false; // 是否正在慌亂離開
    private CustomerSpot assignedSpot;  // ⭐ 修正：加入編號的 spot reference
    private FoodArea myFoodArea;

    // 卡住檢測系統
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float stuckTimeThreshold = 2f; // 2秒沒移動就視為卡住
    private const float stuckDistanceThreshold = 0.1f; // 移動距離小於0.1就算沒動

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



    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // 設置避障參數，確保顧客會互相避開
        if (agent != null)
        {
            agent.avoidancePriority = 50; // 預設優先級
            agent.radius = 0.5f; // 避障半徑
            agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }
    }

    // -------------------------------
    // 卡住檢測和解決
    // -------------------------------
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
                    Debug.LogWarning($"[Customer] 檢測到卡住！嘗試解決... (已卡住 {stuckTimer:F1} 秒)");

                    // 解決方案1：降低避障優先級，讓其他顧客先通過
                    int currentPriority = agent.avoidancePriority;
                    agent.avoidancePriority = 90; // 設為較低優先級
                    Debug.Log($"[Customer] 降低避障優先級：{currentPriority} → 90，讓其他顧客先通過");

                    // 解決方案2：重新計算路徑
                    Vector3 currentDestination = agent.destination;
                    agent.ResetPath();
                    agent.SetDestination(currentDestination);
                    Debug.Log($"[Customer] 重新計算路徑到：{currentDestination}");

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

    // -------------------------------
    //   進場：走到指定 CustomerSpot
    // -------------------------------
    public void SetDestination(CustomerSpot spot)
    {
        assignedSpot = spot;
        targetSpot = spot;

        // 立即佔用座位，避免其他顧客被分配到同一個座位
        spot.OccupySpot(this);

        agent.SetDestination(spot.transform.position);
    }

    // 設定帶路徑點的目的地
    public void SetDestinationWithWaypoints(CustomerSpot spot, List<Vector3> waypoints)
    {
        assignedSpot = spot;
        targetSpot = spot;

        // 立即佔用座位，避免其他顧客被分配到同一個座位
        spot.OccupySpot(this);

        // 設定最終目的地
        finalDestination = spot.transform.position;

        // 清空並填充路徑點隊列
        waypointQueue.Clear();
        foreach (var waypoint in waypoints)
        {
            waypointQueue.Enqueue(waypoint);
        }

        // 如果有路徑點，開始跟隨路徑點
        if (waypointQueue.Count > 0)
        {
            isFollowingWaypoints = true;
            Vector3 firstWaypoint = waypointQueue.Dequeue();
            agent.SetDestination(firstWaypoint);
            Debug.Log($"[Customer] 開始跟隨路徑點，前往第一個路徑點");
        }
        else
        {
            // 沒有路徑點，直接前往目的地
            agent.SetDestination(finalDestination);
        }
    }

    void Update()
    {
        // =============================
        // 卡住檢測和解決系統
        // =============================
        DetectAndResolveStuck();

        // =============================
        // 跟隨路徑點（進場和離場都可以使用）
        // =============================
        if (isFollowingWaypoints)
        {
            // 進場和離場都使用嚴格條件：必須完全停下來
            bool shouldProceed = !agent.pathPending &&
                               agent.remainingDistance <= agent.stoppingDistance + 0.5f &&
                               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);

            if (shouldProceed)
            {
                // 到達當前路徑點
                if (waypointQueue.Count > 0)
                {
                    // 前往下一個路徑點
                    Vector3 nextWaypoint = waypointQueue.Dequeue();
                    agent.SetDestination(nextWaypoint);
                    Debug.Log($"[Customer] 到達路徑點，前往下一個路徑點，剩餘 {waypointQueue.Count} 個");
                }
                else
                {
                    // 所有路徑點都走完了，前往最終目的地
                    agent.SetDestination(finalDestination);
                    isFollowingWaypoints = false;
                    Debug.Log($"[Customer] 所有路徑點完成，前往最終目的地");
                }
            }
        }

        // =============================
        // 抵達座位 (尚未抵達時才檢查)
        // =============================
        if (!isLeaving && !hasArrived && targetSpot != null && !isFollowingWaypoints)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance &&
                (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
            {
                OnReachedSpot();
            }
        }

        // =============================
        // 離場：抵達 leave point
        // =============================
        if (isLeaving && !isFollowingWaypoints)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= 3.0f &&
                (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
            {
                Debug.Log("[Customer] 到達離開點 → 客人消失");
                CustomerManager.Instance.RemoveCustomer(this);
            }
        }
    }

    // 客人坐下時會被呼叫
    private void OnReachedSpot()
    {
        hasArrived = true;
        Debug.Log("Customer reached spot: " + targetSpot.name);

        // 座位已在 SetDestination 時佔用，這裡只需通知抵達
        targetSpot.OnCustomerArrived();

        ShowCorrectRecipe();
    }

    private void ShowCorrectRecipe()
    {
        // 關閉全部（安全防呆）
        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(false);
        if (salmonRecipeUI != null) salmonRecipeUI.SetActive(false);

        // 根據餐點顯示
        if (expectedMealIndex == 0)
        {
            // 顧客想吃漢堡
            if (burgerRecipeUI != null) burgerRecipeUI.SetActive(true);
        }
        else if (expectedMealIndex == 1)
        {
            // 顧客想吃鮭魚香菇三明治
            if (salmonRecipeUI != null) salmonRecipeUI.SetActive(true);
        }

        Debug.Log($"[Customer] 顯示餐點食譜 → Index: {expectedMealIndex}");
    }


    // -------------------------------
    //          上菜判斷
    // -------------------------------
    public void OnFoodServed(bool isCorrect)
    {
        if (burgerRecipeUI != null) burgerRecipeUI.SetActive(false);
        if (salmonRecipeUI != null) salmonRecipeUI.SetActive(false);


        if (isCorrect)
        {
            Debug.Log("[Customer] 食物正確 → +200 元");
            data.money += 200;
            Debug.Log("[Customer] 食物正確 → 開始吃...");
        }
        else
        {
            Debug.Log("[Customer] 食物錯誤 → -100 元");
            data.money -= 100;
            if (data.money < 0) data.money = 0;
            Debug.Log("[Customer] 食物錯誤 → 客人還是會離開");
        }

        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null && gm.moneyText != null)
        {
            gm.moneyText.text = "$ " + data.money;
        }


        // ⭐ 食完後等 5 秒離開
        StartCoroutine(LeaveAfterDelay());
    }

    // -------------------------------
    //    吃完飯 → 等 5 秒 → 離場
    // -------------------------------
    private IEnumerator LeaveAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        Debug.Log("[Customer] 用餐完畢 → 客人準備離開");

        // ⭐ 食完後 → 清除桌上的食物
        if (myFoodArea != null)
        {
            myFoodArea.ClearFoodOnTable();
            myFoodArea.ClearCustomer();   // 讓 FoodArea 空出來
            myFoodArea = null;
        }


        // ⭐ 先釋放座位
        if (assignedSpot != null)
        {
            assignedSpot.ReleaseSpot();
            assignedSpot = null;

            // ⭐ 告訴 CustomerManager 座位空了，可以 spawn 新客人
            CustomerManager.Instance.NotifyCustomerLeft();
        }

        // ⭐ 前往離開點
        CustomerManager.Instance.MoveCustomerToLeavePoint(this);
    }

    // -------------------------------
    //         設定離開路徑
    // -------------------------------
    public void SetDestinationToLeavePoint(Vector3 leavePos)
    {
        agent.SetDestination(leavePos);
        isLeaving = true;
    }

    // 設定帶路徑點的離開路徑
    public void SetDestinationToLeavePointWithWaypoints(Vector3 leavePos, List<Vector3> waypoints)
    {
        isLeaving = true;
        finalDestination = leavePos;

        // 清空並填充路徑點隊列
        waypointQueue.Clear();
        foreach (var waypoint in waypoints)
        {
            waypointQueue.Enqueue(waypoint);
        }

        // 如果有路徑點，開始跟隨路徑點
        if (waypointQueue.Count > 0)
        {
            isFollowingWaypoints = true;
            Vector3 firstWaypoint = waypointQueue.Dequeue();
            agent.SetDestination(firstWaypoint);
            Debug.Log($"[Customer] 離開時將先經過路徑點");
        }
        else
        {
            // 沒有路徑點，直接前往離開點
            agent.SetDestination(finalDestination);
        }
    }


    // -------------------------------
    // 保險：物件銷毀時釋放座位
    // -------------------------------
    void OnDestroy()
    {
        if (assignedSpot != null)
        {
            assignedSpot.ReleaseSpot();
            assignedSpot = null;
        }
    }
}