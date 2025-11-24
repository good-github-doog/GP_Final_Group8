using UnityEngine;

public class CustomerSpot : MonoBehaviour
{
    [Header("Spot Status")]
    private bool isOccupied = false;
    private Customer currentCustomer;

    public bool IsOccupied => isOccupied;
    public Customer CurrentCustomer => currentCustomer;

    [Header("Meal Request")]
    public int maxMealIndex = 1;
    public int wantedMeal = -1;

    [Header("Food Area Reference")]
    public FoodArea myFoodArea;

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

    private void GenerateMealRequest()
    {
        float weightBurger = 0.8f;
        float r = Random.value;

        if (r < weightBurger)
            wantedMeal = 0;
        else
            wantedMeal = 1;

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
