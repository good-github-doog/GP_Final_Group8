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

    public TextMeshProUGUI countText;
    public Image icon; // 卡牌圖像
    public CanvasGroup canvasGroup; // 用來控制透明度或互動性

    private RectTransform parentCanvas;

    private void Start()
    {
        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }
        UpdateUI();
    }

    // ⭐ 新增：讓 Manager 可以初始化這個 Slot
    public void Initialize(string name, int quantity, GameObject cardPrefab)
    {
        ingredientName = name;
        count = quantity;
        ingredientCardPrefab = cardPrefab;
        
        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }
        
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

        GameObject newCard = Instantiate(ingredientCardPrefab, parentCanvas.transform);
        IngredientCard card = newCard.GetComponent<IngredientCard>();
        card.Setup(this, ingredientName);
        card.transform.position = Mouse.current.position.ReadValue();
    }

    public void ReturnCard()
    {
        count++;
        UpdateUI();
    }

    public void UpdateUI() // ⭐ 改為 public，讓 Manager 可以調用
    {
        if (countText != null)
        {
            countText.text = count.ToString();
        }

        if (canvasGroup != null)
        {
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

    // ⭐ 新增：獲取當前數量（供 Manager 保存用）
    public int GetCurrentCount()
    {
        return count;
    }
}