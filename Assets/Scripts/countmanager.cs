using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class countmanager : MonoBehaviour
{
    public Slider slider;
    public AudioSource bgm;
    public GameObject setpanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void nextscene(string sc)
    {
        data.daynumber += 1;
        SceneManager.LoadScene(sc);
    }

    public void opensetting()
    {
        setpanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void closesetting()
    {
        setpanel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void setvolume(float value)
    {
        bgm.volume = value;
        data.bgmvol = value;
    }
}
