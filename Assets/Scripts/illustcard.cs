using UnityEngine;
using UnityEngine.UI;

public class illustcard : MonoBehaviour
{
    public Image thefoodimg;
    public Transform container;
    public GameObject ingredientprefab;

    private illustmanager mgr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }   

    public void setillust(string illustname, illustmanager m)
    {
        mgr = m;

        if (illustdata.isunlocked[illustname]) thefoodimg.sprite = data.GetSprite("donefoods/" + illustname);
        else thefoodimg.sprite = data.GetSprite("none");

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (string ingred in illustdata.foodtoingred[illustname])
        {
            GameObject obj = Instantiate(ingredientprefab, container);
            if (illustdata.isunlocked[illustname]) {
                obj.GetComponent<Image>().sprite = data.GetSprite(ingred);
                Button btn = obj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => mgr.updatethedesc(ingred));
            }
            else obj.GetComponent<Image>().sprite = data.GetSprite("none");
        }
    }
}
