using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IrisTransitionCutout : MonoBehaviour
{
    public static IrisTransitionCutout Instance;

    [Header("UI")]
    public Canvas transitionCanvas;
    public Image overlayImage;                  // IrisOverlay 的 Image
    public IrisRaycastFilter raycastFilter;     // IrisOverlay 上的 IrisRaycastFilter

    [Header("Timing")]
    public float duration = 0.6f;

    [Header("Hole")]
    public Vector2 centerUV = new Vector2(0.5f, 0.5f);
    public float openRadiusPixels = 750f;      // 開到看不到黑邊（通常設很大）
    public float closeRadiusPixels = 0f;
    public float softnessUV = 0.02f;

    Material mat;
    bool isTransitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 防呆
        if (transitionCanvas != null)
        {
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.overrideSorting = true;
            transitionCanvas.sortingOrder = 32767;
        }

        // 讓材質變成 runtime instance（避免改到 Project 裡的共享材質）
        mat = Instantiate(overlayImage.material);
        overlayImage.material = mat;

        ApplyHole(openRadiusPixels); // 初始全開
        ForceOnTop();
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode m) => ForceOnTop();

    void ForceOnTop()
    {
        if (transitionCanvas == null) return;
        transitionCanvas.enabled = true;
        transitionCanvas.overrideSorting = true;
        transitionCanvas.sortingOrder = 32767;
        transitionCanvas.transform.SetAsLastSibling();
    }

    // 把「像素半徑」轉成 shader 用的 UV 半徑
    float PixelsToUvRadius(float px)
    {
        float min = Mathf.Min(Screen.width, Screen.height);
        return (min <= 1f) ? 0f : (px / min);
    }

    void ApplyHole(float radiusPx)
    {
        float aspect = (Screen.height <= 1) ? 1f : (Screen.width / (float)Screen.height);

        mat.SetColor("_Color", Color.black);
        mat.SetVector("_Center", new Vector4(centerUV.x, centerUV.y, 0, 0));
        mat.SetFloat("_Aspect", aspect);
        mat.SetFloat("_Softness", softnessUV);
        mat.SetFloat("_Radius", PixelsToUvRadius(radiusPx));

        // 同步 raycast：洞內點擊穿透
        if (raycastFilter != null)
        {
            raycastFilter.centerUV = centerUV;
            raycastFilter.radiusPixels = radiusPx;
        }
    }

    public void LoadSceneWithIris(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(CoLoad(sceneName));
    }

    IEnumerator CoLoad(string sceneName)
    {
        isTransitioning = true;
        ForceOnTop();

        // 先關起來
        yield return Animate(openRadiusPixels, closeRadiusPixels);

        SceneManager.LoadScene(sceneName);

        // 等一幀讓新場景 UI 起來
        yield return null;

        ForceOnTop();

        // ✅ 關鍵：先把洞半徑強制設成最小，避免從中間開始
        ApplyHole(closeRadiusPixels);

        // ✅ 再等一幀，確保這個「最小狀態」真的被畫出來
        yield return null;

        // 再開起來
        yield return Animate(closeRadiusPixels, openRadiusPixels);

        isTransitioning = false;
    }

    IEnumerator Animate(float fromPx, float toPx)
    {
        float t = 0f;

        // ✅ 至少跑 2 幀（你想更穩就 3）
        int minFrames = 2;
        int frames = 0;

        while (t < duration || frames < minFrames)
        {
            t += Time.unscaledDeltaTime;
            frames++;

            float p = (duration <= 0.0001f) ? 1f : Mathf.Clamp01(t / duration);
            p = Mathf.SmoothStep(0, 1, p);

            float r = Mathf.Lerp(fromPx, toPx, p);
            ApplyHole(r);
            yield return null;
        }

        ApplyHole(toPx);
    }

}
