using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class homemanager : MonoBehaviour
{
    // public TextMeshProUGUI show;
    public Slider slider;
    public AudioSource bgm;
    public GameObject setpanel;
    public GameObject lockPanel;  // 關卡鎖定提示面板
    public TextMeshProUGUI lockMessage;  // 提示訊息文字
    public Button okButton;  // 關閉彈窗按鈕

    [Header("Stage Billboards")]
    public GameObject stage2Billboard;  // 第二關的告示牌
    public GameObject stage3Billboard;  // 第三關的告示牌
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // show.text = "value : " + data.val;
        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);

        // 根據關卡解鎖狀態設置 Billboard
        UpdateBillboards();
    }

    // Update is called once per frame
    void Update()
    {
        HandleShortcuts();
    }

    public void nextscene()
    {
        // 檢查關卡是否已解鎖
        if (data.nowstage == 2 && !data.hasCompletedStageHellCuisine[0])
        {
            ShowLockMessage("Stage 2 Locked!\nComplete Stage 1 Hell Cuisine first.");
            return;  // 阻止進入
        }
        else if (data.nowstage == 3 && !data.hasCompletedStageHellCuisine[1])
        {
            ShowLockMessage("Stage 3 Locked!\nComplete Stage 2 Hell Cuisine first.");
            return;  // 阻止進入
        }

        //data.nowstage = 1;
        //SceneManager.LoadScene("Shopping");
        IrisTransitionCutout.Instance.LoadSceneWithIris("Shopping");

    }

    // 顯示關卡鎖定提示彈窗
    private void ShowLockMessage(string message)
    {
        if (lockPanel != null)
        {
            lockPanel.SetActive(true);
            if (lockMessage != null)
            {
                lockMessage.text = message;
            }
        }
    }

    // 關閉關卡鎖定提示彈窗
    public void CloseLockPanel()
    {
        if (lockPanel != null)
        {
            lockPanel.SetActive(false);
        }
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

    public void setvolume(float value)
    {
        bgm.volume = value;
        data.bgmvol = value;
    }

    private void UpdateBillboards()
    {
        // 如果第二關已解鎖（完成第一關地獄料理），隱藏第二關的 Billboard
        if (data.hasCompletedStageHellCuisine[0] && stage2Billboard != null)
        {
            stage2Billboard.SetActive(false);
            Debug.Log("[HomeManager] Stage 2 unlocked, hiding billboard");
        }

        // 如果第三關已解鎖（完成第二關地獄料理），隱藏第三關的 Billboard
        if (data.hasCompletedStageHellCuisine[1] && stage3Billboard != null)
        {
            stage3Billboard.SetActive(false);
            Debug.Log("[HomeManager] Stage 3 unlocked, hiding billboard");
        }
    }

    // ==================== SHORTCUTS ====================
    private void HandleShortcuts()
    {
        // C: Cheat mode
        if (Input.GetKeyDown(KeyCode.C))
        {
            ActivateCheatMode();
        }
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

        // 更新 Billboard 狀態
        UpdateBillboards();
    }
}
