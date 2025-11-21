
# if UNITY_EDITOR
using UnityEditor;
# endif
using Seagull.Interior_I1.Inspector;
using UnityEngine;

namespace Seagull.Interior_I1.SceneProps
{
    public enum Axis
    {
        x, y, z
    }

    public class Rotatable : MonoBehaviour
    {
        [SerializeField] private float startAngle;
        [SerializeField] private float endAngle;
        [SerializeField] private Axis rotationAxis;

        [Range(-1f, 1f)] public float rotation;

        // Start is called before the first frame update
        private void Start()
        {
            updateAngle();
        }

        private float lastRotation = -1;
        private void FixedUpdate()
        {
            if (lastRotation == -1)
            {
                lastRotation = rotation;
                return;
            }

            if (lastRotation == rotation) return;
            updateAngle();
            lastRotation = rotation;
        }

        private void OnValidate()
        {
            updateAngle();
            lastRotation = rotation;
        }

        private void updateAngle()
        {
            // 將旋轉值限制在 -1 到 1 之間
            rotation = Mathf.Clamp(rotation, -1f, 1f);

            // startAngle = 關閉狀態的角度（通常是 0）
            // endAngle = 旋轉幅度（例如 90 表示可以旋轉 ±90°）
            // rotation = -1 → startAngle - endAngle（反向旋轉，例如 -90°）
            // rotation = 0  → startAngle（關閉狀態，例如 0°）
            // rotation = 1  → startAngle + endAngle（正向旋轉，例如 +90°）
            float angle = startAngle + rotation * endAngle;

            Vector3 axis = rotationAxis switch
            {
                Axis.x => Vector3.right,
                Axis.y => Vector3.up,
                Axis.z => Vector3.forward,
                _ => Vector3.zero
            };
            transform.localRotation = Quaternion.AngleAxis(angle, axis);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Rotatable))]
    public class RotatableEditor : AnInspector { }
#endif
}