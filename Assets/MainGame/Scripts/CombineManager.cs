using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombineManager : MonoBehaviour
{
    public CombineArea combineArea;
    //public GameObject cardPrefab;

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
        {"麵粉+水", "麵糰"},
        {"麵糰+火", "麵包"},
        {"pork+rice", "porkRice"},
        {"bread+ham", "火腿三明治"},

        // ★ 新增的合成
        {"beef+burgerbun+cheese+lettuce", "hamburger"},
        {"mushroom+salmon+sandwich", "salmonMushroomsandwich"}
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

        // 2️⃣ 檢查配方表
        if (recipeBook.TryGetValue(comboKey, out string resultName))
        {
            Debug.Log($"合成成功：{resultName}");

            // 3️⃣ 清除舊食材
            foreach (Transform child in combineArea.transform)
            {
                Destroy(child.gameObject);
            }
            combineArea.ingredientsInArea.Clear();

            // 4️⃣ 在合成區生成新卡
            // 4️⃣ 在合成區生成新卡（根據結果名稱決定用哪個 Prefab）
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

        }
        else
        {
            Debug.Log("沒有這個配方！");
            combineArea.ClearArea();
        }
    }

}
