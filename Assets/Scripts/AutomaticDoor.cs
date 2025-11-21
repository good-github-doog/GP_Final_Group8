using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Seagull.Interior_I1.SceneProps;

/// <summary>
/// 自動門控制器 - 當顧客接近時自動開門，離開後自動關門
/// </summary>
public class AutomaticDoor : MonoBehaviour
{
    [Header("門設置")]
    [Tooltip("門上的 RotatableObject 組件")]
    public RotatableObject rotatableObject;

    [Header("方向判斷設置")]
    [Tooltip("外側參考點（例如：餐廳外）- 從這邊來的顧客會讓門向外側打開")]
    public Transform outerSidePoint;

    [Tooltip("內側參考點（例如：餐廳內）- 從這邊來的顧客會讓門向內側打開")]
    public Transform innerSidePoint;

    [Header("開關門設置")]
    [Tooltip("門打開的速度")]
    public float openSpeed = 2f;

    [Tooltip("門關閉的延遲時間（秒）")]
    public float closeDelay = 1f;

    [Tooltip("門打開的方向：1 = 正向打開，-1 = 反向打開，0 = 自動判斷（需要設置參考點）")]
    [Range(-1f, 1f)]
    public float openDirection = 0f;

    [Header("觸發器設置")]
    [Tooltip("觸發開門的標籤（通常是 'Customer' 或 'Player'）")]
    private List<string> triggerTags = new List<string> { "Customer" };

    // 追蹤有多少個物體在觸發區域內
    private int objectsInTrigger = 0;

    // 目標旋轉值（0 = 關閉，1 = 打開）
    private float targetRotation = 0f;

    // 當前旋轉值
    private float currentRotation = 0f;

    // 關門協程
    private Coroutine closeCoroutine;

    private void Start()
    {
        // 如果沒有指定 RotatableObject，嘗試在同一個物體上找
        if (rotatableObject == null)
        {
            rotatableObject = GetComponent<RotatableObject>();
            if (rotatableObject != null)
            {
                Debug.Log("[AutomaticDoor] 自動找到 RotatableObject 組件");
            }
        }

        if (rotatableObject == null)
        {
            Debug.LogError("[AutomaticDoor] 找不到 RotatableObject 組件！請在 Inspector 中指定。");
        }
        else
        {
            Debug.Log("[AutomaticDoor] RotatableObject 設置成功");
        }

        // 檢查 Collider 設置
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("[AutomaticDoor] 沒有 Collider！請添加 Box Collider。");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError("[AutomaticDoor] Collider 沒有設為 Trigger！請勾選 'Is Trigger'。");
        }
        else
        {
            Debug.Log("[AutomaticDoor] Collider 設置正確（Is Trigger = true）");
        }

        // 確保門初始是關閉的
        currentRotation = 0f;
        if (rotatableObject != null)
        {
            rotatableObject.rotate(currentRotation);
            Debug.Log("[AutomaticDoor] 門已初始化為關閉狀態");
        }

        Debug.Log($"[AutomaticDoor] 觸發標籤列表：{string.Join(", ", triggerTags)}");
    }

    private void Update()
    {
        if (rotatableObject == null) return;

        // 平滑地移動到目標旋轉值
        if (Mathf.Abs(currentRotation - targetRotation) > 0.01f)
        {
            currentRotation = Mathf.MoveTowards(currentRotation, targetRotation, openSpeed * Time.deltaTime);
            rotatableObject.rotate(currentRotation);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[AutomaticDoor] 偵測到物體進入：{other.name}，Tag: {other.tag}");

        // 檢查是否是觸發開門的物體
        if (ShouldTriggerDoor(other.gameObject))
        {
            objectsInTrigger++;
            Debug.Log($"[AutomaticDoor] ✓ {other.name} 進入觸發區域，當前數量：{objectsInTrigger}");

            // 有物體進入，打開門（傳入觸發物體的 Transform 來判斷方向）
            OpenDoor(other.transform);
        }
        else
        {
            Debug.Log($"[AutomaticDoor] ✗ {other.name} 的 Tag ({other.tag}) 不在觸發列表中");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 檢查是否是觸發開門的物體
        if (ShouldTriggerDoor(other.gameObject))
        {
            objectsInTrigger--;
            Debug.Log($"[AutomaticDoor] {other.name} 離開觸發區域，當前數量：{objectsInTrigger}");

            // 如果沒有物體在觸發區域內，延遲關門
            if (objectsInTrigger <= 0)
            {
                objectsInTrigger = 0; // 確保不會是負數
                CloseDoorDelayed();
            }
        }
    }

    /// <summary>
    /// 檢查物體是否應該觸發門
    /// </summary>
    private bool ShouldTriggerDoor(GameObject obj)
    {
        foreach (string tag in triggerTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 打開門
    /// </summary>
    /// <param name="triggerObject">觸發開門的物體（用於判斷方向）</param>
    public void OpenDoor(Transform triggerObject = null)
    {
        // 取消關門協程（如果有的話）
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        // 決定門打開的方向
        float direction = openDirection;

        // 如果 openDirection 設為 0，自動判斷方向
        if (Mathf.Approximately(openDirection, 0f) && triggerObject != null)
        {
            // 檢查是否有設置參考點
            if (outerSidePoint != null && innerSidePoint != null)
            {
                // 計算顧客到兩個參考點的距離
                float distanceToOuter = Vector3.Distance(triggerObject.position, outerSidePoint.position);
                float distanceToInner = Vector3.Distance(triggerObject.position, innerSidePoint.position);

                // 顧客離哪個點更近，就表示從那邊來
                // 從外側來 → 門向外側打開（-1）
                // 從內側來 → 門向內側打開（+1）
                direction = distanceToOuter < distanceToInner ? -1f : 1f;

                Debug.Log($"[AutomaticDoor] 自動判斷方向：到外側距離 = {distanceToOuter:F2}，到內側距離 = {distanceToInner:F2}，開門方向 = {(direction > 0 ? "內側" : "外側")}");
            }
            else
            {
                Debug.LogWarning("[AutomaticDoor] 未設置參考點！請在 Inspector 中設置 Outer Side Point 和 Inner Side Point。使用預設方向。");
                direction = 1f;
            }
        }
        else if (openDirection != 0)
        {
            direction = Mathf.Sign(openDirection);
            Debug.Log($"[AutomaticDoor] 使用固定方向：{(direction > 0 ? "正向" : "反向")}");
        }

        targetRotation = direction; // 設定目標旋轉值
        Debug.Log($"[AutomaticDoor] 門正在向 {(targetRotation > 0 ? "正向" : "反向")} 打開...");
    }

    /// <summary>
    /// 延遲關門
    /// </summary>
    public void CloseDoorDelayed()
    {
        // 取消之前的關門協程
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }

        closeCoroutine = StartCoroutine(CloseDoorCoroutine());
    }

    /// <summary>
    /// 關門協程
    /// </summary>
    private IEnumerator CloseDoorCoroutine()
    {
        // 等待延遲時間
        yield return new WaitForSeconds(closeDelay);

        // 再次檢查是否有物體在觸發區域內
        if (objectsInTrigger <= 0)
        {
            targetRotation = 0f; // 完全關閉
            Debug.Log("[AutomaticDoor] 門正在關閉...");
        }

        closeCoroutine = null;
    }

    /// <summary>
    /// 立即關門（無延遲）
    /// </summary>
    public void CloseDoorImmediate()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        targetRotation = 0f;
        Debug.Log("[AutomaticDoor] 門立即關閉");
    }

    // 在 Editor 中顯示觸發區域和參考點（方便調試）
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            // 繪製觸發區域
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider boxCol)
            {
                Gizmos.DrawCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        // 繪製外側參考點（紅色球體）
        if (outerSidePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(outerSidePoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, outerSidePoint.position);

            // 顯示標籤
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(outerSidePoint.position + Vector3.up * 0.5f, "外側 (Outer)");
            #endif
        }

        // 繪製內側參考點（藍色球體）
        if (innerSidePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(innerSidePoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, innerSidePoint.position);

            // 顯示標籤
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(innerSidePoint.position + Vector3.up * 0.5f, "內側 (Inner)");
            #endif
        }

        // 如果兩個點都設置了，繪製連接線
        if (outerSidePoint != null && innerSidePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(outerSidePoint.position, innerSidePoint.position);
        }
    }
}
