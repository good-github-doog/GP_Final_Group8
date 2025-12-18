using UnityEngine;
using UnityEngine.UI;
using System;

public class RecipeBookManualUI : MonoBehaviour
{
    [Header("Left Page")]
    public Image leftResultImage;  // 左頁大圖

    [Serializable]
    public class Entry
    {
        public string recipeName;      // 例如 "beefburger"
        public Sprite resultSprite;    // 你要顯示的大圖（也可用 data.GetSprite，但你怕格式就手拉）
        public GameObject rightPage;   // 右頁對應的配方 panel
    }

    [Header("Manual Mapping")]
    public Entry[] entries;

    public void Show(string recipeName)
    {
        // 全關
        foreach (var e in entries)
            if (e != null && e.rightPage != null)
                e.rightPage.SetActive(false);

        // 找到目標
        foreach (var e in entries)
        {
            if (e == null) continue;
            if (e.recipeName != recipeName) continue;

            if (leftResultImage != null && e.resultSprite != null)
            {
                leftResultImage.sprite = e.resultSprite;
                leftResultImage.preserveAspect = true;
            }

            if (e.rightPage != null)
                e.rightPage.SetActive(true);

            return;
        }

        Debug.LogWarning("Recipe not mapped: " + recipeName);
    }
}
