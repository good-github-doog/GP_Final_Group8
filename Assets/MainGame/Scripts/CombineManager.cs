using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombineManager : MonoBehaviour
{
    public CombineArea combineArea;
    //public GameObject cardPrefab;
    public GameObject resultPrefab;

    [Header("Recipe UI")]
    public Transform recipeListParent;   // ScrollView/Content
    public GameObject recipeItemPrefab;  // 你的食譜 prefab（有 RecipeItemUI + Text）


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
        {"butter+dough+fruit+meat+mixer+vegetable", "chaos"}, {"butter+dough+fruit+mixer+seafood+vegetable", "chaos"} // hell

    };

    
    void Start()
    {
        prefabDict = new Dictionary<string, GameObject>();
        foreach (var item in prefabList)
            prefabDict[item.name] = item.prefab;
        
        BuildRecipeUIForStage(data.nowstage);

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
        string resultName = null;

        if (recipeBook.TryGetValue(comboKey, out resultName) || recipeBook.TryGetValue(keybytype, out resultbytype))
        {
            if (resultName == null) resultName = resultbytype;
        }

        if (resultName != null)
        {
            Debug.Log($"合成成功：{resultName}");

            if (illustdata.isunlocked[resultName] == false)
            {
                illustdata.isunlocked[resultName] = true;
                Debug.Log($"解鎖新料理：{resultName}");
            }

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

            // 檢查是否為地獄料理並設置完成狀態
            if (MealTable.MealMap.TryGetValue(resultName, out int mealId))
            {
                if (mealId == 16 || mealId == 17) // meatjuice or seafoodjuice
                {
                    data.hasCompletedStageHellCuisine[0] = true;
                    Debug.Log("完成 Stage 1 地獄料理！");
                }
                else if (mealId == 21) // rawsealandpizza
                {
                    data.hasCompletedStageHellCuisine[1] = true;
                    Debug.Log("完成 Stage 2 地獄料理！");
                }
                else if (mealId == 25) // chaos
                {
                    data.hasCompletedStageHellCuisine[2] = true;
                    Debug.Log("完成 Stage 3 地獄料理！");
                }
            }

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

    void BuildRecipeUIForStage(int stage)
    {
        // 先清掉舊的 UI（換關卡時也能重建）
        if (recipeListParent != null)
        {
            for (int i = recipeListParent.childCount - 1; i >= 0; i--)
                Destroy(recipeListParent.GetChild(i).gameObject);
        }

        // 取得本關料理清單（用 resultName 去收集）
        List<string> recipes = GetRecipesByStage(stage);

        // 生成 UI
        foreach (var r in recipes)
        {
            GameObject go = Instantiate(recipeItemPrefab, recipeListParent);
            var ui = go.GetComponent<RecipeItemUI>();
            ui.SetName(r);
        }
    }

    // 你可以依照你 recipeBook 的註解「1/2/3」來決定分關
    List<string> GetRecipesByStage(int stage)
    {
        // 這裡我用「手動分組」最直覺也最穩：把每關有哪些結果寫成 set
        // 你如果想完全自動（從 recipeBook 解析），也可以，我下一則就能給你。

        HashSet<string> stage1 = new HashSet<string>
        {
            "beefburger","porkburger","steakburger","shrimpburger","salmonburger","lobsterburger",
            "beefsandwich","porksandwich","steaksandwich","shrimpsandwich","salmonsandwich","lobstersandwich",
            "applesalad","kiwisalad","tomatosalad","pineapplesalad",
            "meatjuice","seafoodjuice"
        };

        HashSet<string> stage2 = new HashSet<string>
        {
            "margheritapizza","hawaiipizza","seafoodpizza","rawsealandpizza"
        };

        HashSet<string> stage3 = new HashSet<string>
        {
            "grilllobimp","gumbo","steak","doublesauce","doublesaucesteak","chaos"
        };

        var results = new HashSet<string>();
        foreach (var kv in recipeBook)
        {
            string result = kv.Value;
            if (stage == 1 && stage1.Contains(result)) results.Add(result);
            if (stage == 2 && stage2.Contains(result)) results.Add(result);
            if (stage == 3 && stage3.Contains(result)) results.Add(result);
        }

        // 排序讓 UI 穩定
        List<string> list = new List<string>(results);
        list.Sort();
        return list;
    }


}
