using UnityEngine;
using TMPro;

public class illustmanager : MonoBehaviour
{
    public TextMeshProUGUI nowillustype;
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public bagpool thepool;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nowillustype.text = illustdata.illustype[illustdata.nowtype];
        showillust();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updatethedesc(string ss)
    {
        title.text = "type : " + data.gettype(ss);
        desc.text = ingreddiscription.getinfo(ss);
    }

    public void updatethedescwithdonefood(string ss)
    {
        title.text = "type : " + illustdata.typedesc[ss];
        desc.text = illustdata.detailoftype[ss];
    }

    public void changeillustype(bool lorr)
    {
        illustdata.nowtype += lorr ? -1 : 1;
        if (illustdata.nowtype < 0) illustdata.nowtype = illustdata.illustype.Count - 1;
        else if (illustdata.nowtype >= illustdata.illustype.Count) illustdata.nowtype = 0;
        nowillustype.text = illustdata.illustype[illustdata.nowtype];

        showillust();
    }

    private void showillust()
    {
        foreach (var child in thepool.pool)
        {
            child.SetActive(false);
        }
        string typekey = illustdata.illustype[illustdata.nowtype];
        updatethedescwithdonefood(typekey);
        foreach (string illustname in illustdata.illustlist[typekey])
        {
            GameObject obj = thepool.GetObject();
            obj.GetComponent<illustcard>().setillust(illustname, this);
        }
    }
}
