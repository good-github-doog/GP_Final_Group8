 using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FoodArea : MonoBehaviour, IDropHandler
{
    public Vector3 desiredPosition = new Vector3(-2.7f, 2f, -1.8f);

    [System.Serializable]
    public class FoodPrefab
    {
        public string name;
        public GameObject prefab;
    }
    public List<FoodPrefab> prefabList;
    private Dictionary<string, GameObject> prefabDict;
    private GameObject newFood = null;



    void Start()
    {
        prefabDict = new Dictionary<string, GameObject>();
        foreach (var item in prefabList)
            prefabDict[item.name] = item.prefab;
    }
    public int expectedMealIndex = -1;   // CustomerSpot 會設定這個
    private Customer currentCustomer;
    public void SetCustomer(Customer customer, int expectedMealIndex)
    {
        currentCustomer = customer;
        this.expectedMealIndex = expectedMealIndex;

        if (customer != null)
        {
            customer.SetFoodArea(this);  // ⭐非常重要：綁定「顧客 ↔ 食物區」
            customer.SetExpectedMeal(expectedMealIndex);   // ⭐ 傳給 Customer
        }
    }
    public void ClearCustomer()
    {
        currentCustomer = null;
        expectedMealIndex = -1;
    }
    public void OnDrop(PointerEventData eventData)
    {
        FoodCard card = eventData.pointerDrag.GetComponent<FoodCard>();
        if (card == null) return;
/*
        if (!prefabDict.TryGetValue(card.foodName, out GameObject prefabToUse))
        {
            Debug.LogError($"找不到對應的食物 Prefab：{card.foodName}");
            return;
        }
*/
        GameObject prefabToUse = null;
        prefabToUse = MealTable.GetFood("foodprefebs/" + card.foodName);
        if (prefabToUse == null){
            Debug.LogError($"找不到對應的食物 Prefab：{card.foodName}");
            return;
        }

        Quaternion desiredRotation = Quaternion.identity;
        newFood = Instantiate(prefabToUse, desiredPosition, desiredRotation);
        Debug.Log($"[FoodArea] 放入上菜區：{card.foodName}");




        // 拿到食物的名稱後刪除卡片
        string foodName = card.foodName;
        Destroy(card.gameObject);

        if (!MealTable.MealMap.TryGetValue(foodName, out int foodIndex))
        {
            Debug.Log($"[FoodArea] 食物名稱 {foodName} 不在 MealTable 中！");
            return;
        }

        Debug.Log($"[FoodArea] 食物 '{foodName}' → 編號 {foodIndex}");

        // 使用 MealTable 的匹配方法（支援 Any Burger/Sandwich/Pizza）
        bool isCorrect = MealTable.IsMealMatch(foodName, expectedMealIndex);

        // 顯示匹配結果
        if (expectedMealIndex == MealTable.ANY_BURGER)
        {
            Debug.Log($"[FoodArea] 顧客點了 Any Burger，提供了 {foodName}");
        }
        else if (expectedMealIndex == MealTable.ANY_SANDWICH)
        {
            Debug.Log($"[FoodArea] 顧客點了 Any Sandwich，提供了 {foodName}");
        }
        else if (expectedMealIndex == MealTable.ANY_PIZZA)
        {
            Debug.Log($"[FoodArea] 顧客點了 Any Pizza，提供了 {foodName}");
        }

        Debug.Log(isCorrect ? "✔ Food is correctly served!" : "✘ Food is incorrectly served!");

        if (currentCustomer != null)
        {
            currentCustomer.OnFoodServed(isCorrect);
        }
        else
        {
            Debug.LogWarning("[FoodArea] 沒有正在服務的客人！");
        }
    }

    public void ClearFoodOnTable()
    {
        Debug.Log("[FoodArea] ClearFoodOnTable 被呼叫");

        if (newFood != null)
        {
            Debug.Log($"[FoodArea] 銷毀 {newFood.name}");
            Destroy(newFood);
            newFood = null;
        }
        else
        {
            Debug.Log("[FoodArea] 桌上沒有食物可清除");
        }
    }


}