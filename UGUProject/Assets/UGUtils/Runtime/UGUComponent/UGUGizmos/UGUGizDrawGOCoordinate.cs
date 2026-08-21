using UnityEngine;

namespace UGU.Runtime
{
    public class UGUGizDrawGOCoordinate : MonoBehaviour
    {
        [SerializeField] private float m_scaleAxis = 20;

        private void OnDrawGizmos()
        {
            Debug.DrawRay(transform.position, transform.forward * m_scaleAxis, Color.blue);
            Debug.DrawRay(transform.position, transform.up * m_scaleAxis, Color.green);
            Debug.DrawRay(transform.position, transform.right * m_scaleAxis, Color.red);
        }
    }
}