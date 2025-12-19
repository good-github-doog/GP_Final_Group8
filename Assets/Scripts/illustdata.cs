using UnityEngine;
using System.Collections.Generic;

public static class illustdata
{
    public static int nowtype = 0;
    public static Dictionary<int, string> illustype = new Dictionary<int, string>()
    {
        {0, "burger"}, {1, "sandwich"}, {2, "salad"}, {3, "pizza"}, {4, "highlevel"}, {5, "hell"}
    };

    public static Dictionary<string, List<string>> illustlist = new Dictionary<string, List<string>>()
    {
        {"burger", new List<string>(){"beefburger", "porkburger", "steakburger", "shrimpburger", "salmonburger", "lobsterburger"}},
        {"sandwich", new List<string>(){"beefsandwich", "porksandwich", "steaksandwich", "shrimpsandwich", "salmonsandwich", "lobstersandwich"}},
        {"salad", new List<string>(){"applesalad", "kiwisalad", "tomatosalad", "pineapplesalad"}},
        {"pizza", new List<string>(){"margheritapizza", "hawaiipizza", "seafoodpizza"}},
        {"highlevel", new List<string>(){"doublesaucesteak", "gumbo", "grilllobimp"}},
        {"hell", new List<string>(){"meatjuice", "seafoodjuice", "rawsealandpizza", "chaos"}}
    };

    public static Dictionary<string, string> typedesc = new Dictionary<string, string>()
    {
        {"burger", "Level 1"},
        {"sandwich", "Level 1"},
        {"salad", "Level 1"},
        {"pizza", "Level 2"},
        {"highlevel", "Level 3"},
        {"hell", "something crazy"}
    };

    public static Dictionary<string, bool> isunlocked = new Dictionary<string, bool>()
    {
        {"beefburger", false}, {"porkburger", false}, {"steakburger", false}, {"shrimpburger", false}, {"salmonburger", false}, {"lobsterburger", false},
        {"beefsandwich", false}, {"porksandwich", false}, {"steaksandwich", false}, {"shrimpsandwich", false}, {"salmonsandwich", false}, {"lobstersandwich", false},
        {"applesalad", false}, {"kiwisalad", false}, {"tomatosalad", false}, {"pineapplesalad", false},
        {"margheritapizza", false}, {"hawaiipizza", false}, {"seafoodpizza", false},
        {"doublesaucesteak", false}, {"gumbo", false}, {"grilllobimp", false},
        {"meatjuice", false}, {"seafoodjuice", false}, {"rawsealandpizza", false}, {"chaos", false}
    };

    public static Dictionary<string, List<string>> foodtoingred = new Dictionary<string, List<string>>()
    {
        {"beefburger", new List<string>(){"beef", "burgerbun", "cheese", "lettuce"}},
        {"porkburger", new List<string>(){"pork", "burgerbun", "cheese", "lettuce"}},
        {"steakburger", new List<string>(){"steak", "burgerbun", "cheese", "lettuce"}},
        {"shrimpburger", new List<string>(){"shrimp", "burgerbun", "cheese", "lettuce"}},
        {"salmonburger", new List<string>(){"salmon", "burgerbun", "cheese", "lettuce"}},
        {"lobsterburger", new List<string>(){"lobster", "burgerbun", "cheese", "lettuce"}},

        {"beefsandwich", new List<string>(){"beef", "mushroom", "sandwich"}},
        {"porksandwich", new List<string>(){"pork", "mushroom", "sandwich"}},
        {"steaksandwich", new List<string>(){"steak", "mushroom", "sandwich"}},
        {"shrimpsandwich", new List<string>(){"shrimp", "mushroom", "sandwich"}},
        {"salmonsandwich", new List<string>(){"salmon", "mushroom", "sandwich"}},
        {"lobstersandwich", new List<string>(){"lobster", "mushroom", "sandwich"}},

        {"applesalad", new List<string>(){"apple", "lettuce"}},
        {"kiwisalad", new List<string>(){"kiwi", "lettuce"}},
        {"tomatosalad", new List<string>(){"tomato", "lettuce"}},
        {"pineapplesalad", new List<string>(){"pineapple", "lettuce"}},

        {"margheritapizza", new List<string>(){"cheese", "dough", "oven", "tomato"}},
        {"hawaiipizza", new List<string>(){"cheese", "dough", "mushroom", "oven", "pineapple"}},
        {"seafoodpizza", new List<string>(){"cheese", "dough", "oven", "shrimp"}},

        {"steak", new List<string>(){"oven", "beef"}},
        {"doublesauce", new List<string>(){"mixer", "mushroom", "pepper"}},
        {"doublesaucesteak", new List<string>(){"doublesauce", "steak"}},
        {"gumbo", new List<string>(){"butter", "lettuce", "mixer", "pepper", "tomato"}},
        {"grilllobimp", new List<string>(){"butter", "lobster", "oven", "pepper", "pineapple", "shrimp"}},

        {"meatjuice", new List<string>(){"fruit", "fruit", "meat", "mixer"}},
        {"seafoodjuice", new List<string>(){"fruit", "fruit", "mixer", "seafood"}},
        {"rawsealandpizza", new List<string>(){"dough", "meat", "seafood"}},
        {"chaos", new List<string>(){"butter", "dough", "fruit", "meat", "mixer", "vegetable"}}
    };
}
