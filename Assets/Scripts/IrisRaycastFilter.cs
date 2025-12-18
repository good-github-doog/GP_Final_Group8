using UnityEngine;
using UnityEngine.UI;

public class IrisRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [HideInInspector] public Vector2 centerUV = new Vector2(0.5f, 0.5f);
    [HideInInspector] public float radiusPixels = 300f; // 由轉場腳本動態更新

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // sp 是螢幕像素座標
        Vector2 centerPx = new Vector2(centerUV.x * Screen.width, centerUV.y * Screen.height);
        float d = Vector2.Distance(sp, centerPx);

        // 洞內：不擋（回傳 false 讓點擊穿透）
        if (d <= radiusPixels) return false;

        // 洞外：要擋（黑幕阻擋互動）
        return true;
    }
}
