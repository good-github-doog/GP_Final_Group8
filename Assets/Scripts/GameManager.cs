using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private float timer = 120f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moneyText;

    [Header("Recipe")]
    public GameObject recipePanel;
    public Button recipeButton;
    private RectTransform recipeRect;
    public float slideDuration = 0.4f; // 動畫速度
    public AutoFlip autoFlip;
    public int recipeStartPage = 4;
    public bool FirstTimeInGame = true;
    public bool isRecipeOpen = false;

    void Start()
    {
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

        if (timer <= 0f)
        {
            //SceneManager.LoadScene("Counting");
            IrisTransitionCutout.Instance.LoadSceneWithIris("Counting");

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //SceneManager.LoadScene("Counting");
            IrisTransitionCutout.Instance.LoadSceneWithIris("Counting");

        }
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
}
