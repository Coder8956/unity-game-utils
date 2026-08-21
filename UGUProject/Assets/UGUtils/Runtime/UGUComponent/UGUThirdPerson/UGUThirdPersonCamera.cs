using UnityEngine;

namespace UGU.Runtime
{
    public class UGUThirdPersonCamera : MonoBehaviour
    {
        #region Inspector Properties

        [SerializeField] private string m_rotateCameraXInput = "Mouse X";
        [SerializeField] private string m_rotateCameraYInput = "Mouse Y";

        [SerializeField] private Transform m_target;

        [Tooltip("Lerp speed between Camera States")]
        [SerializeField] private float m_smoothCameraRotation = 12f;

        [Tooltip("What layer will be culled")]
        [SerializeField] private LayerMask m_cullingLayer = 1 << 0;

        [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
        [SerializeField] private bool m_lockCamera;

        [SerializeField] private float m_rightOffset = 0f;

        [SerializeField] private float m_defaultDistance = 5f;
        [SerializeField] private float m_followDistanceMax = 60f;
        [SerializeField] private float m_followDistanceMin = 1f;
        [SerializeField] private float m_mouseScrollSpeed = 10f;

        [SerializeField] private float m_height = 1.4f;
        [SerializeField] private float m_smoothFollow = 10f;
        [SerializeField] private float m_xMouseSensitivity = 3f;
        [SerializeField] private float m_yMouseSensitivity = 3f;
        [SerializeField] private float m_yMinLimit = -40f;
        [SerializeField] private float m_yMaxLimit = 80f;

        [SerializeField] private bool m_checkBlockedCulling = false;

        #endregion

        #region Hidden Properties

        [HideInInspector] public int indexList, indexLookPoint;
        [HideInInspector] public float offSetPlayerPivot;
        [HideInInspector] public string currentStateName;
        [HideInInspector] public Transform currentTarget;
        [HideInInspector] public Vector2 movementSpeed;

        private Transform m_targetLookAt;
        private Vector3 m_currentTargetPos;
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

        #endregion

        public bool IsEnable { get; set; }

        public Transform Target
        {
            get => m_target;
            set => m_target = value;
        }

        private void Awake()
        {
            IsEnable = true;
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            if (m_target == null)
                return;
            m_camera = GetComponent<Camera>();
            currentTarget = m_target;
            m_currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot,
                currentTarget.position.z);

            m_targetLookAt = new GameObject("targetLookAt").transform;
            m_targetLookAt.position = currentTarget.position;
            m_targetLookAt.hideFlags = HideFlags.HideInHierarchy;
            m_targetLookAt.rotation = currentTarget.rotation;

            m_mouseY = currentTarget.eulerAngles.x;
            m_mouseX = currentTarget.eulerAngles.y;

            m_distance = m_defaultDistance;
            m_currentHeight = m_height;
        }

        protected virtual void Update()
        {
            if (!IsEnable) return;
            CameraInput();
        }

        private void LateUpdate()
        {
            if (!IsEnable) return;
            if (m_target == null || m_targetLookAt == null) return;

            CameraMovement();
        }

        protected virtual void CameraInput()
        {
            if (Input.GetMouseButton(1))
            {
                var y = Input.GetAxis(m_rotateCameraYInput);
                var x = Input.GetAxis(m_rotateCameraXInput);

                RotateCamera(x, y);
            }
        }

        /// <summary>
        /// Camera Rotation behaviour
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void RotateCamera(float x, float y)
        {
            // free rotation
            m_mouseX += x * m_xMouseSensitivity;
            m_mouseY -= y * m_yMouseSensitivity;

            movementSpeed.x = x;
            movementSpeed.y = -y;
            if (!m_lockCamera)
            {
                m_mouseY = ClampAngle(m_mouseY, m_yMinLimit, m_yMaxLimit);
                m_mouseX = ClampAngle(m_mouseX, m_xMinLimit, m_xMaxLimit);
            }
            else
            {
                m_mouseY = currentTarget.root.localEulerAngles.x;
                m_mouseX = currentTarget.root.localEulerAngles.y;
            }
        }

        /// <summary>
        /// Camera behaviour
        /// </summary>
        private void CameraMovement()
        {
            if (currentTarget == null)
                return;

            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0f)
            {
                m_defaultDistance -= scroll * m_mouseScrollSpeed;
                m_defaultDistance = Mathf.Clamp(m_defaultDistance, m_followDistanceMin, m_followDistanceMax);
            }

            m_distance = Mathf.Lerp(m_distance, m_defaultDistance, m_smoothFollow * Time.deltaTime);
            m_cullingDistance = Mathf.Lerp(m_cullingDistance, m_distance, Time.deltaTime);
            var camDir = (m_forward * m_targetLookAt.forward) + (m_rightOffset * m_targetLookAt.right);

            camDir = camDir.normalized;

            var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot,
                currentTarget.position.z);
            m_currentTargetPos = targetPos;
            var desiredCPos = targetPos + new Vector3(0, m_height, 0);
            var currentCPos = m_currentTargetPos + new Vector3(0, m_currentHeight, 0);

            if (m_checkBlockedCulling)
            {
                RaycastHit hitInfo;

                UGUClipPlanePoints planePoints =
                    NearUGUClipPlanePoints(m_camera, currentCPos + (camDir * (m_distance)), m_clipPlaneMargin);
                UGUClipPlanePoints oldPoints =
                    NearUGUClipPlanePoints(m_camera, desiredCPos + (camDir * m_distance), m_clipPlaneMargin);

                //Check if Height is not blocked
                if (Physics.SphereCast(targetPos, m_checkHeightRadius, Vector3.up, out hitInfo, m_cullingHeight + 0.2f,
                    m_cullingLayer))
                {
                    var t = hitInfo.distance - 0.2f;
                    t -= m_height;
                    t /= (m_cullingHeight - m_height);
                    m_cullingHeight = Mathf.Lerp(m_height, m_cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
                }

                //Check if desired target position is not blocked
                if (CullingRayCast(desiredCPos, oldPoints, out hitInfo, m_distance + 0.2f, m_cullingLayer, Color.blue))
                {
                    m_distance = hitInfo.distance - 0.2f;
                    if (m_distance < m_defaultDistance)
                    {
                        var t = hitInfo.distance;
                        t -= m_cullingMinDist;
                        t /= m_cullingMinDist;
                        m_currentHeight = Mathf.Lerp(m_cullingHeight, m_height, Mathf.Clamp(t, 0.0f, 1.0f));
                        currentCPos = m_currentTargetPos + new Vector3(0, m_currentHeight, 0);
                    }
                }
                else
                {
                    m_currentHeight = m_height;
                }

                //Check if target position with culling height applied is not blocked
                if (CullingRayCast(currentCPos, planePoints, out hitInfo, m_distance, m_cullingLayer, Color.cyan))
                    m_distance = Mathf.Clamp(m_cullingDistance, 0.0f, m_defaultDistance);
            }

            var calculatedLookPoint = currentCPos + m_targetLookAt.forward * 2f;
            calculatedLookPoint += (m_targetLookAt.right * Vector3.Dot(camDir * (m_distance), m_targetLookAt.right));
            m_targetLookAt.position = currentCPos;

            Quaternion newRot = Quaternion.Euler(m_mouseY, m_mouseX, 0);
            m_targetLookAt.rotation =
                Quaternion.Slerp(m_targetLookAt.rotation, newRot, m_smoothCameraRotation * Time.deltaTime);
            transform.position = currentCPos + (camDir * (m_distance));
            var rotation = Quaternion.LookRotation(calculatedLookPoint - transform.position);

            transform.rotation = rotation;
            movementSpeed = Vector2.zero;
        }

        /// <summary>
        /// Custom Raycast using NearClipPlanesPoints
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="hitInfo"></param>
        /// <param name="distance"></param>
        /// <param name="cullingLayer"></param>
        /// <returns></returns>
        private bool CullingRayCast(Vector3 from, UGUClipPlanePoints to, out RaycastHit hitInfo, float distance,
            LayerMask cullingLayer, Color color)
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

        private static float ClampAngle(float angle, float min, float max)
        {
            do
            {
                if (angle < -360)
                    angle += 360;
                if (angle > 360)
                    angle -= 360;
            } while (angle < -360 || angle > 360);

            return Mathf.Clamp(angle, min, max);
        }

        private static UGUClipPlanePoints NearUGUClipPlanePoints(Camera camera, Vector3 pos, float clipPlaneMargin)
        {
            var clipPlanePoints = new UGUClipPlanePoints();

            var transform = camera.transform;
            var halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
            var aspect = camera.aspect;
            var distance = camera.nearClipPlane;
            var height = distance * Mathf.Tan(halfFOV);
            var width = height * aspect;
            height *= 1 + clipPlaneMargin;
            width *= 1 + clipPlaneMargin;
            clipPlanePoints.LowerRight = pos + transform.right * width;
            clipPlanePoints.LowerRight -= transform.up * height;
            clipPlanePoints.LowerRight += transform.forward * distance;

            clipPlanePoints.LowerLeft = pos - transform.right * width;
            clipPlanePoints.LowerLeft -= transform.up * height;
            clipPlanePoints.LowerLeft += transform.forward * distance;

            clipPlanePoints.UpperRight = pos + transform.right * width;
            clipPlanePoints.UpperRight += transform.up * height;
            clipPlanePoints.UpperRight += transform.forward * distance;

            clipPlanePoints.UpperLeft = pos - transform.right * width;
            clipPlanePoints.UpperLeft += transform.up * height;
            clipPlanePoints.UpperLeft += transform.forward * distance;

            return clipPlanePoints;
        }

        private struct UGUClipPlanePoints
        {
            public Vector3 UpperLeft;
            public Vector3 UpperRight;
            public Vector3 LowerLeft;
            public Vector3 LowerRight;
        }
    }
}
