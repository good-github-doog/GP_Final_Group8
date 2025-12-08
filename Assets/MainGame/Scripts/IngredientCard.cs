using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IngredientCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private IngredientSlot sourceSlot; // 來源卡，用於退回數量
    public bool droppedInCombine = false;

    public string ingredientName;
    public string ingredinetType;

    public void Setup(IngredientSlot slot, string name)
    {
        sourceSlot = slot;
        ingredientName = name;
        ingredinetType = data.gettype(name);
        Image foodpic = transform.Find("content").GetComponent<Image>();
        foodpic.sprite = data.GetSprite(name);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        droppedInCombine = false;
    }
    public void ForceBeginDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        var fakeEvent = new PointerEventData(EventSystem.current)
        {
            position = mousePos
        };

        OnBeginDrag(fakeEvent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 如果沒有被成功放到合成區，銷毀並退還數量
        if (!droppedInCombine)
        {
            sourceSlot.ReturnCard();
            Destroy(gameObject);
        }
    }

    public void MarkAsDropped()
    {
        droppedInCombine = true;
    }
}
