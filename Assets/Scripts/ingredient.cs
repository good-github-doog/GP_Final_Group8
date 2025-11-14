using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ingredient : MonoBehaviour
{
    public TextMeshProUGUI number;
    public string thename;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setingredient(string item, int nn)
    {
        number.text = item +' ' +  nn;
        Color cc = Color.white;
        switch(item)
        {
            case "4" :
                cc = Color.red;
                break;
            case "5" :
                cc = Color.blue;
                break;
        }
        gameObject.GetComponent<Image>().color = cc;

        thename = item;
    }

    public void updatenum(int nn)
    {
        number.text = thename +  " " + nn;
    }
}
