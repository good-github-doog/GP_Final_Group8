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

    [Header("Leave Settings")]
    public float leaveDistanceThreshold = 3.0f;

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
        if (isFollowingWaypoints)
        {
            bool shouldProceed = !agent.pathPending &&
                               agent.remainingDistance <= agent.stoppingDistance + 0.5f &&
                               (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f);

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
                (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f))
            {
                OnReachedSpot();
            }
        }

        if (isLeaving && !isFollowingWaypoints)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= leaveDistanceThreshold &&
                (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f))
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
