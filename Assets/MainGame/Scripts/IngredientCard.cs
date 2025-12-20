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
    private bool autoDragging = false;
    private ScrollRect parentScrollRect;

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
        if (parentScrollRect != null) parentScrollRect.enabled = false;
        autoDragging = false; // 若玩家後續手動拖曳，關閉自動拖曳狀態
        // 如果原本在合成區，先從清單中移除
        if (transform.parent != null && transform.parent.TryGetComponent(out CombineArea combineArea) && droppedInCombine)
        {
            combineArea.RemoveIngredient(ingredientName, ingredinetType);
        }
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
        if (parentScrollRect != null) parentScrollRect.enabled = true;

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

    // 由 Slot 在生成時呼叫，讓卡片立即跟隨滑鼠
    public void StartAutoDrag()
    {
        autoDragging = true;
        droppedInCombine = false;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
        if (parentScrollRect != null) parentScrollRect.enabled = false;
    }

    private void Update()
    {
        if (!autoDragging) return;

        rectTransform.position = Mouse.current.position.ReadValue();

        // 釋放左鍵時嘗試觸發 Drop
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            autoDragging = false;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue(),
                button = PointerEventData.InputButton.Left,
                pointerDrag = gameObject,
                pointerPress = gameObject,
                rawPointerPress = gameObject
            };

            // 射線檢測目前滑鼠下的 UI，手動觸發 Drop
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var result in results)
            {
                ExecuteEvents.Execute(result.gameObject, eventData, ExecuteEvents.dropHandler);
            }

            OnEndDrag(eventData);
        }
    }

    public void SetParentScrollRect(ScrollRect scrollRect)
    {
        parentScrollRect = scrollRect;
        if (parentScrollRect != null) parentScrollRect.enabled = false;
    }
}
