using UnityEditor;
using UnityEngine;

// 需要这个命名空间
namespace UGU.Runtime
{
    public class UGUGizDrawNum : MonoBehaviour
    {
        [SerializeField] private float m_pointRadius = 0.5f;
        [SerializeField] private int m_number = 0;

        public int Number
        {
            get => m_number;
            set => m_number = value;
        }

        [SerializeField] private int m_fontSize = 20;

        [SerializeField] private Color m_gizmoColor = Color.red;

        public Color GizmoColor => m_gizmoColor;

        // [SerializeField] private Color m_gizmoNumColor = Color.red;
        [SerializeField] private Vector3 m_offset = new(0.5f, 0, 0);

        void OnDrawGizmos()
        {
            Gizmos.color = m_gizmoColor;
            Gizmos.DrawSphere(transform.position, m_pointRadius);
#if UNITY_EDITOR
            // 绘制数字
            Handles.Label(transform.position + m_offset, m_number.ToString(),
                new GUIStyle()
                {
                    normal = new GUIStyleState() {textColor = m_gizmoColor},
                    fontSize = m_fontSize,
                    fontStyle = FontStyle.Bold
                });
#endif
        }
    }
}