using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class IngredientManager : MonoBehaviour
{

    [Header("Slot 配置")]
    public Transform slotContainer; // 放置所有 Slot 的父物件（例如一個 Panel）

    [Header("參考")]
    private List<IngredientSlot> activeSlots = new List<IngredientSlot>();

    private Image foodpic;
    public GameObject gameslotprefeb;

    private void Start()
    {
        LoadSlotsFromData();
    }

    private void OnDestroy()
    {
        SaveSlotsToData();
    }

    // 也可以在場景切換前手動調用
    private void OnApplicationQuit()
    {
        SaveSlotsToData();
    }

    /// <summary>
    /// 根據 data.inbag 生成對應的 Slot
    /// </summary>
    private void LoadSlotsFromData()
    {
        // 清空現有的 Slot
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        activeSlots.Clear();

        // 根據 data.inbag 生成 Slot
        foreach (var ingredData in data.inbag)
        {
            GameObject slotObj = Instantiate(gameslotprefeb, slotContainer);
            foodpic = slotObj.transform.Find("content").GetComponent<Image>();
            foodpic.sprite = data.GetSprite(ingredData.name);
            IngredientSlot slot = slotObj.GetComponent<IngredientSlot>();

            if (slot != null)
            {
                // 設定 Slot 的初始數據
                slot.ingredientName = ingredData.name;
                slot.count = ingredData.quantity;

                activeSlots.Add(slot);

                // Debug.Log($"生成食材槽：{ingredData.name}，數量：{ingredData.quantity}");
            }
        }
    }

    /// 將當前 Slot 的數量更新回 data.inbag
    private void SaveSlotsToData()
    {
        foreach (var slot in activeSlots)
        {
            if (slot == null) continue;

            // 在 data.inbag 中找到對應的食材
            var ingredData = data.inbag.Find(x => x.name == slot.ingredientName);

            if (ingredData != null)
            {
                ingredData.quantity = slot.count;
                // Debug.Log($"保存食材數據：{slot.ingredientName}，數量：{slot.count}");
            }
        }
    }

    public void ManualSave()
    {
        SaveSlotsToData();
    }

    public void RefreshSlots()
    {
        //SaveSlotsToData();
        LoadSlotsFromData();
    }

}