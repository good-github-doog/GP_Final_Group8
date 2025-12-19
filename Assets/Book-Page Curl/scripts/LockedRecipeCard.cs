using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LockedRecipeCard : MonoBehaviour
{
    static readonly List<LockedRecipeCard> all = new();
    public static void RefreshAll()
    {
        foreach (var c in all) if (c != null) c.Refresh();
    }

    [Header("Identity")]
    public string recipeName;            // 例如 "meatjuice" / "seafoodjuice"

    [Header("Refs")]
    public Button button;
    public Image icon;
    public TextMeshProUGUI label;
    public RecipeBookManualUI bookUI;

    [Header("Locked visuals")]
    public Sprite lockedSprite;
    public string lockedText = "???";

    [Header("Unlocked display (optional)")]
    public string unlockedDisplayName;

    void OnEnable()
    {
        if (!all.Contains(this)) all.Add(this);
        if (button == null) button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        Refresh();
    }

    void OnDisable() => all.Remove(this);

    public void Refresh()
    {
        bool unlocked = IsUnlocked(recipeName);

        // ⭐ 圖片只改顯示，不影響功能
        if (icon != null)
            icon.sprite = unlocked ? data.GetSprite(recipeName) : lockedSprite;

        // ⭐ 文字只改顯示
        if (label != null)
            label.text = unlocked
                ? (string.IsNullOrEmpty(unlockedDisplayName) ? recipeName : unlockedDisplayName)
                : lockedText;

        // ⭐ 關鍵：按鈕永遠可以按
        if (button != null)
            button.interactable = true;
    }

    void OnClick()
    {
        // ⭐ 永遠可以進 Show
        if (bookUI != null)
            bookUI.Show(recipeName);
    }

    bool IsUnlocked(string name)
    {
        return illustdata.isunlocked != null
            && illustdata.isunlocked.TryGetValue(name, out bool ok)
            && ok;
    }
}
