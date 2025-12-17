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

    // Stage 1 的 "Any" 類型餐點 (Any Burger, Any Sandwich)
    private int[] stage1Any = {MealTable.ANY_BURGER, MealTable.ANY_SANDWICH};

    // Stage 2 的餐點（包含 Any Pizza）
    private int[] stage2Meals = {18, 19, 20, MealTable.ANY_PIZZA};

    // Stage 3 的餐點（包含所有 Any 類型）
    private int[] stage3Meals = {23, 24, MealTable.ANY_BURGER, MealTable.ANY_SANDWICH, MealTable.ANY_PIZZA};

    private void GenerateMealRequest()
    {
        float hellweight = 0.05f;
        float r = Random.value;
        float needkillweight = 0.01f;
        float rr = Random.value;
        float anyweight = 0.15f; // "Any" 類型出現的機率
        float rrr = Random.value;

        if (data.nowstage == 1)
        {
            if (r < hellweight) wantedMeal = Random.Range(16,18);
            else if (rr < needkillweight) wantedMeal = stneedkill[Random.Range(0, stneedkill.Length)];
            else if (rrr < anyweight) wantedMeal = stage1Any[Random.Range(0, stage1Any.Length)]; // 15% 機率出現 Any 類型
            else if (data.clearstage == 1) wantedMeal = stmenu[Random.Range(0, stmenu.Length)];
            else wantedMeal = stall[Random.Range(0, stall.Length)];
        }
        else if (data.nowstage == 2)
        {
            if (r < hellweight) wantedMeal = 21;
            else wantedMeal = stage2Meals[Random.Range(0, stage2Meals.Length)]; // 包含 Any Pizza
        }
        else if (data.nowstage == 3)
        {
            if (r < hellweight) wantedMeal = 25;
            else if (rr < needkillweight) wantedMeal = 22;
            else wantedMeal = stage3Meals[Random.Range(0, stage3Meals.Length)]; // 包含所有 Any 類型
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
