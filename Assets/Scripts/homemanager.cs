using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class homemanager : MonoBehaviour
{
    // public TextMeshProUGUI show;
    public Slider slider;
    public AudioSource bgm;
    public GameObject setpanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // show.text = "value : " + data.val;
        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     data.val -= 1;
        //     show.text = "value : " + data.val;
        // }
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     data.reset();
        //     show.text = "value : " + data.val;
        // }
    }

    public void nextscene()
    {
        //data.nowstage = 1;
        //SceneManager.LoadScene("Shopping");
        IrisTransitionCutout.Instance.LoadSceneWithIris("Shopping");

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
