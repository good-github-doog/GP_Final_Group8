using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class IngredientSlot : MonoBehaviour, IPointerDownHandler
{
    public string ingredientName;
    public int count = 3; // 初始數量
    public GameObject ingredientCardPrefab; // 生成可拖曳卡牌的 Prefab
    public Transform cardSpawnParent; // 放生成卡的地方（通常是 Canvas

    public TextMeshProUGUI countText;
    public Image icon; // 卡牌圖像
    public Transform spawnPoint;
    public CanvasGroup canvasGroup; // 用來控制透明度或互動性

    public RectTransform parentCanvas; // 用來生成卡牌的 Canvas
    //public UnityEngine.Events.UnityEvent onPointerDownImmediate;

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        UpdateUI();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"點擊了食材槽：{ingredientName}，剩餘數量：{count}");
        if (count > 0)
        {
            CreateNewCard();
            count--;
            UpdateUI();
        }
    }

    private void CreateNewCard()
    {
        if (ingredientCardPrefab == null) return;

        // 生成在 slot 的 spawnPoint
        GameObject newCard = Instantiate(ingredientCardPrefab, spawnPoint.position, spawnPoint.rotation, parentCanvas.transform);

        IngredientCard card = newCard.GetComponent<IngredientCard>();
        card.Setup(this, ingredientName);

        // 你已經在正確位置了，不需要用滑鼠座標
        // card.transform.position = Mouse.current.position.ReadValue();
    }

    public void ReturnCard()
    {
        count++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        countText.text = count.ToString();

        if (count <= 0)
        {
            canvasGroup.alpha = 0.5f;
            canvasGroup.interactable = false;
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }
    }
}
