using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

        // 讓本關商店販售的食材排前面，其餘維持原順序
        var toolOrder = new Dictionary<string, int> { { "oven", 0 }, { "mixer", 1 }, { "steak", 2 }, { "doublesauce", 3 } };
        data.goodsmap.TryGetValue(data.nowstage, out var stageGoods);
        Dictionary<string, int> shopOrder = null;
        if (stageGoods != null)
        {
            shopOrder = new Dictionary<string, int>();
            for (int i = 0; i < stageGoods.Count; i++)
            {
                shopOrder[stageGoods[i]] = i;
            }
        }

        var orderedBag = data.inbag
            .Select((ing, idx) => new { ing, idx })
            .OrderBy(x =>
            {
                if (toolOrder.TryGetValue(x.ing.name, out int tOrder)) return 0;
                if (shopOrder != null && shopOrder.ContainsKey(x.ing.name)) return 1;
                return 2;
            })
            .ThenBy(x =>
            {
                if (toolOrder.TryGetValue(x.ing.name, out int tOrder)) return tOrder;
                if (shopOrder != null && shopOrder.TryGetValue(x.ing.name, out int order)) return order;
                return x.idx; // 保留非商店項目的原始順序
            })
            .Select(x => x.ing);

        // 根據排序後的資料生成 Slot
        foreach (var ingredData in orderedBag)
        {
            if (ingredData.quantity <= 0) continue; // 跳過數量為 0 的食材

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
    public void SaveSlotsToData()
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
