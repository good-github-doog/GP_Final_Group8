using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CombineArea : MonoBehaviour, IDropHandler
{
    public List<string> ingredientsInArea = new List<string>();
    public List<string> ingredstypeInArea = new List<string>();

    public void OnDrop(PointerEventData eventData)
    {
        IngredientCard card = eventData.pointerDrag.GetComponent<IngredientCard>();
        if (card != null)
        {
            if (card.droppedInCombine) return;
            card.MarkAsDropped();
            ingredientsInArea.Add(card.ingredientName);
            ingredstypeInArea.Add(card.ingredinetType);
            card.transform.SetParent(transform); // 把卡放進合成區
            Debug.Log($"放入合成區：{card.ingredientName}");
        }
    }

    public void ClearArea()
    {
        //Debug.Log($"[ClearArea] 開始清除，當前子物件數量: {transform.childCount}");

        ingredientsInArea.Clear();
        ingredstypeInArea.Clear();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        //Debug.Log("合成區已清空");
    }
}
