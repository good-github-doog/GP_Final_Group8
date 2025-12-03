using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    void Reset()
    {
        // 方便在 Inspector 里自动填主摄
        targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        // 让物体正面始终朝向摄像机
        var cam = targetCamera.transform;
        transform.rotation = Quaternion.LookRotation(
            cam.rotation * Vector3.forward,
            cam.rotation * Vector3.up
        );
    }
}
