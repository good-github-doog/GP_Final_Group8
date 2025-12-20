using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text;
using System.Linq;

public class CountingReportManager : MonoBehaviour
{
    //[Header("Bar 設定")]
    public RectTransform incomeBar;   // 綠色收入柱 (IncomeBarToday)
    public RectTransform penaltyBar;  // 紅色懲罰柱 (Penalty)
    public float maxBarHeight = 150f; // 最高柱子高度（自己調）

    //[Header("文字顯示")]
    public TextMeshProUGUI dateText;      // today‘s date 那個
    public TextMeshProUGUI incomeText;    // 顯示今天收入：+1000
    public TextMeshProUGUI penaltyText;   // 顯示懲罰：-300
    public TextMeshProUGUI netText;       // 顯示淨利：+700 或 -100

    [Header("結算文字")]
    public TextMeshProUGUI cashOutLeftText;
    public TextMeshProUGUI cashOutRightText;
    public TextMeshProUGUI cashInLeftText;
    public TextMeshProUGUI cashInRightText;
    public TextMeshProUGUI cashOutText; // fallback: 仍然可以用單欄
    public TextMeshProUGUI cashInText;  // fallback: 仍然可以用單欄
    public TextMeshProUGUI cashFlowText;
    public string separatorLine = "--------------------";
    public int leftColumnWidth = 24;

    //[Header("測試用 / 或從 data 讀")]
    public int incomeValue = 1000;   // 今天賺多少（不含懲罰）
    public int penaltyValue = 300;   // 今天被扣多少

    public bool useDataFromGlobal = true; // 如果你有 data.xxx 就打勾

    // 這裡假設你有一個 data 類別，裡面有今天收入、今天懲罰
    // 你可以依照自己的變數名稱改
    public int penaltyPerKill = 50;  // 每殺一個客人扣多少錢

    void Start()
    {
        // 1. 從 data 取資料（如果有的話）
        if (useDataFromGlobal)
        {
            // ✳ 這幾行依照你自己的 data 類別改名字 ✳
            // 先示範一種寫法：

            // 今天總收入（不含懲罰）：你可以在 Game scene 結束時先存好
            // 例如：data.todayIncome
            incomeValue = data.incomeServe;

            // 懲罰：用今天殺的顧客數 * 每人扣的錢
            penaltyValue = data.penaltyWrong + data.penaltyKill + data.penaltyOther;

            // 如果你目前沒有 todayIncome，也可以先手動在 Inspector 填 incomeValue 測試
        }

        // 2. 更新文字
        UpdateTexts();

        // 3. 更新柱子高度
        UpdateBars();

        UpdateSettlementTexts();
    }

    void UpdateTexts()
    {
        int net = incomeValue - penaltyValue;

        if (dateText != null)
        {
            // 先簡單用系統日期，你也可以自己傳字串進來
            dateText.text = "Day " + data.daynumber;
        }

        if (incomeText != null)
        {
            incomeText.text = $"+{incomeValue}";
        }

        if (penaltyText != null)
        {
            penaltyText.text = $"-{penaltyValue}";
        }

        if (netText != null)
        {
            string sign = net >= 0 ? "+" : "-";
            netText.text = $"淨利：{sign}{Mathf.Abs(net)}";
        }
    }

    void UpdateBars()
    {
        // 為了避免全是 0，先找一個基準
        float max = Mathf.Max(1, Mathf.Abs(incomeValue), Mathf.Abs(penaltyValue));

        // 收入高度
        float incomeH = maxBarHeight * (incomeValue / max);
        print(incomeValue);
        // 懲罰高度
        float penaltyH = maxBarHeight * (penaltyValue / max);
        print(penaltyValue);

        if (incomeBar != null)
        {
            var size = incomeBar.sizeDelta;
            size.y = incomeH;
            incomeBar.sizeDelta = size;
        }

        if (penaltyBar != null)
        {
            var size = penaltyBar.sizeDelta;
            size.y = penaltyH;
            penaltyBar.sizeDelta = size;
        }
    }

    void UpdateSettlementTexts()
    {
        if (cashOutText != null)
        {
            cashOutText.text = BuildCashOutText();
        }

        if (cashInText != null)
        {
            cashInText.text = BuildCashInText();
        }

        var (outLeft, outRight) = BuildCashOutColumns();
        if (cashOutLeftText != null) cashOutLeftText.text = outLeft;
        if (cashOutRightText != null) cashOutRightText.text = outRight;

        var (inLeft, inRight) = BuildCashInColumns();
        if (cashInLeftText != null) cashInLeftText.text = inLeft;
        if (cashInRightText != null) cashInRightText.text = inRight;

        if (cashFlowText != null)
        {
            cashFlowText.text = BuildCashFlowText();
        }
    }

    string BuildCashOutText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Cash out");
        AppendLines(sb, data.ingredientsBoughtToday);
        AppendFooterTotal(sb, GetTotalAmount(data.ingredientsBoughtToday));
        return sb.ToString();
    }

    string BuildCashInText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Cash in");
        AppendLines(sb, data.foodsSoldToday);
        AppendFooterTotal(sb, GetTotalAmount(data.foodsSoldToday));
        return sb.ToString();
    }

    string BuildCashFlowText()
    {
        int cashIn = GetTotalAmount(data.foodsSoldToday);
        int cashOut = GetTotalAmount(data.ingredientsBoughtToday);
        int flow = cashIn - cashOut;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Balance ${flow}");
        return sb.ToString();
    }

    (string left, string right) BuildCashOutColumns()
    {
        return BuildTwoColumnList("Cash out", data.ingredientsBoughtToday);
    }

    (string left, string right) BuildCashInColumns()
    {
        return BuildTwoColumnList("Cash in", data.foodsSoldToday);
    }

    (string left, string right) BuildTwoColumnList(string title, System.Collections.Generic.Dictionary<string, data.DailyItemStat> source)
    {
        StringBuilder left = new StringBuilder();
        StringBuilder right = new StringBuilder();

        left.AppendLine(title);
        right.AppendLine(string.Empty);

        if (source.Count == 0)
        {
            left.AppendLine("None");
            right.AppendLine(string.Empty);
            left.AppendLine(separatorLine);
            right.AppendLine(string.Empty);
        }
        else
        {
            foreach (var kvp in source.OrderBy(k => k.Key))
            {
                left.AppendLine($"{kvp.Key}*{kvp.Value.count}");
                right.AppendLine($"${kvp.Value.totalAmount}");
            }

            left.AppendLine(separatorLine);
            right.AppendLine("");
            right.AppendLine($"${GetTotalAmount(source)}");
        }

        return (left.ToString(), right.ToString());
    }

    void AppendLines(StringBuilder sb, System.Collections.Generic.Dictionary<string, data.DailyItemStat> source)
    {
        if (source.Count == 0)
        {
            sb.AppendLine("None");
            return;
        }

        foreach (var kvp in source)
        {
            sb.AppendLine(FormatLine(kvp.Key, kvp.Value.count, kvp.Value.totalAmount));
        }
    }

    void AppendFooterTotal(StringBuilder sb, int total)
    {
        sb.AppendLine(separatorLine);
        sb.AppendLine(FormatRightAligned($"${total}"));
    }

    int GetTotalAmount(System.Collections.Generic.Dictionary<string, data.DailyItemStat> source)
    {
        int total = 0;
        foreach (var kvp in source) total += kvp.Value.totalAmount;
        return total;
    }

    string FormatLine(string name, int count, int amount)
    {
        string left = $"{name}*{count}";
        string right = $"${amount}";
        return $"{left.PadRight(leftColumnWidth)}{right}";
    }

    string FormatRightAligned(string value)
    {
        int width = Mathf.Max(leftColumnWidth + 8, value.Length);
        return value.PadLeft(width);
    }

    string FormatLabelWithRightAlignedValue(string label, string value)
    {
        int width = Mathf.Max(leftColumnWidth + 8, label.Length + value.Length + 1);
        int pad = Mathf.Max(0, width - value.Length);
        return label.PadRight(pad) + value;
    }

    // ===================== 按鈕事件 =====================

    // 「new day」按鈕
    public void OnNewDayButton()
    {
        int net = incomeValue - penaltyValue;

        // ✳ 決定淨利要不要真的加到總金額
        data.money += net;

        // 清今天的統計
        data.killCountYesterday = data.killCountToday;
        data.killCountToday = 0;
        // data.todayIncome = 0; // 有的話也一起清

        // 回到主遊戲 Scene 名稱自己改
        SceneManager.LoadScene("Game");
    }

    // 「exit」按鈕
    public void OnExitButton()
    {
        // 回主選單或直接 Quit，看你需求
        // Application.Quit();
        SceneManager.LoadScene("home");
    }
}
