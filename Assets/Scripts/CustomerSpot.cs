using UnityEngine;

public class CustomerSpot : MonoBehaviour
{
    [Header("Spot Status")]
    private bool isOccupied = false;
    private Customer currentCustomer;

    [Header("Meal Request")]
    public int maxMealIndex = 1;
    public int wantedMeal = -1;

    [Header("Food Area Reference")]
    public FoodArea myFoodArea;
    public bool IsOccupied => isOccupied;
    public Customer CurrentCustomer => currentCustomer;

    public void OccupySpot(Customer customer)
    {
        isOccupied = true;
        currentCustomer = customer;
    }

    public void OnCustomerArrived()
    {
        GenerateMealRequest();
        myFoodArea.SetCustomer(currentCustomer, wantedMeal);
    }

    public void ReleaseSpot()
    {
        isOccupied = false;

        if (myFoodArea != null)
            myFoodArea.ClearCustomer();

        currentCustomer = null;
    }

    private int[] stmenu = {0,1,2,6,7,8,12};
    private int[] stall = {0,1,2,3,5,6,7,8,9,11,12,14,15};
    private int[] stneedkill = {4,10,13};
    private void GenerateMealRequest()
    {
        float hellweight = 0.05f;
        float r = Random.value;
        float needkillweight = 0.01f;
        float rr = Random.value;

        if (data.nowstage == 1)
        {
            if (r < hellweight) wantedMeal = Random.Range(16,18);
            else if (rr < needkillweight) wantedMeal = stneedkill[Random.Range(0, stneedkill.Length)];
            else if (data.clearstage == 1) wantedMeal = stmenu[Random.Range(0, stmenu.Length)];
            else wantedMeal = stall[Random.Range(0, stall.Length)];
        }
        else if (data.nowstage == 2)
        {
            if (r < hellweight) wantedMeal = 21;
            else wantedMeal = Random.Range(18,21);
        }
        else if (data.nowstage == 3)
        {
            if (r < hellweight) wantedMeal = 25;
            else if (rr < needkillweight) wantedMeal = 22;
            else wantedMeal = Random.Range(23,25);
        }

        if (myFoodArea != null)
        {
            myFoodArea.expectedMealIndex = wantedMeal;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
