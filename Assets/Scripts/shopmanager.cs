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
    public TMP_InputField nowamount;
    public Slider slider;
    public AudioSource bgm;
    public AudioSource effect;
    public AudioClip clicksound;
    public GameObject setpanel;
    public GameObject buypanel;
    public GameObject illustpanel;
    public GameObject hintpanel;
    public TextMeshProUGUI itemNameText;
    public bagpool pool;
    public bagpool poolforgoods;

    private string nowbuyingitem;
    private List<string> nowgoods;
    private int amount = 1;
    private int totalprise = 0;
    private int maxbuyamount = 99;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        show.text = "" + data.money;
        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);

        Settoolitem("mixer");
        if (data.nowstage >= 2 || data.clearstage >= 2) Settoolitem("oven");
        foreach (var remains in data.inbag)
        {
            if (remains.quantity == 0) continue;
            GameObject uiObj = pool.GetObject();
            uiObj.GetComponent<ingredient>().setingredient(remains.name, remains.quantity);
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

    }

    public void nextscene()
    {
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
        data.BeginNewDay();
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

    public void AddItem()
    {
        if (data.money < totalprise) return;
        data.money -= totalprise;
        show.text = "" + data.money;
        data.costIngredients += data.nowprise;   // 🔸記錄材料花費

        data.ingreds_data isexist = data.inbag.Find(x => x.name == nowbuyingitem);
        if (isexist != null)
        {
            isexist.quantity += amount;
            GameObject numup = pool.pool.Find(obj => obj.GetComponent<ingredient>().thename == nowbuyingitem);
            if (numup != null)
                numup.GetComponent<ingredient>().updatenum(isexist.quantity);
        }
        else
        {
            data.inbag.Add(new data.ingreds_data(nowbuyingitem, amount));

            GameObject uiObj = pool.GetObject();
            uiObj.GetComponent<ingredient>().setingredient(nowbuyingitem, amount);
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
