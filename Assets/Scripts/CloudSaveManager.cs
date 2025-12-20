using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;

public class CloudSaveManager : MonoBehaviour
{
    private static CloudSaveManager instance;
    public static CloudSaveManager Instance => instance;
    private const string SaveKey = "player_save_v1";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    private class SaveData
    {
        public int money;
        public List<IngredientEntry> inbag;
        public bool[] hasCompletedStageHellCuisine;
        public bool[] hintUnlockedByStage;
        public List<string> unlockedIllusts;
    }

    [System.Serializable]
    private class IngredientEntry
    {
        public string name;
        public int quantity;
    }

    /// <summary>
    /// 將金錢、背包、地獄料理完成狀態上傳到 Cloud Save。
    /// 需要先完成 Authentication 登入。
    /// </summary>
    public async Task SaveAsync()
    {
        var saveData = new SaveData
        {
            money = data.money,
            hasCompletedStageHellCuisine = data.hasCompletedStageHellCuisine,
            hintUnlockedByStage = data.hintUnlockedByStage,
            inbag = new List<IngredientEntry>(),
            unlockedIllusts = new List<string>()
        };

        foreach (var item in data.inbag)
        {
            saveData.inbag.Add(new IngredientEntry
            {
                name = item.name,
                quantity = item.quantity
            });
        }

        foreach (var kv in illustdata.isunlocked)
        {
            if (kv.Value) saveData.unlockedIllusts.Add(kv.Key);
        }

        string json = JsonUtility.ToJson(saveData);
        var payload = new Dictionary<string, object> { { SaveKey, json } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(payload);
        Debug.Log("[CloudSave] Save successful");
    }

    /// <summary>
    /// 從 Cloud Save 載入存檔並回填到 data.cs。
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            var keys = new HashSet<string> { SaveKey };
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (!results.TryGetValue(SaveKey, out Item saveItem))
            {
                Debug.Log("[CloudSave] No remote save found");
                return;
            }

            string json = saveItem.Value.GetAsString();
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[CloudSave] Save json is empty");
                return;
            }

            var saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData == null)
            {
                Debug.LogWarning("[CloudSave] Parse save json failed");
                return;
            }

            data.money = saveData.money;
            data.hasCompletedStageHellCuisine = saveData.hasCompletedStageHellCuisine ?? new bool[3] { false, false, false };
            data.hintUnlockedByStage = saveData.hintUnlockedByStage ?? new bool[3] { false, false, false };

            data.inbag.Clear();
            if (saveData.inbag != null)
            {
                foreach (var entry in saveData.inbag)
                {
                    data.inbag.Add(new data.ingreds_data(entry.name, entry.quantity));
                }
            }

            // reset all illusts to false, then apply unlocked list
            if (illustdata.isunlocked != null)
            {
                var illustKeys = new List<string>(illustdata.isunlocked.Keys);
                foreach (var k in illustKeys) illustdata.isunlocked[k] = false;
            }
            if (saveData.unlockedIllusts != null)
            {
                foreach (var name in saveData.unlockedIllusts)
                {
                    if (illustdata.isunlocked.ContainsKey(name))
                        illustdata.isunlocked[name] = true;
                }
            }

            Debug.Log("[CloudSave] Load successful");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CloudSave] Load failed: {ex.Message}");
        }
    }
}
