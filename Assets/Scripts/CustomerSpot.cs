using UnityEngine;

public class CustomerSpot : MonoBehaviour
{
    [Header("Spot Status")]
    private bool isOccupied = false;
    public Customer currentCustomer;

    public bool IsOccupied => isOccupied;
    public Customer CurrentCustomer => currentCustomer;

    // ----------------------------------------------------------------------

    public void OccupySpot(Customer customer)
    {
        isOccupied = true;
        currentCustomer = customer;
        // ❌ 不在這裡生成訂單
    }

    /// 由外部呼叫：當客人「走到這個座位」的那一刻
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

    // ----------------------------------------------------------------------

    /// 產生客人的訂單（0~maxMealIndex）
    private void GenerateMealRequest()
    {
        float weightBurger = 0.8f;   // 80%
        float weightSandwich = 0.2f; // 20%

        float r = Random.value;   // 0 ~ 1 的隨機數

        if (r < weightBurger)
            wantedMeal = 0;              // 漢堡
        else
            wantedMeal = 1;              // 鮭魚香菇三明治

        Debug.Log($"[CustomerSpot] 客人需求餐點編號：{wantedMeal}");

        // 將需求傳給 FoodArea
        if (myFoodArea != null)
        {
            myFoodArea.expectedMealIndex = wantedMeal;
            Debug.Log($"[CustomerSpot] 已將餐點需求 {wantedMeal} 設定給 FoodArea");
        }
        else
        {
            Debug.LogWarning("[CustomerSpot] myFoodArea 未設定，無法接收上菜判定");
        }
    }

    // ----------------------------------------------------------------------

    // Visual feedback in editor
    void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}