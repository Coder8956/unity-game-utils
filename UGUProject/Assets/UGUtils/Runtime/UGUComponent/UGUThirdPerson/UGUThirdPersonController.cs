using UnityEngine;

namespace UGU.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class UGUThirdPersonController : MonoBehaviour
    {
        [SerializeField] private float m_moveSpeed = 5f; // 移动速度
        [SerializeField] private float m_turnSpeed = 10f; // 转向速度
        [SerializeField] private bool m_isRigRreezeRotation = true;
        [SerializeField] private Transform m_cameraTransform;

        public Transform CameraTransform
        {
            get => m_cameraTransform;
            set => m_cameraTransform = value;
        }

        private Rigidbody m_rb;

        public bool IsEnable { get; set; }

        void Awake()
        {
            // 获取Rigidbody组件
            m_rb = GetComponent<Rigidbody>();
            m_rb.freezeRotation = m_isRigRreezeRotation;
            IsEnable = true;
            // 获取主相机的Transform组件
            // if (cameraTransform == null)
            //     cameraTransform = Camera.main.transform;
        }

        private void StopMove()
        {
            // 停止移动
            m_rb.linearVelocity = new Vector3(0, m_rb.linearVelocity.y, 0);
        }

        void Update()
        {
            if (!IsEnable)
            {
                StopMove();
                return;
            }

            // 获取输入
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // 计算移动方向
            Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

            if (moveDirection.magnitude >= 0.1f)
            {
                // 根据相机的方向调整移动方向
                Vector3 cameraForward = Vector3.Scale(m_cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 move = cameraForward * moveDirection.z + m_cameraTransform.right * moveDirection.x;

                // 使角色朝向移动方向
                if (move != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(move);
                    transform.rotation =
                        Quaternion.Slerp(transform.rotation, targetRotation, m_turnSpeed * Time.deltaTime);
                }

                // 移动角色（使用Rigidbody的速度）
                MoveRigidbody(move);
            }
            else
            {
                // 如果没有输入，停止移动
                StopMove();
            }
        }

        private void MoveRigidbody(Vector3 move)
        {
            // 计算目标速度
            Vector3 targetVelocity = move * m_moveSpeed;

            // 保持当前的垂直速度（重力）
            targetVelocity.y = m_rb.linearVelocity.y;

            // 设置Rigidbody的速度
            m_rb.linearVelocity = targetVelocity;
        }
    }
}