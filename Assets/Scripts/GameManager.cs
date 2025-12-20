using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private List<string> thegudietitles = new List<string>() {
        "basic ui",
        "open the recipe book",
        "open the settings",
        "ingredients",
        "combine",
        "serve"
    };
    private List<string> theguidedescs = new List<string>() {
        "the money\nremaining time",
        " ", " ",
        "in this area, are the ingredients you have\nyou can tap and drag to the combine area",
        "press the cook button to combine the ingredients in the area, to make the dish",
        "drag the made dish to the serve area to serve the customer"
    };

    private float timer = 120f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI guidetitle;
    public TextMeshProUGUI guidedesc;
    public Slider slider;
    public AudioSource bgm;
    public GameObject setpanel;
    public GameObject guidepanel;
    public Image theimg;
    public List<Sprite> guideimgs;
    public int nowguideimg = 0;

    [Header("Recipe")]
    public GameObject recipePanel;
    public Button recipeButton;
    private RectTransform recipeRect;
    public float slideDuration = 0.4f; // 動畫速度
    public AutoFlip autoFlip;
    public int recipeStartPage = 4;
    public bool FirstTimeInGame = true;
    public bool isRecipeOpen = false;
    private bool saveTriggeredOnTimeout = false;

    void Start()
    {
        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);
        theimg.sprite = guideimgs[nowguideimg];
        guidetitle.text = thegudietitles[nowguideimg];
        guidedesc.text = theguidedescs[nowguideimg];

        // Reset kill counter at the start of each day
        data.killCountYesterday = data.killCountToday;
        data.killCountToday = 0;
        Debug.Log("[GameManager] Kill counter reset for new day");

        if (moneyText != null)
        {
            moneyText.text = "$ " + data.money;  // <-- 開始時顯示
        }

        if (recipePanel != null)
        {
            recipeRect = recipePanel.GetComponent<RectTransform>();

            // 一開始放在螢幕下方
            recipeRect.anchoredPosition = new Vector2(0, -Screen.height);
            recipePanel.SetActive(true);

            StartCoroutine(SlideInRecipe());
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(timer).ToString() + "s";
        }

        if (moneyText != null)
        {
            moneyText.text = "$ " + data.money;   // <-- 持續顯示金錢
        }

        if (timer <= 0f && !saveTriggeredOnTimeout)
        {
            saveTriggeredOnTimeout = true;
            SaveAndEndDay();
        }

        HandleShortcuts();
    }

    // ---------------- 開啟動畫 ----------------

    public void OpenRecipe()
    {
        recipePanel.SetActive(true);
        StartCoroutine(SlideInRecipe());
    }

    IEnumerator SlideInRecipe()
    {
        if (isRecipeOpen) yield break; // 如果已經開啟就不再執行
        isRecipeOpen = true;
        if (autoFlip != null)
            autoFlip.AutoFlipToPage(recipeStartPage);
        Vector2 start = new Vector2(0, -Screen.height);
        Vector2 end = Vector2.zero; // 回到中間

        float t = 0;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = t / slideDuration;

            // ease-out 效果
            p = 1 - Mathf.Pow(1 - p, 3);

            recipeRect.anchoredPosition = Vector2.Lerp(start, end, p);
            yield return null;
        }

        recipeRect.anchoredPosition = Vector2.zero;
    }

    // ---------------- 關閉動畫 ----------------

    public void CloseRecipe()
    {
        StartCoroutine(SlideOutRecipe());
        Time.timeScale = 1f;
    }

    IEnumerator SlideOutRecipe()
    {
        Vector2 start = recipeRect.anchoredPosition;
        Vector2 end = new Vector2(0, -Screen.height);

        float t = 0;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = t / slideDuration;

            // ease-in 效果
            p = Mathf.Pow(p, 3);

            recipeRect.anchoredPosition = Vector2.Lerp(start, end, p);
            yield return null;
        }
        recipePanel.SetActive(false);
        isRecipeOpen = false;
    }

    // ==================== SHORTCUTS ====================
    private void HandleShortcuts()
    {
        // Space or S: Skip the day
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.S))
        {
            SkipDay();
        }

        // C: Cheat mode
        if (Input.GetKeyDown(KeyCode.C))
        {
            ActivateCheatMode();
        }
    }

    private void SkipDay()
    {
        Debug.Log("[GameManager] Skipping day...");
        IrisTransitionCutout.Instance.LoadSceneWithIris("Counting");
    }

    private void SaveAndEndDay()
    {
        var saveManager = CloudSaveManager.Instance ?? FindAnyObjectByType<CloudSaveManager>();
        if (saveManager != null)
        {
            _ = saveManager.SaveAsync();
            Debug.Log("[CloudSave] Save triggered when timer ended");
        }
        else
        {
            Debug.LogWarning("[CloudSave] No CloudSaveManager found when timer ended");
        }

        IrisTransitionCutout.Instance.LoadSceneWithIris("Counting");
    }

    private void ActivateCheatMode()
    {
        Debug.Log("[CHEAT MODE] Activated! Unlocking all stages and adding ingredients...");

        // 解鎖所有關卡
        data.clearstage = 3;
        data.hasCompletedStageHellCuisine[0] = true;
        data.hasCompletedStageHellCuisine[1] = true;
        data.hasCompletedStageHellCuisine[2] = true;
        Debug.Log("[CHEAT MODE] All stages unlocked!");

        // 給予所有種類的食材各10個
        string[] allIngredients = {
            "burgerbun", "sandwich", "mushroom", "cheese",
            "salmon", "lettuce", "beef", "pork",
            "apple", "kiwi", "dough", "shrimp",
            "tomato", "pineapple", "butter", "pepper",
            "lobster", "steak", "doublesauce"
        };

        foreach (string ingredientName in allIngredients)
        {
            var existingIngredient = data.inbag.Find(x => x.name == ingredientName);

            if (existingIngredient != null)
            {
                existingIngredient.quantity += 10;
            }
            else
            {
                data.ingreds_data newIngredient = new data.ingreds_data(ingredientName, 10);
                data.inbag.Add(newIngredient);
            }
        }

        Debug.Log("[CHEAT MODE] Added 10 of each ingredient!");
    }

    public void opensetting()
    {
        setpanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void closesetting()
    {
        setpanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void openguide()
    {
        guidepanel.SetActive(true);
    }

    public void closeguide()
    {
        guidepanel.SetActive(false);
    }

    public void changeimg(bool lorr)
    {
        nowguideimg += lorr ? -1 : 1;
        if (nowguideimg < 0) nowguideimg = guideimgs.Count - 1;
        else if (nowguideimg >= guideimgs.Count) nowguideimg = 0;
        theimg.sprite = guideimgs[nowguideimg];
        guidetitle.text = thegudietitles[nowguideimg];
        guidedesc.text = theguidedescs[nowguideimg];
    }

    public void setvolume(float value)
    {
        bgm.volume = value;
        data.bgmvol = value;
    }
}
