using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class UserLoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Slider slider;
    public AudioSource bgm;
    public GameObject setpanel;
    [SerializeField] private string homeSceneName = "Home";
    private bool hasLoadedHome = false;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("服務初始化完成");

        slider.value = data.bgmvol;
        bgm.volume = data.bgmvol;
        slider.onValueChanged.AddListener(setvolume);
        
        // (選用功能) 檢查是否已經有登入過的快取
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("偵測到已登入，ID: " + AuthenticationService.Instance.PlayerId);
            await TryLoadHomeAsync();
        }
    }

    // --- 按鈕 1: 註冊帳號 ---
    public async void OnSignUpClicked()
    {
        string uName = usernameInput.text;
        string pWord = passwordInput.text;
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(uName, pWord);
            Debug.Log("註冊成功！ID: " + AuthenticationService.Instance.PlayerId);
            await TryLoadHomeAsync();
        }
        catch (AuthenticationException ex) { Debug.LogError("註冊失敗: " + ex.Message); }
        catch (RequestFailedException ex) { Debug.LogError("請求錯誤: " + ex.Message); }
    }

    // --- 按鈕 2: 登入帳號 ---
    public async void OnSignInClicked()
    {
        string uName = usernameInput.text;
        string pWord = passwordInput.text;
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(uName, pWord);
            Debug.Log("帳號登入成功！ID: " + AuthenticationService.Instance.PlayerId);
            await TryLoadHomeAsync();
        }
        catch (AuthenticationException ex) { Debug.LogError("登入失敗: " + ex.Message); }
        catch (RequestFailedException ex) { Debug.LogError("請求錯誤: " + ex.Message); }
    }

    // --- 按鈕 3: 訪客登入 (匿名) ---
    // 這是原本舊腳本的功能，我們把它加回來
    public async void OnGuestLoginClicked()
    {
        try
        {
            Debug.Log("嘗試訪客登入...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("訪客登入成功！ID: " + AuthenticationService.Instance.PlayerId);
            await TryLoadHomeAsync();
        }
        catch (AuthenticationException ex) { Debug.LogError("訪客登入失敗: " + ex.Message); }
        catch (RequestFailedException ex) { Debug.LogError("請求錯誤: " + ex.Message); }
    }

    // --- 輔助功能: 登出 ---
    public void OnSignOutClicked()
    {
        AuthenticationService.Instance.SignOut();
        Debug.Log("已登出 (清除目前 Session)");
    }

    private async Task TryLoadHomeAsync()
    {
        if (hasLoadedHome) return;
        if (string.IsNullOrEmpty(homeSceneName))
        {
            Debug.LogWarning("[Login] homeSceneName 未設定，無法跳轉");
            return;
        }

        var saveManager = CloudSaveManager.Instance ?? FindAnyObjectByType<CloudSaveManager>();
        if (saveManager != null)
        {
            try { await saveManager.LoadAsync(); }
            catch (Exception ex) { Debug.LogWarning($"[Login] Cloud load failed, continue to Home. {ex.Message}"); }
        }
        else
        {
            Debug.LogWarning("[Login] No CloudSaveManager found, skip cloud load");
        }

        hasLoadedHome = true;
        SceneManager.LoadScene(homeSceneName);
    }

    public void setvolume(float value)
    {
        bgm.volume = value;
        data.bgmvol = value;
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
}
