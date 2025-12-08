using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombineManager : MonoBehaviour
{
    public CombineArea combineArea;
    //public GameObject cardPrefab;
    public GameObject resultPrefab;

    [System.Serializable]
    public class IngredientPrefab
    {
        public string name;
        public GameObject prefab;
    }

    public List<IngredientPrefab> prefabList;
    private Dictionary<string, GameObject> prefabDict;


    // 定義食材組合規則
    private Dictionary<string, string> recipeBook = new Dictionary<string, string>()
    {
        // 1
        {"beef+burgerbun+cheese+lettuce", "beefburger"}, {"burgerbun+cheese+lettuce+pork", "porkburger"},
        {"burgerbun+cheese+lettuce+steak", "steakburger"}, {"burgerbun+cheese+lettuce+shrimp", "shrimpburger"},
        {"burgerbun+cheese+lettuce+salmon", "salmonburger"}, {"burgerbun+cheese+lettuce+lobster", "lobsterburger"},

        {"beef+mushroom+sandwich", "beefsandwich"}, {"mushroom+pork+sandwich", "porksandwich"},
        {"mushroom+sandwich+steak", "steaksandwich"}, {"mushroom+sandwich+shrimp", "shrimpsandwich"},
        {"mushroom+salmon+sandwich", "salmonsandwich"}, {"lobster+mushroom+sandwich", "lobstersandwich"},

        {"apple+lettuce", "applesalad"}, {"kiwi+lettuce", "kiwisalad"},
        {"lettuce+tomato", "tomatosalad"}, {"lettuce+pineapple", "pineapplesalad"},

        {"fruit+fruit+meat+mixer", "meatjuice"}, {"fruit+fruit+mixer+seafood", "seafoodjuice"}, // hell

        // 2
        {"cheese+dough+oven+tomato", "margheritapizza"}, {"cheese+dough+mushroom+oven+pineapple", "hawaiipizza"},
        {"cheese+dough+oven+shrimp", "seafoodpizza"},
        {"dough+meat+seafood", "rawsealandpizza"}, // hell

        // 3
        {"butter+lobster+oven+pepper+pineapple+shrimp", "grilllobimp"},
        {"butter+lettuce+mixer+pepper+tomato", "gumbo"},
        {"oven+beef", "steak"}, {"mixer+mushroom+pepper", "doublesauce"}, {"doublesauce+steak", "doublesaucesteak"},
        {"dough+fruit+meat+mixer+vegetable", "chaos"}, {"dough+fruit+mixer+seafood+vegetable", "chaos"} // hell

    };

    
    void Start()
    {
        prefabDict = new Dictionary<string, GameObject>();
        foreach (var item in prefabList)
            prefabDict[item.name] = item.prefab;
    }

    void SpawnNewCard(string name)
    {
        // GameObject prefabToUse;
        // if (!prefabDict.TryGetValue(name, out prefabToUse))
        //     prefabToUse = cardPrefab; // fallback

        // GameObject newCard = Instantiate(prefabToUse, combineArea.transform);
        // newCard.GetComponent<IngredientCard>().ingredientName = name;
        if (prefabDict.TryGetValue(name, out GameObject prefab))
        {
            Instantiate(prefab, combineArea.transform);
        }
        else
        {
            Debug.LogError($"找不到 {name} 的 Prefab！");
        }
    }

    public void OnCombineButtonClicked()
    {
        //if (combineArea.ingredientsInArea.Count < 2) return;

        // 1️⃣ 排序名稱確保一致
        combineArea.ingredientsInArea.Sort();
        string comboKey = string.Join("+", combineArea.ingredientsInArea);
        Debug.Log($"嘗試合成：{comboKey}");

        combineArea.ingredstypeInArea.Sort();
        string keybytype = string.Join("+", combineArea.ingredstypeInArea);
        string resultbytype = null;

        // 2️⃣ 檢查配方表
        if (recipeBook.TryGetValue(comboKey, out string resultName) || recipeBook.TryGetValue(keybytype, out resultbytype))
        {
            if (resultName == null) resultName = resultbytype;
            Debug.Log($"合成成功：{resultName}");

            // 3️⃣ 清除舊食材
            foreach (Transform child in combineArea.transform)
            {
                Destroy(child.gameObject);
            }
            combineArea.ingredientsInArea.Clear();
            combineArea.ingredstypeInArea.Clear();

            GameObject newCard = Instantiate(resultPrefab, combineArea.transform);
            FoodCard card = newCard.GetComponent<FoodCard>();
            card.setup(resultName);
            card.foodName = resultName;

            // 4️⃣ 在合成區生成新卡
            // 4️⃣ 在合成區生成新卡（根據結果名稱決定用哪個 Prefab）
            /*
            if (prefabDict.TryGetValue(resultName, out GameObject resultPrefab))
            {
                GameObject newCard = Instantiate(resultPrefab, combineArea.transform);

                Debug.Log($"生成 Prefab：{resultPrefab.name}");
                Debug.Log($"Instance 名稱：{newCard.name}");

                // 如果你的成品卡片也需要 FoodCard
                FoodCard card = newCard.GetComponent<FoodCard>();
                if (card != null)
                    card.foodName = resultName;
            }
            else
            {
                Debug.LogError($"找不到 {resultName} 對應的 Prefab！");
            }
            */
        }
        else
        {
            Debug.Log("沒有這個配方！");
            combineArea.ClearArea();
        }
    }

}
