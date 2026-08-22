using Invector;
using UnityEngine;

namespace Invector.CharacterController
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        // ── Inspector 配置 ────────────────────────────────────────

        [Tooltip("Target transform for the camera to follow")]
        [SerializeField] private Transform m_target;

        [Tooltip("Lerp speed between Camera States")]
        [SerializeField] private float m_smoothCameraRotation = 12f;

        [Tooltip("What layer will be culled")]
        [SerializeField] private LayerMask m_cullingLayer = 1 << 0;

        [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
        [SerializeField] private bool m_lockCamera;

        [SerializeField] private float m_rightOffset = 0f;
        [SerializeField] private float m_defaultDistance = 2.5f;
        [SerializeField] private float m_height = 1.4f;
        [SerializeField] private float m_smoothFollow = 10f;
        [SerializeField] private float m_xMouseSensitivity = 3f;
        [SerializeField] private float m_yMouseSensitivity = 3f;
        [SerializeField] private float m_yMinLimit = -40f;
        [SerializeField] private float m_yMaxLimit = 80f;

        // ── 隐藏字段 ─────────────────────────────────────────────

        [HideInInspector] public int indexList, indexLookPoint;
        [HideInInspector] public float offSetPlayerPivot;
        [HideInInspector] public string currentStateName;
        [HideInInspector] public Transform currentTarget;
        [HideInInspector] public Vector2 movementSpeed;

        // ── 运行时状态 ──────────────────────────────────────────

        private Transform m_targetLookAt;
        private Vector3 m_currentTargetPos;
        private Vector3 m_currentCPos;
        private Vector3 m_desiredCPos;
        private Camera m_camera;
        private float m_distance = 5f;
        private float m_mouseY = 0f;
        private float m_mouseX = 0f;
        private float m_currentHeight;
        private float m_cullingDistance;
        private float m_checkHeightRadius = 0.4f;
        private float m_clipPlaneMargin = 0f;
        private float m_forward = -1f;
        private float m_xMinLimit = -360f;
        private float m_xMaxLimit = 360f;
        private float m_cullingHeight = 0.2f;
        private float m_cullingMinDist = 0.1f;

        // ── 生命周期 ─────────────────────────────────────────────

        private void Start()
        {
            Init();
        }

        private void FixedUpdate()
        {
            if (m_target == null || m_targetLookAt == null) return;

            CameraMovement();
        }

        // ── 公共接口 ──────────────────────────────────────────────

        public void Init()
        {
            if (m_target == null)
                return;

            m_camera = GetComponent<Camera>();
            currentTarget = m_target;
            m_currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);

            m_targetLookAt = new GameObject("targetLookAt").transform;
            m_targetLookAt.position = currentTarget.position;
            m_targetLookAt.hideFlags = HideFlags.HideInHierarchy;
            m_targetLookAt.rotation = currentTarget.rotation;

            m_mouseY = currentTarget.eulerAngles.x;
            m_mouseX = currentTarget.eulerAngles.y;

            m_distance = m_defaultDistance;
            m_currentHeight = m_height;
        }

        /// <summary>
        /// Set the target for the camera
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            currentTarget = newTarget ? newTarget : m_target;
        }

        public void SetMainTarget(Transform newTarget)
        {
            m_target = newTarget;
            currentTarget = newTarget;
            m_mouseY = currentTarget.rotation.eulerAngles.x;
            m_mouseX = currentTarget.rotation.eulerAngles.y;
            Init();
        }

        /// <summary>
        /// Convert a point in the screen in a Ray for the world
        /// </summary>
        public Ray ScreenPointToRay(Vector3 point)
        {
            return m_camera.ScreenPointToRay(point);
        }

        /// <summary>
        /// Camera Rotation behaviour
        /// </summary>
        public void RotateCamera(float x, float y)
        {
            m_mouseX += x * m_xMouseSensitivity;
            m_mouseY -= y * m_yMouseSensitivity;

            movementSpeed.x = x;
            movementSpeed.y = -y;
            if (!m_lockCamera)
            {
                m_mouseY = Extensions.ClampAngle(m_mouseY, m_yMinLimit, m_yMaxLimit);
                m_mouseX = Extensions.ClampAngle(m_mouseX, m_xMinLimit, m_xMaxLimit);
            }
            else
            {
                m_mouseY = currentTarget.root.localEulerAngles.x;
                m_mouseX = currentTarget.root.localEulerAngles.y;
            }
        }

        // ── 相机逻辑 ─────────────────────────────────────────────

        private void CameraMovement()
        {
            if (currentTarget == null)
                return;

            m_distance = Mathf.Lerp(m_distance, m_defaultDistance, m_smoothFollow * Time.deltaTime);
            m_cullingDistance = Mathf.Lerp(m_cullingDistance, m_distance, Time.deltaTime);
            var camDir = (m_forward * m_targetLookAt.forward) + (m_rightOffset * m_targetLookAt.right);

            camDir = camDir.normalized;

            var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
            m_currentTargetPos = targetPos;
            m_desiredCPos = targetPos + new Vector3(0, m_height, 0);
            m_currentCPos = m_currentTargetPos + new Vector3(0, m_currentHeight, 0);
            RaycastHit hitInfo;

            ClipPlanePoints planePoints = m_camera.NearClipPlanePoints(m_currentCPos + (camDir * (m_distance)), m_clipPlaneMargin);
            ClipPlanePoints oldPoints = m_camera.NearClipPlanePoints(m_desiredCPos + (camDir * m_distance), m_clipPlaneMargin);

            if (Physics.SphereCast(targetPos, m_checkHeightRadius, Vector3.up, out hitInfo, m_cullingHeight + 0.2f, m_cullingLayer))
            {
                var t = hitInfo.distance - 0.2f;
                t -= m_height;
                t /= (m_cullingHeight - m_height);
                m_cullingHeight = Mathf.Lerp(m_height, m_cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
            }

            if (CullingRayCast(m_desiredCPos, oldPoints, out hitInfo, m_distance + 0.2f, m_cullingLayer, Color.blue))
            {
                m_distance = hitInfo.distance - 0.2f;
                if (m_distance < m_defaultDistance)
                {
                    var t = hitInfo.distance;
                    t -= m_cullingMinDist;
                    t /= m_cullingMinDist;
                    m_currentHeight = Mathf.Lerp(m_cullingHeight, m_height, Mathf.Clamp(t, 0.0f, 1.0f));
                    m_currentCPos = m_currentTargetPos + new Vector3(0, m_currentHeight, 0);
                }
            }
            else
            {
                m_currentHeight = m_height;
            }
            if (CullingRayCast(m_currentCPos, planePoints, out hitInfo, m_distance, m_cullingLayer, Color.cyan)) m_distance = Mathf.Clamp(m_cullingDistance, 0.0f, m_defaultDistance);
            var lookPoint = m_currentCPos + m_targetLookAt.forward * 2f;
            lookPoint += (m_targetLookAt.right * Vector3.Dot(camDir * (m_distance), m_targetLookAt.right));
            m_targetLookAt.position = m_currentCPos;

            Quaternion newRot = Quaternion.Euler(m_mouseY, m_mouseX, 0);
            m_targetLookAt.rotation = Quaternion.Slerp(m_targetLookAt.rotation, newRot, m_smoothCameraRotation * Time.deltaTime);
            transform.position = m_currentCPos + (camDir * (m_distance));
            var rotation = Quaternion.LookRotation((lookPoint) - transform.position);

            transform.rotation = rotation;
            movementSpeed = Vector2.zero;
        }

        private bool CullingRayCast(Vector3 from, ClipPlanePoints to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
        {
            bool value = false;

            if (Physics.Raycast(from, to.LowerLeft - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                m_cullingDistance = hitInfo.distance;
            }

            if (Physics.Raycast(from, to.LowerRight - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
            }

            if (Physics.Raycast(from, to.UpperLeft - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
            }

            if (Physics.Raycast(from, to.UpperRight - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
            }

            return hitInfo.collider && value;
        }
    }
}
