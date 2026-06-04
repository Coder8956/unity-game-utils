using UnityEngine;

namespace UGU.Runtime.UGUComponent.UGUGizmos
{
    public class UGUGizDrawGOPoint : MonoBehaviour
    {
        [SerializeField] private bool m_isDraw = true;
        [SerializeField] private float m_radius = 1.0f; // 球的半径
        [SerializeField] private Color m_gizmoColor = Color.red; // 颜色

        public Color GizmoColor
        {
            get => m_gizmoColor;
            set => m_gizmoColor = value;
        }

        void OnDrawGizmos()
        {
            if (!m_isDraw) return;
            Gizmos.color = m_gizmoColor;
            Gizmos.DrawSphere(transform.position, m_radius);
        }

        /// <summary>
        /// 只有当该 GameObject 处于选中状态时，Gizmos 才会被绘制。
        /// </summary>
        // void OnDrawGizmosSelected()
        // {
        //     Gizmos.color = m_gizmoColor;
        //     Gizmos.DrawSphere(transform.position, m_radius);
        // }
    }
}