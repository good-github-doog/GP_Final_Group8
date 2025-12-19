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

    public static void RefreshOne(string recipeName)
    {
        foreach (var c in all)
        {
            if (c == null) continue;
            if (c.recipeName == recipeName)
            {
                c.Refresh();
                return; // 找到就停
            }
        }
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

    [Header("Unlocked Sprite Override (Manual)")]
    public Sprite unlockedSpriteOverride; // 只給 ??? 用，解鎖後用這張圖

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
        {
            if (!unlocked)
            {
                icon.sprite = lockedSprite;
            }
            else
            {
                Sprite s = unlockedSpriteOverride != null ? unlockedSpriteOverride : data.GetSprite(recipeName);

                // 保底：如果 GetSprite 沒拿到，就不要變白，先用 lockedSprite 頂著
                icon.sprite = (s != null) ? s : lockedSprite;
            }

            // 保底：避免 alpha 被你淡入淡出改到 0
            var c = icon.color;
            c.a = 1f;
            icon.color = c;
        }


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
