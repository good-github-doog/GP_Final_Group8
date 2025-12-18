using UnityEngine;
using UnityEngine.UI;

public class RecipeSelectButton : MonoBehaviour
{
    public string recipeName;
    public RecipeBookManualUI bookUI;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (bookUI != null) bookUI.Show(recipeName);
        });
    }
}
