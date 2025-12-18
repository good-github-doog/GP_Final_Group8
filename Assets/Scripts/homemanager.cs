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
    public GameObject lockPanel;  // 關卡鎖定提示面板
    public TextMeshProUGUI lockMessage;  // 提示訊息文字
    public Button okButton;  // 關閉彈窗按鈕
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
        // 檢查關卡是否已解鎖
        if (data.nowstage == 2 && !data.hasCompletedStageHellCuisine[0])
        {
            ShowLockMessage("Stage 2 Locked!\nComplete Stage 1 Hell Cuisine first.");
            return;  // 阻止進入
        }
        else if (data.nowstage == 3 && !data.hasCompletedStageHellCuisine[1])
        {
            ShowLockMessage("Stage 3 Locked!\nComplete Stage 2 Hell Cuisine first.");
            return;  // 阻止進入
        }

        //data.nowstage = 1;
        //SceneManager.LoadScene("Shopping");
        IrisTransitionCutout.Instance.LoadSceneWithIris("Shopping");

    }

    // 顯示關卡鎖定提示彈窗
    private void ShowLockMessage(string message)
    {
        if (lockPanel != null)
        {
            lockPanel.SetActive(true);
            if (lockMessage != null)
            {
                lockMessage.text = message;
            }
        }
    }

    // 關閉關卡鎖定提示彈窗
    public void CloseLockPanel()
    {
        if (lockPanel != null)
        {
            lockPanel.SetActive(false);
        }
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
