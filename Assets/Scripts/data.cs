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

        public ingreds_data(string nn, int n)
        {
            name = nn;
            quantity = n;
            typemap.TryGetValue(nn, out type);
        }
    }

    public static int money = 1000;
    public static int nowprise = 0;

    public static int daynumber = 1;
    // ▶ 一天開始時的金額
    public static int dayStartMoney = 1000;

    // ▶ 今日收支細項
    public static int costIngredients = 0; // 買材料花費
    public static int incomeServe = 0; // 正確送餐收入
    public static int penaltyWrong = 0; // 送錯 / 其他失誤扣錢
    public static int penaltyKill = 0; // 殺顧客扣錢
    public static int penaltyOther = 0; // 其他想加的懲罰
    public static float bgmvol = 1f;
    public static List<ingreds_data> inbag = new List<ingreds_data>();
    public static int killCountToday = 0;
    public static int killCountYesterday = 0;
    public static int clearstage = 1;
    public static int nowstage = 1;
    public static int isPanicMode = 0; // 0 = normal, 1 = panic (killing event occurred)

    // 地獄料理完成狀態 [Stage1, Stage2, Stage3]
    public static bool[] hasCompletedStageHellCuisine = new bool[3] { false, false, false };

    // ===========================
    // ▶ 開始「新的一天」時呼叫
    // ===========================
    public static void BeginNewDay()
    {
        dayStartMoney = money;

        costIngredients = 0;
        incomeServe = 0;
        penaltyWrong = 0;
        penaltyKill = 0;
        penaltyOther = 0;

        killCountYesterday = killCountToday;
        killCountToday = 0;
    }

    // 檢查是否達到遊戲結束條件（3次擊殺）
    public static bool IsGameOver()
    {
        return (killCountYesterday + killCountToday >= 3);
    }

    // 重置遊戲數據到初始狀態
    public static void reset()
    {
        money = 1000;
        inbag.Clear();
        killCountToday = 0;
        killCountYesterday = 0;
        daynumber = 1;
        dayStartMoney = 1000;
        costIngredients = 0;
        incomeServe = 0;
        penaltyWrong = 0;
        penaltyKill = 0;
        penaltyOther = 0;
        clearstage = 1;
        nowstage = 1;
        isPanicMode = 0;
        hasCompletedStageHellCuisine = new bool[3] { false, false, false };
    }

    public static string gettype(string tar)
    {
        typemap.TryGetValue(tar, out string tp);
        return tp;
    }

    private static Dictionary<string, int> prisemap = new Dictionary<string, int>() {
        {"burgerbun", 20}, {"sandwich", 25}, {"mushroom", 35}, {"cheese", 40},
        {"salmon", 60}, {"lettuce", 15}, {"beef", 70}, {"pork", 50},
        {"apple", 20}, {"pineapple", 20}, {"tomato", 25}, {"butter", 40},
        {"pepper", 35}, {"shrimp", 60}, {"dough", 30}
    };
    public static void setnowprise(string ss)
    {
        prisemap.TryGetValue(ss, out nowprise);
    }

    private static List<string> stage1goods = new List<string>() {
        "burgerbun", "sandwich", "beef", "pork", "salmon", "mushroom", "cheese", "lettuce", "apple"
    };
    private static List<string> stage2goods = new List<string>() {
        "dough", "cheese", "tomato", "pineapple", "mushroom", "shrimp", "salmon", "beef", "pork"
    };
    private static List<string> stage3goods = new List<string>() {
        "pepper", "butter", "tomato", "lettuce", "pineapple", "mushroom", "shrimp", "salmon", "beef", "pork", "dough", "burgerbun", "sandwich"
    };
    public static Dictionary<int, List<string>> goodsmap = new Dictionary<int, List<string>>() {
        {1, stage1goods}, {2, stage2goods}, {3, stage3goods}
    };

    private static Dictionary<string, Sprite> piccache = new Dictionary<string, Sprite>();
    public static Sprite GetSprite(string path)
    {
        if (piccache.TryGetValue(path, out Sprite sp)) return sp;

        sp = Resources.Load<Sprite>(path);
        if (sp != null) piccache[path] = sp;
        return sp;
    }
}
