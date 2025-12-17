using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IrisTransition : MonoBehaviour
{
    public static IrisTransition Instance;

    [Header("UI References")]
    public Canvas transitionCanvas;     // 放這個 Canvas
    public RectTransform hole;          // 中間那個洞(用來縮放)

    [Header("Timing")]
    public float duration = 0.6f;

    bool isTransitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
        hole.localScale = Vector3.one * 15f;

        // 一開始先確保在最上層
        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 9999;
        }
    }

    public void LoadSceneWithIris(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(CoLoadSceneWithIris(sceneName));
    }

    IEnumerator CoLoadSceneWithIris(string sceneName)
    {
        isTransitioning = true;

        // 確保遮罩顯示
        if (transitionCanvas != null) transitionCanvas.enabled = true;

        yield return StartCoroutine(CloseIris());

        SceneManager.LoadScene(sceneName);

        // 等一幀讓新場景起來
        yield return null;

        yield return StartCoroutine(OpenIris());

        // 轉場結束後你也可以選擇關掉 Canvas（可留著也行）
        // if (transitionCanvas != null) transitionCanvas.enabled = false;

        isTransitioning = false;
    }

    public IEnumerator CloseIris()
    {
        float t = 0;
        Vector3 startScale = Vector3.one * 15f;
        Vector3 endScale = Vector3.zero;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0, 1, t / duration);
            hole.localScale = Vector3.Lerp(startScale, endScale, p);
            yield return null;
        }
        hole.localScale = endScale;
    }

    public IEnumerator OpenIris()
    {
        float t = 0;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 15f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0, 1, t / duration);
            hole.localScale = Vector3.Lerp(startScale, endScale, p);
            yield return null;
        }
        hole.localScale = endScale;
    }
}
