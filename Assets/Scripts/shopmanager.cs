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
    public Slider slider;
    public AudioSource bgm;
    public GameObject setpanel;
    public GameObject buypanel;
    public TextMeshProUGUI itemNameText;
    public bagpool pool;


    private string nowbuyingitem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        show.text = "" + data.money;
        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);

        foreach (var remains in data.inbag)
        {
            if (remains.quantity == 0) continue;
            GameObject uiObj = pool.GetObject();
            uiObj.GetComponent<ingredient>().setingredient(remains.name, remains.quantity);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void nextscene()
    {
        SceneManager.LoadScene("Game");
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

    public void openbuy(string itname)
    {
        buypanel.SetActive(true);
        data.setnowprise(itname);
        itemprise.text = "$ " + data.nowprise;
        nowbuyingitem = itname;
        itemNameText.text = itname;
        itemdisc.text = ingreddiscription.getinfo(itname);
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

    public void AddItem()
    {
        if (data.money < data.nowprise) return;
        data.money -= data.nowprise;
        show.text = "" + data.money;

        data.ingreds_data isexist = data.inbag.Find(x => x.name == nowbuyingitem);
        if (isexist != null)
        {
            isexist.quantity++;
            GameObject numup = pool.pool.Find(obj => obj.GetComponent<ingredient>().thename == nowbuyingitem);
            if (numup != null)
                numup.GetComponent<ingredient>().updatenum(isexist.quantity);
        }
        else
        {
            data.inbag.Add(new data.ingreds_data(nowbuyingitem));

            GameObject uiObj = pool.GetObject();
            uiObj.GetComponent<ingredient>().setingredient(nowbuyingitem, 1);
        }
    }
}
