using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Transform[] spots;          // 放 3 個 spot
    public Button leftBtn;
    public Button rightBtn;
    public float moveSpeed = 5f;

    private int currentIndex = 0;      // 起始在 spot1（index=0）

    void Start()
    {
        SyncStage();
        UpdateButtons();
    }

    void Update()
    {
        // 平滑移動
        transform.position = Vector3.Lerp(
            transform.position,
            spots[currentIndex].position,
            Time.deltaTime * moveSpeed
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            spots[currentIndex].rotation,
            Time.deltaTime * moveSpeed
        );
    }

    public void MoveLeft()   // 從 1->2->3
    {
        if (currentIndex < spots.Length - 1)
        {
            currentIndex++;
            SyncStage();
            UpdateButtons();
        }
    }

    public void MoveRight()  // 從 3->2->1
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            SyncStage();
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        leftBtn.interactable = (currentIndex < spots.Length - 1);
        rightBtn.interactable = (currentIndex > 0);
    }

    private void SyncStage()
    {
        // currentIndex is 0-based, nowstage is 1-based.
        data.nowstage = currentIndex + 1;
    }
}
