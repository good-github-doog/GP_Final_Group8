using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class shopmanager : MonoBehaviour
{
    public TextMeshProUGUI show;
    public TextMeshProUGUI itemprise;
    public TextMeshProUGUI itemdisc;
    public TextMeshProUGUI inbagitemtyp;
    public TextMeshProUGUI inbagitemdes;
    public TMP_InputField nowamount;
    public Slider slider;
    public AudioSource bgm;
    public AudioSource effect;
    public AudioClip clicksound;
    public GameObject setpanel;
    public GameObject buypanel;
    public GameObject illustpanel;
    public GameObject hintpanel;
    public GameObject inbagshowpanel;
    public TextMeshProUGUI itemNameText;
    public bagpool pool;
    public bagpool poolforgoods;
    public Image inbagitemimg;

    private string nowbuyingitem;
    private List<string> nowgoods;
    private int amount = 1;
    private int totalprise = 0;
    private int maxbuyamount = 99;

    [Header("Hint UI (Confirm / Bought)")]
    public GameObject hintConfirmRoot;   // Yes/No 那坨
    public GameObject hintBoughtRoot;    // 已購買圖片那坨
    public Button hintYesButton;         // 可選：要鎖互動
    public Button hintNoButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 開啟商店即為新的一天開始，先重置當日收支統計
        data.BeginNewDay();
        show.text = "" + data.money;
        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);

        Settoolitem("mixer");
        if (data.nowstage >= 2 || data.clearstage >= 2) Settoolitem("oven");
        foreach (var remains in data.inbag)
        {
            if (remains.quantity <= 0) continue;
            GameObject uiObj = pool.GetObject();
            uiObj.GetComponent<ingredient>().setingredient(remains.name, remains.quantity);
            string tmp = remains.name;
            uiObj.GetComponent<Button>().onClick.RemoveAllListeners();
            uiObj.GetComponent<Button>().onClick.AddListener(() => openinbagshow(tmp));
            uiObj.GetComponent<Button>().onClick.AddListener(() => effect.PlayOneShot(clicksound));
        }

        data.goodsmap.TryGetValue(data.nowstage, out nowgoods);
        foreach (var good in nowgoods)
        {
            GameObject obj = poolforgoods.GetObject();
            obj.GetComponent<Image>().sprite = data.GetSprite(good);
            Button btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => openbuy(good));
            btn.onClick.AddListener(() => effect.PlayOneShot(clicksound));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ActivateCheatMode();
        }
    }

    private void ActivateCheatMode()
    {
        Debug.Log("[CHEAT MODE] Activated! Unlocking all stages and adding ingredients...");

        // 解鎖所有關卡
        // data.clearstage = 3;
        // data.hasCompletedStageHellCuisine[0] = true;
        // data.hasCompletedStageHellCuisine[1] = true;
        // data.hasCompletedStageHellCuisine[2] = true;
        // Debug.Log("[CHEAT MODE] All stages unlocked!");

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

        IngredientManager ingredientManager = FindAnyObjectByType<IngredientManager>();
        if (ingredientManager != null)
        {
            ingredientManager.RefreshSlots();
            Debug.Log("[CHEAT MODE] Ingredient list UI refreshed!");
        }
        else
        {
            Debug.LogWarning("[CHEAT MODE] IngredientManager not found, UI not refreshed");
        }
    }

    public void nextscene()
    {
        var saveManager = CloudSaveManager.Instance ?? FindAnyObjectByType<CloudSaveManager>();
        if (saveManager != null)
        {
            _ = saveManager.SaveAsync();
            Debug.Log("[CloudSave] Save triggered when leaving shop");
        }
        else
        {
            Debug.LogWarning("[CloudSave] No CloudSaveManager found when leaving shop");
        }

        // Choose scene based on the current stage.
        string sceneName = "Game";
        switch (data.nowstage)
        {
            case 1:
                sceneName = "Level 1";
                break;
            case 2:
                sceneName = "Game";
                break;
            case 3:
                sceneName = "Level 3";
                break;
        }
        Debug.Log("Loading scene: " + sceneName);
        //SceneManager.LoadScene(sceneName);
        IrisTransitionCutout.Instance.LoadSceneWithIris(sceneName);
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

    public void openhint()
    {
        hintpanel.SetActive(true);
        bool bought;
        switch (data.nowstage)
        {
            case 1:
                bought = data.hintUnlockedByStage[0];
                break;
            case 2:
                bought = data.hintUnlockedByStage[1];
                break;
            case 3:
                bought = data.hintUnlockedByStage[2];
                break;
            default:
                bought = false;
                break;
        }
        if (hintConfirmRoot != null) hintConfirmRoot.SetActive(!bought);
        if (hintBoughtRoot != null) hintBoughtRoot.SetActive(bought);
        if (hintYesButton != null) hintYesButton.interactable = !bought;
        if (hintNoButton != null)  hintNoButton.interactable  = !bought;
    }

    public void closehint()
    {
        hintpanel.SetActive(false);
    }

    public void openbuy(string itname)
    {
        amount = 1;
        nowamount.text = "" + amount;
        if (!buypanel.activeSelf) buypanel.SetActive(true);
        data.setnowprise(itname);
        totalprise = data.nowprise;
        itemprise.text = "$ " + data.nowprise;
        nowbuyingitem = itname;
        itemNameText.text = itname;
        itemdisc.text = ingreddiscription.getinfo(itname);
    }

    // Yes 按鈕：確認要買提示（先不扣錢）
    public void OnHintYes()
    {
        // 解鎖「當前關卡」的 hint
        
        if (data.money >= data.hintPrice)
        {
            switch (data.nowstage)
            {
                case 1:
                    data.hintUnlockedByStage[0] = true;
                    break;
                case 2:
                    data.hintUnlockedByStage[0] = true;
                    break;
                case 3:
                    data.hintUnlockedByStage[0] = true;
                    break;
            }
            data.money -= data.hintPrice;
            show.text = "" + data.money;
            if (hintConfirmRoot != null) hintConfirmRoot.SetActive(false);
            if (hintBoughtRoot != null) hintBoughtRoot.SetActive(true);
            if (hintYesButton != null) hintYesButton.interactable = false;
            if (hintNoButton != null)  hintNoButton.interactable  = false;
            Debug.Log($"[Hint] Stage {data.nowstage} hint unlocked. Price = {data.hintPrice} (not charged yet)");

        }
        else return;
        
        

        

        if (effect != null && clicksound != null) effect.PlayOneShot(clicksound);
    }

    // No 按鈕：不買，直接關掉
    public void OnHintNo()
    {
        closehint();

        if (effect != null && clicksound != null) effect.PlayOneShot(clicksound);
    }


    public void plusnum(int nn)
    {
        amount += nn;
        amount = Mathf.Clamp(amount, 1, maxbuyamount);
        totalprise = amount * data.nowprise;
        itemprise.text = "$ " + totalprise;
        nowamount.text = "" + amount;
    }

    public void minusnum(int nn)
    {
        amount -= nn;
        amount = Mathf.Clamp(amount, 1, maxbuyamount);
        totalprise = amount * data.nowprise;
        itemprise.text = "$ " + totalprise;
        nowamount.text = "" + amount;
    }

    public void keyinchange(string vv)
    {
        if (int.TryParse(vv, out int n)) amount = Mathf.Clamp(n, 1, maxbuyamount);
        else amount = 1;
        totalprise = amount * data.nowprise;
        itemprise.text = "$ " + totalprise;
        nowamount.text = "" + amount;
    }

    public void closebuy()
    {
        buypanel.SetActive(false);
    }

    public void setvolume(float value)
    {
        bgm.volume = value;
        data.bgmvol = value;
    }

    public void openillust()
    {
        illustpanel.SetActive(true);
    }

    public void closeillust()
    {
        illustpanel.SetActive(false);
    }

    public void openinbagshow(string itemname)
    {
        inbagshowpanel.SetActive(true);
        inbagitemimg.sprite = data.GetSprite(itemname);
        inbagitemtyp.text = "type : " + data.gettype(itemname);
        inbagitemdes.text = ingreddiscription.getinfo(itemname);
    }

    public void closeinbagshow()
    {
        inbagshowpanel.SetActive(false);
    }

    public void AddItem()
    {
        if (data.money < totalprise) return;
        data.money -= totalprise;
        show.text = "" + data.money;
        data.costIngredients += totalprise;   // 🔸記錄材料花費
        data.AddIngredientPurchase(nowbuyingitem, amount, totalprise);

        string tmp = nowbuyingitem;

        data.ingreds_data isexist = data.inbag.Find(x => x.name == nowbuyingitem);
        if (isexist != null)
        {
            if (isexist.quantity > 0)
            {
                isexist.quantity += amount;
                GameObject numup = pool.pool.Find(obj => obj.GetComponent<ingredient>().thename == nowbuyingitem);
                if (numup != null)
                    numup.GetComponent<ingredient>().updatenum(isexist.quantity);
            }
            else 
            {
                isexist.quantity = amount;
                GameObject uiObj = pool.GetObject();
                uiObj.GetComponent<ingredient>().setingredient(nowbuyingitem, isexist.quantity);
                uiObj.GetComponent<Button>().onClick.RemoveAllListeners();
                uiObj.GetComponent<Button>().onClick.AddListener(() => openinbagshow(tmp));
                uiObj.GetComponent<Button>().onClick.AddListener(() => effect.PlayOneShot(clicksound));
            }
        }
        else
        {
            data.inbag.Add(new data.ingreds_data(nowbuyingitem, amount));

            GameObject uiObj = pool.GetObject();
            uiObj.GetComponent<ingredient>().setingredient(nowbuyingitem, amount);
            uiObj.GetComponent<Button>().onClick.RemoveAllListeners();
            uiObj.GetComponent<Button>().onClick.AddListener(() => openinbagshow(tmp));
            uiObj.GetComponent<Button>().onClick.AddListener(() => effect.PlayOneShot(clicksound));
        }
    }

    private void Settoolitem(string toolname)
    {
        data.ingreds_data isexist = data.inbag.Find(x => x.name == toolname);
        if (isexist != null)
        {
            isexist.quantity = maxbuyamount;
            GameObject numup = pool.pool.Find(obj => obj.GetComponent<ingredient>().thename == toolname);
            if (numup != null)
                numup.GetComponent<ingredient>().updatenum(isexist.quantity);
        }
        else
        {
            data.inbag.Add(new data.ingreds_data(toolname, maxbuyamount));
        }
    }
}
