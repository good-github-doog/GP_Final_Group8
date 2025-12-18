using UnityEngine;
using UnityEngine.UI;

public class illustcard : MonoBehaviour
{
    public Image thefoodimg;
    public Transform container;
    public GameObject ingredientprefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }   

    public void setillust(string illustname)
    {
        if (illustdata.isunlocked[illustname]) thefoodimg.sprite = data.GetSprite("donefoods/" + illustname);
        else thefoodimg.sprite = data.GetSprite("none");

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (string ingred in illustdata.foodtoingred[illustname])
        {
            GameObject obj = Instantiate(ingredientprefab, container);
            if (illustdata.isunlocked[illustname]) obj.GetComponent<Image>().sprite = data.GetSprite(ingred);
            else obj.GetComponent<Image>().sprite = data.GetSprite("none");
        }
    }
}
