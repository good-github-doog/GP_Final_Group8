using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class RecipeBookManualUI : MonoBehaviour
{
    [Header("Detail (After Click)")]
    public GameObject detailRoot;
    public Image leftResultImage;
    public Button backButton;

    [Header("Fade (List Mode)")]
    public CanvasGroup listGroup;
    public float fadeDuration = 0.35f;

    [Header("Locked Visual")]
    public Sprite lockedBigSprite;         // 共用的 ?
    public GameObject lockedRightPage;     // fallback（可不用拉）
    [Header("Debug")]
    public bool enableDebugHotkey = true;
    public KeyCode unlockAllKey = KeyCode.F1;

// 你可以選擇：只解鎖 hell(???) 或全部料理
public bool unlockOnlyHell = true;

    [Serializable]
    public class Entry
    {
        public string recipeName;

        [Header("Unlocked")]
        public Sprite resultSprite;
        public GameObject rightPage;

        [Header("Locked (only for ??? recipes)")]
        public Sprite lockedBigSpriteOverride; // 每個 ??? 可不同(可不填)
        public GameObject lockedRightPage;     // 每個 ??? 自己的提示頁
    }

    [Header("Manual Mapping")]
    public Entry[] entries;

    Coroutine fadeCo;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToList);
        }

        ResetView();
    }

    void Update()
    {
        if (!enableDebugHotkey) return;

        if (Input.GetKeyDown(unlockAllKey))
        {
            DebugUnlockAll();
        }
    }

    [ContextMenu("DEBUG/Unlock All Recipes")]
    public void DebugUnlockAll()
    {
        if (illustdata.isunlocked == null) return;

        if (unlockOnlyHell)
        {
            if (illustdata.illustlist.TryGetValue("hell", out var hellList))
            {
                foreach (var name in hellList)
                {
                    if (illustdata.isunlocked.ContainsKey(name))
                        illustdata.isunlocked[name] = true;
                }
            }
        }
        else
        {
            // 全部料理都解鎖
            var keys = new System.Collections.Generic.List<string>(illustdata.isunlocked.Keys);
            foreach (var k in keys)
                illustdata.isunlocked[k] = true;
        }

        Debug.Log($"[DEBUG] Unlocked {(unlockOnlyHell ? "hell" : "ALL")} recipes!");

        // 如果你有用 LockedRecipeCard 管理 ??? 卡片顯示，順便刷新
        LockedRecipeCard.RefreshAll();
    }

    // ✅ 關掉所有右頁（包含 unlocked/locked）
    void HideAllRightPages()
    {
        if (entries == null) return;

        foreach (var e in entries)
        {
            if (e == null) continue;

            if (e.rightPage != null)
                e.rightPage.SetActive(false);

            if (e.lockedRightPage != null)
                e.lockedRightPage.SetActive(false);
        }

        if (lockedRightPage != null)
            lockedRightPage.SetActive(false);
    }

    public void ResetView()
    {
        if (detailRoot != null) detailRoot.SetActive(false);

        if (leftResultImage != null)
            leftResultImage.gameObject.SetActive(false);

        HideAllRightPages();

        if (listGroup != null)
        {
            listGroup.gameObject.SetActive(true);
            listGroup.alpha = 1f;
            listGroup.interactable = true;
            listGroup.blocksRaycasts = true;
        }
    }

    public void BackToList()
    {
        if (detailRoot != null) detailRoot.SetActive(false);

        HideAllRightPages();

        if (leftResultImage != null)
            leftResultImage.gameObject.SetActive(false);

        FadeList(true);
    }

    public void Show(string recipeName)
    {
        // 找 entry
        Entry target = null;
        if (entries != null)
        {
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (e.recipeName == recipeName)
                {
                    target = e;
                    break;
                }
            }
        }

        if (target == null)
        {
            Debug.LogWarning("Recipe not mapped: " + recipeName);
            return;
        }

        // ✅ 判斷是不是 hell 類（???）
        bool isHell = illustdata.illustlist.TryGetValue("hell", out var hellList)
                   && hellList.Contains(recipeName);

        // ✅ 只有 hell 才看 isunlocked；其他都視為 unlocked
        bool unlocked = true;
        if (isHell)
        {
            // 用 TryGetValue 更安全（避免 key 不存在直接炸）
            unlocked = illustdata.isunlocked.TryGetValue(recipeName, out bool ok) && ok;
        }

        // ✅ 先清場：關掉所有右頁（包含 locked）
        HideAllRightPages();

        // 左頁圖
        if (leftResultImage != null)
        {
            leftResultImage.gameObject.SetActive(true);

            if (unlocked)
            {
                leftResultImage.sprite = target.resultSprite;
            }
            else
            {
                // ✅ 未解鎖：先用 entry override，沒有才用共用 ?
                var s = target.lockedBigSpriteOverride != null ? target.lockedBigSpriteOverride : lockedBigSprite;
                leftResultImage.sprite = s;
            }

            leftResultImage.preserveAspect = true;
        }

        // 右頁
        if (unlocked)
        {
            if (target.rightPage != null)
                target.rightPage.SetActive(true);
        }
        else
        {
            if (target.lockedRightPage != null)
                target.lockedRightPage.SetActive(true);
            else if (lockedRightPage != null)
                lockedRightPage.SetActive(true);
        }

        if (detailRoot != null)
            detailRoot.SetActive(true);

        FadeList(false);
    }

    void FadeList(bool show)
    {
        if (listGroup == null) return;

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeCanvasGroup(listGroup, show ? 1f : 0f, fadeDuration, disableWhenZero: !show));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup g, float target, float dur, bool disableWhenZero)
    {
        if (target <= 0f)
        {
            g.interactable = false;
            g.blocksRaycasts = false;
        }
        else
        {
            g.gameObject.SetActive(true);
            g.interactable = true;
            g.blocksRaycasts = true;
        }

        float start = g.alpha;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = (dur <= 0.0001f) ? 1f : Mathf.Clamp01(t / dur);
            g.alpha = Mathf.Lerp(start, target, p);
            yield return null;
        }

        g.alpha = target;

        if (disableWhenZero && Mathf.Approximately(target, 0f))
            g.gameObject.SetActive(false);
    }
}
