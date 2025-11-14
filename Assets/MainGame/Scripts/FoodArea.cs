using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FoodArea : MonoBehaviour, IDropHandler
{
    public Vector3 desiredPosition = new Vector3(-2.7f, 2f, -1.8f);

    [System.Serializable]
    public class FoodPrefab
    {
        public string name;
        public GameObject prefab;
    }
    public List<FoodPrefab> prefabList;
    private Dictionary<string, GameObject> prefabDict;

    void Start()
    {
        prefabDict = new Dictionary<string, GameObject>();
        foreach (var item in prefabList)
            prefabDict[item.name] = item.prefab;
    }

    public void OnDrop(PointerEventData eventData)
    {
        FoodCard card = eventData.pointerDrag.GetComponent<FoodCard>();
        if (card != null)
        {
            GameObject prefabToUse;
            if (!prefabDict.TryGetValue(card.foodName, out prefabToUse))
            {
                Debug.LogError($"找不到對應的食物 Prefab：{card.foodName}");
                return;
            }

            //Debug.Log(card.transform.position.x);

            /*
            else if (card.transform.position.x < 380)
                desiredPosition = new Vector3(-3.2f, 2f, -1.8f);
            else if (card.transform.position.x < 580)
                desiredPosition = new Vector3(-1.7f, 2f, -1.8f);
            else
                desiredPosition = new Vector3(-0.25f, 2f, -1.8f);
            */
            Quaternion desiredRotation = Quaternion.identity;
            GameObject newFood = Instantiate(prefabToUse, desiredPosition, desiredRotation);
            Debug.Log($"放入上菜區：{card.foodName}");

            Destroy(card.gameObject);
        }
    }

    /*
    public void ClearArea()
    {
        Debug.Log($"[ClearArea] 開始清除，當前子物件數量: {transform.childCount}");

        ingredientsInArea.Clear();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("上菜區已清空");
    }
    */
}
