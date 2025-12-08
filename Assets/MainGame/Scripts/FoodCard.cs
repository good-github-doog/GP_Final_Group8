using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string foodName;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private RectTransform rectTransform;

    public void setup(string name)
    {
        Image foodpic = transform.Find("content").GetComponent<Image>();
        foodpic.sprite = data.GetSprite("donefoods/" + name);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        //transform.SetParent(transform.root);

        transform.SetParent(originalParent.parent);  // 提升層級避免被 UI 蓋住
        canvasGroup.blocksRaycasts = false; // 避免卡牌阻擋 Drop 區
    }

    public void OnDrag(PointerEventData eventData)
    {
        //rectTransform.anchoredPosition += eventData.delta / transform.root.localScale.x;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
        rectTransform,
        eventData.position,
        eventData.pressEventCamera,
        out Vector3 worldPos
        );

        rectTransform.position = worldPos;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == transform.root)
        
        canvasGroup.blocksRaycasts = true;
    }
}
