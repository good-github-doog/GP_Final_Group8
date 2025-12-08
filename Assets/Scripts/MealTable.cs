using System.Collections.Generic;

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
}