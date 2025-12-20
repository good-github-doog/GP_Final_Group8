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
    public int expectedMealIndex = -1;
    private Customer currentCustomer;
    public void SetCustomer(Customer customer, int expectedMealIndex)
    {
        currentCustomer = customer;
        this.expectedMealIndex = expectedMealIndex;

        if (customer != null)
        {
            customer.SetFoodArea(this);
        }
    }
    public void ClearCustomer()
    {
        currentCustomer = null;
        expectedMealIndex = -1;
    }

    public string GetExpectedMealText()
    {
        MealTable.OrderText.TryGetValue(expectedMealIndex, out string ordering);
        return ordering;
    }
    public void OnDrop(PointerEventData eventData)
    {
        FoodCard card = eventData.pointerDrag.GetComponent<FoodCard>();
        if (card == null) return;

        GameObject prefabToUse = null;
        prefabToUse = MealTable.GetFood("foodprefebs/" + card.foodName);
        if (prefabToUse == null) return;

        Quaternion desiredRotation = Quaternion.identity;
        newFood = Instantiate(prefabToUse, desiredPosition, desiredRotation);

        string foodName = card.foodName;
        Destroy(card.gameObject);

        if (!MealTable.MealMap.TryGetValue(foodName, out int foodIndex))
        {
            foodIndex = -10;
        }

        // 直接比較餐點編號
        bool isCorrect = foodIndex == expectedMealIndex;

        // 處理價格邏輯
        if (isCorrect)
        {
            int price = MealTable.GetPrice(expectedMealIndex);
            data.money += price;
            data.incomeServe += price;
        }
        else
        {
            data.money -= 100;
            data.penaltyWrong += 100;
            if (data.money < 0) data.money = 0;

            // 觸發相機震動
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeOnce(0.2f, 0.2f);
            }
        }

        // 更新金錢顯示
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null && gm.moneyText != null)
        {
            gm.moneyText.text = "$ " + data.money;
        }

        if (currentCustomer != null)
        {
            currentCustomer.OnFoodServed(isCorrect);
        }
    }

    public void ClearFoodOnTable()
    {
        if (newFood != null)
        {
            Destroy(newFood);
            newFood = null;
        }
    }
}