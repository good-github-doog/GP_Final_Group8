using UnityEngine;
using System.Collections.Generic;

public static class data
{
    public static string[] ingredname = {
        "oven", "mixer",

        "burgerbun", "sandwich", "mushroom", "cheese",
        "salmon", "lettuce", "beef", "pork",
        "apple", "kiwi", "dough", "shrimp",
        "tomato", "pineapple", "butter", "pepper",
        "lobster",

        "steak", "doublesauce"
    };
    private static Dictionary<string, string> typemap = new Dictionary<string, string>() {
        {"oven", "oven"}, {"mixer", "mixer"},
        {"beef", "meat"}, {"pork", "meat"}, {"steak", "meat"},
        {"salmon", "seafood"}, {"shrimp", "seafood"}, {"lobster", "seafood"},
        {"burgerbun", "dough"}, {"sandwich", "dough"}, {"dough", "dough"},
        {"lettuce", "vegetable"},
        {"mushroom", "mushroom"}, {"cheese", "cheese"}, {"butter", "butter"}, {"pepper", "pepper"},
        {"apple", "fruit"}, {"kiwi", "fruit"}, {"tomato", "fruit"}, {"pineapple", "fruit"},
        {"doublesauce", "sauce"}
    };
    public class ingreds_data
    {
        public string name;
        public int quantity;
        public string type;

        public ingreds_data(string nn)
        {
            name = nn;
            quantity = 1;
            typemap.TryGetValue(nn, out type);
        }
    }

    public static int money = 1000;
    public static int nowprise = 0;
    public static float bgmvol = 1f;
    public static List<ingreds_data> inbag = new List<ingreds_data>();
    public static int killCountToday = 0;
    public static int killCountYesterday = 0;
    public static int clearstage = 1;
    public static int nowstage = 0;

    public static void reset()
    {
        // val = 10;
    }

    public static string gettype(string tar)
    {
        typemap.TryGetValue(tar, out string tp);
        return tp;
    }

    public static void setnowprise(string ss)
    {
        switch (ss)
        {
            case "burgerbun":
                nowprise = 20;
                break;
            case "sandwich":
                nowprise = 25;
                break;
            case "mushroom":
                nowprise = 35;
                break;
            case "cheese":
                nowprise = 40;
                break;
            case "salmon":
                nowprise = 60;
                break;
            case "lettuce":
                nowprise = 15;
                break;
            case "beef":
                nowprise = 70;
                break;
            case "pork":
                nowprise = 50;
                break;
        }
    }
}
