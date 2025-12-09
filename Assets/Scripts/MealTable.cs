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

    private static Dictionary<string, GameObject> foodprefebs = new Dictionary<string, GameObject>();
    public static GameObject GetFood(string path)
    {
        if (foodprefebs.TryGetValue(path, out GameObject pf)) return pf;
        
        pf = Resources.Load<GameObject>(path);
        if (pf != null) foodprefebs[path] = pf;
        return pf;
    }
}