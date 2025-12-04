using UnityEngine;
using System.Collections.Generic;

public static class ingreddiscription
{
    public static Dictionary<string, string> desctable = new Dictionary<string, string>()
    {
        { "pork", "the pig meet" },
        { "beef", "the cow meet" },
        { "lettuce", "some kind of vegtable" },
        { "cheese", "mouse: yummy" },
        { "salmon", "fisssssh" },
        { "mushroom", "wierd shape" },
        { "sandwich", "toast of triangle" },
        { "burgerbun", "hamberger!!" },
        { "tomato", "red fruit? vegetable?" },
        { "pineapple", "stab" },
        { "apple", "!doctor" },
        { "kiwi", "I'm bird~" },
        { "dough", "need to bake" },
        { "butter", "exquisite~" },
        { "pepper", "blaaaack" },
        { "shrimp", "emmm...." },
        { "lobster", "high level ingred" },

        { "steak", "2nd ingred" },
        { "doublesauce", "yuck" },

        { "mixer", "make things together" },
        { "oven", "hitttttt up" }
    };

    public static string getinfo(string tag)
    {
        if (desctable.TryGetValue(tag, out string desc))
        {
            return desc;
        }
        return "No Description Found";
    }
}
