using UnityEngine;
using TMPro;

public class RecipeItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText; // 你 prefab 上那個文字

    public void SetName(string recipeName)
    {
        titleText.text = recipeName;
    }
}
