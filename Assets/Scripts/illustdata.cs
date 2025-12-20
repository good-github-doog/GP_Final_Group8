using UnityEngine;
using System.Collections.Generic;

public static class illustdata
{
    public static int nowtype = 0;
    public static Dictionary<int, string> illustype = new Dictionary<int, string>()
    {
        {0, "burger"}, {1, "sandwich"}, {2, "salad"}, {3, "pizza"}, {4, "highlevel"}, {5, "2nd made ingred"}, {6, "hell"}
    };

    public static Dictionary<string, List<string>> illustlist = new Dictionary<string, List<string>>()
    {
        {"burger", new List<string>(){"beefburger", "porkburger", "steakburger", "shrimpburger", "salmonburger", "lobsterburger"}},
        {"sandwich", new List<string>(){"beefsandwich", "porksandwich", "steaksandwich", "shrimpsandwich", "salmonsandwich", "lobstersandwich"}},
        {"salad", new List<string>(){"applesalad", "kiwisalad", "tomatosalad", "pineapplesalad"}},
        {"pizza", new List<string>(){"margheritapizza", "hawaiipizza", "seafoodpizza"}},
        {"2nd made ingred", new List<string>(){"steak", "doublesauce"}},
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
        {"hell", "crazy"},
        
        {"beefburger", "burger"}, {"porkburger", "burger"}, {"steakburger", "burger"}, {"shrimpburger", "burger"}, {"salmonburger", "burger"}, {"lobsterburger", "burger"},
        {"beefsandwich", "sandwich"}, {"porksandwich", "sandwich"}, {"steaksandwich", "sandwich"}, {"shrimpsandwich", "sandwich"}, {"salmonsandwich", "sandwich"}, {"lobstersandwich", "sandwich"},
        {"applesalad", "salad"}, {"kiwisalad", "salad"}, {"tomatosalad", "salad"}, {"pineapplesalad", "salad"},
        {"margheritapizza", "pizza"}, {"hawaiipizza", "pizza"}, {"seafoodpizza", "pizza"},
        {"steak", "2nd made ingred"}, {"doublesauce", "2nd made ingred"},
        {"doublesaucesteak", "highlevel"}, {"gumbo", "highlevel"}, {"grilllobimp", "highlevel"},
        {"meatjuice", "hell"}, {"seafoodjuice", "hell"}, {"rawsealandpizza", "hell"}, {"chaos", "hell"}
    };

    public static Dictionary<string, string> detailoftype = new Dictionary<string, string>()
    {
        {"burger", "fast food made by four ingredients, the bun may be needed"},
        {"sandwich", "made by three ingredients, kinds number is same as burger"},
        {"salad", "the salad ig green..."},
        {"pizza", "easy to make but..."},
        {"highlevel", "is the difficult one to make"},
        {"hell", "can you unlock all of them ?"},

        {"beefburger", "name : beefburger\nA delicious beef burger with fresh ingredients."},
        {"porkburger", "name : porkburger\nA tasty pork burger with cheese and lettuce."},
        {"steakburger", "name : steakburger\nA premium steak burger for meat lovers."},
        {"shrimpburger", "name : shrimpburger\nA seafood delight in a burger form."},
        {"salmonburger", "name : salmonburger\nA healthy salmon burger with fresh toppings."},
        {"lobsterburger", "name : lobsterburger\nA luxurious lobster burger experience."},

        {"beefsandwich", "name : beefsandwich\nA hearty beef sandwich with mushrooms."},
        {"porksandwich", "name : porksandwich\nA savory pork sandwich with mushrooms."},
        {"steaksandwich", "name : steaksandwich\nA gourmet steak sandwich for connoisseurs."},
        {"shrimpsandwich", "name : shrimpsandwich\nA delightful shrimp sandwich with mushrooms."},
        {"salmonsandwich", "name : salmonsandwich\nA fresh salmon sandwich with mushrooms."},
        {"lobstersandwich", "name : lobstersandwich\nAn exquisite lobster sandwich treat."},

        {"applesalad", "name : applesalad\nA refreshing apple salad with lettuce."},
        {"kiwisalad", "name : kiwisalad\nA tropical kiwi salad with fresh greens."},
        {"tomatosalad", "name : tomatosalad\nA classic tomato salad with crisp lettuce."},
        {"pineapplesalad", "name : pineapplesalad\nA sweet pineapple salad with lettuce."},

        {"margheritapizza", "name : margheritapizza\nA traditional Margherita pizza with fresh ingredients."},
        {"hawaiipizza", "name : hawaiipizza\nA tropical Hawaiian pizza with pineapple and mushrooms."},
        {"seafoodpizza", "name : seafoodpizza\nA seafood pizza loaded with shrimp and cheese."},

        {"steak", "name : steak\nA perfectly cooked steak, ready to serve."},
        {"doublesauce", "name : doublesauce\nA rich and flavorful double sauce made by mushroom and pepper."},
        {"doublesaucesteak", "name : doublesaucesteak\nSteak topped with a rich double sauce."},
        {"gumbo", "name : gumbo\nA flavorful gumbo with butter, vegetables, and spices."},
        {"grilllobimp", "name : grilllobimp\nGrilled lobster and shrimp in a buttery sauce."},

        {"meatjuice", "name : meatjuice\nA unique juice blend of fruits and meats."},
        {"seafoodjuice", "name : seafoodjuice\nA refreshing juice mix of fruits and seafood."},
        {"rawsealandpizza", "name : rawsealandpizza\nA raw pizza combining meat and seafood flavors."},
        {"chaos", "name : chaos\nAn unpredictable dish combining various ingredients."}
    };

    public static Dictionary<string, bool> isunlocked = new Dictionary<string, bool>()
    {
        {"beefburger", false}, {"porkburger", false}, {"steakburger", false}, {"shrimpburger", false}, {"salmonburger", false}, {"lobsterburger", false},
        {"beefsandwich", false}, {"porksandwich", false}, {"steaksandwich", false}, {"shrimpsandwich", false}, {"salmonsandwich", false}, {"lobstersandwich", false},
        {"applesalad", false}, {"kiwisalad", false}, {"tomatosalad", false}, {"pineapplesalad", false},
        {"margheritapizza", false}, {"hawaiipizza", false}, {"seafoodpizza", false},
        {"steak", false}, {"doublesauce", false},
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
