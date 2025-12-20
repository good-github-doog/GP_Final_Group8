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
        {"beef+oven", "steak"}, {"mixer+mushroom+pepper", "doublesauce"}, {"doublesauce+steak", "doublesaucesteak"},
        {"butter+dough+fruit+meat+mixer+vegetable", "chaos"}, {"butter+dough+fruit+mixer+seafood+vegetable", "chaos"}, // hell

        // other
        {"burgerbun", "bun"}, {"sandwich", "sand"}, {"burgerbun+sandwich", "sabu"}, {"dough+oven", "bread"},
        {"apple", "apple"}, {"kiwi", "kiwi"}, {"tomato", "tomato"}, {"pineapple", "pineapple"}

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

            //只針對 ???（hell 類）做解鎖
            bool isMystery = illustdata.illustlist.TryGetValue("hell", out var hellList)
                            && hellList.Contains(resultName);

            if (isMystery)
            {
                // 保險：避免 key 不存在爆炸
                if (illustdata.isunlocked.TryGetValue(resultName, out bool unlocked))
                {
                    if (!unlocked)
                    {
                        illustdata.isunlocked[resultName] = true;
                        Debug.Log($"[Unlock] 解鎖 ??? 料理：{resultName}");
                    }
                }
                else
                {
                    // 你的 isunlocked 字典目前有把所有料理都放進去，理論上不會走到這裡
                    illustdata.isunlocked[resultName] = true;
                    Debug.LogWarning($"[Unlock] isunlocked 沒有 key，已補上並解鎖：{resultName}");
                }
            }


            // 3️⃣ 清除舊食材
            foreach (Transform child in combineArea.transform)
            {
                Destroy(child.gameObject);
            }
            combineArea.ingredientsInArea.Clear();
            combineArea.ingredstypeInArea.Clear();

            if (resultName == "steak" || resultName == "doublesauce")
            {
                var exsit = data.inbag.Find(x => x.name == resultName);
                if (exsit != null) exsit.quantity += 1;
                else data.inbag.Add(new data.ingreds_data(resultName, 1));

                IngredientManager ingredientManager = FindFirstObjectByType<IngredientManager>();
                ingredientManager?.RefreshSlots();

                return;
            }

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
        }
        else
        {
            Debug.Log("沒有這個配方！");
            combineArea.ClearArea();
        }
    }
}
