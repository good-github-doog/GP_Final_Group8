using UnityEngine;
using System.Collections.Generic;

public class bagpool : MonoBehaviour
{

    public GameObject ingredient_prefeb;
    private int inipoolsize = 15;
    public List<GameObject> pool = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        if (ingredient_prefeb == null) return;
        for (int i = 0; i < inipoolsize; i++)
        {
            GameObject obj = Instantiate(ingredient_prefeb, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        GameObject newObj = Instantiate(ingredient_prefeb, transform);
        pool.Add(newObj);
        //newObj.thename = item;
        return newObj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}
