using UnityEngine;
using System.Collections.Generic;

public class bagpool : MonoBehaviour
{

    public GameObject prefeb;
    public int inipoolsize = 15;
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
        if (prefeb == null) return;
        for (int i = 0; i < inipoolsize; i++)
        {
            GameObject obj = Instantiate(prefeb, transform);
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
        GameObject newObj = Instantiate(prefeb, transform);
        pool.Add(newObj);
        return newObj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}
