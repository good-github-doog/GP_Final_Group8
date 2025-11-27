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
            UpdateButtons();
        }
    }

    public void MoveRight()  // 從 3->2->1
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        leftBtn.interactable = (currentIndex < spots.Length - 1);
        rightBtn.interactable = (currentIndex > 0);
    }
}
