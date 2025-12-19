using System.Collections.Generic;
using UnityEngine;

public static class MealTable
{
    public static Dictionary<string, int> MealMap = new Dictionary<string, int>()
    {
        {"beefburger", 0}, {"porkburger", 1}, {"salmonburger", 2}, {"shrimpburger", 3}, {"lobsterburger", 4}, {"steakburger", 5},
        {"beefsandwich", 6}, {"porksandwich", 7}, {"salmonsandwich", 8}, {"shrimpsandwich", 9}, {"lobstersandwich", 10}, {"steaksandwich", 11},
        {"applesalad", 12}, {"kiwisalad", 13}, {"tomatosalad", 14}, {"pineapplesalad", 15},
        {"meatjuice", 16}, {"seafoodjuice", 17},

        {"margheritapizza", 18}, {"hawaiipizza", 19}, {"seafoodpizza", 20},
        {"rawsealandpizza", 21},

        {"grilllobimp", 22}, {"gumbo", 23}, {"doublesaucesteak", 24},
        {"chaos", 25}
    };

    public static Dictionary<int, string> OrderText = new Dictionary<int, string>()
    {
        {0, "beef burger..."}, {1, "burger with pig"}, {2, "fish burger ?"}, {3, "burger...but shrimp"}, {4, "higher level of shrimp burger.."}, {5, "i want steak...but still burger"},
        {6, "beef sand..."}, {7, "sand with pig"}, {8, "fish sand ?"}, {9, "sand...but shrimp"}, {10, "higher level of shrimp sand.."}, {11, "i want steak...but still sand"},
        {12, "salad with no doctor"}, {13, "green salad"}, {14, "toto salad"}, {15, "yello ..lad"},
        {16, "juice ??"}, {17, "juice??"},

        {18, "classic marg.."}, {19, "exotic one.."}, {20, "seeeaaa foooood~~"},
        {21, "something special..."},

        {22, "lobimp ?"}, {23, "some soup.."}, {24, "steak...with sauce"},
        {25, "Chaos~!>?~!<"}
    };

    // 🔹 每一道料理的售價（自己改成你要的數字）
    public static Dictionary<int, int> MealPrice = new Dictionary<int, int>()
    {
        {0, 200},   // beefburger
        {1, 165},   // porkburger
        {2, 180},   // salmonburger
        {3, 180},   // shrimpburger
        {4, 600},  // lobsterburger
        {5, 200},  // steakburger

        {6, 170},   // beefsandwich
        {7, 150},   // porksandwich
        {8, 160},   // salmonsandwich
        {9, 160},   // shrimpsandwich
        {10, 600}, // lobstersandwich
        {11, 170}, // steaksandwich

        {12, 50},  // applesalad
        {13, 200},  // kiwisalad
        {14, 60},  // tomatosalad
        {15, 50},  // pineapplesalad

        {16, 600},  // meatjuice
        {17, 600},  // seafoodjuice

        {18, 130},  // margheritapizza
        {19, 160}, // hawaiipizza
        {20, 170}, // seafoodpizza
        {21, 600}, // rawsealandpizza

        {22, 600}, // grilllobimp
        {23, 150},  // gumbo
        {24, 180}, // doublesaucesteak

        {25, 999}  // chaos (想搞笑就超貴🤣)
    };

    public static int GetPrice(int mealId)
    {
        return MealPrice.TryGetValue(mealId, out int price) ? price : 0;
    }

    // 依照「料理名稱字串」拿價格
    public static int GetPrice(string mealName)
    {
        if (MealMap.TryGetValue(mealName, out int id))
            return GetPrice(id);
        return 0;
    }

    private static Dictionary<string, GameObject> foodprefebs = new Dictionary<string, GameObject>();
    public static GameObject GetFood(string path)
    {
        if (foodprefebs.TryGetValue(path, out GameObject pf)) return pf;

        pf = Resources.Load<GameObject>(path);
        if (pf != null) foodprefebs[path] = pf;
        return pf;
    }
}