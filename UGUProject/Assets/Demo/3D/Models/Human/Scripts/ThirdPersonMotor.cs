using UnityEngine;

namespace Invector.CharacterController
{
    public class ThirdPersonMotor : MonoBehaviour
    {
        // ── Inspector 配置 ────────────────────────────────────────

        [Header("Movement")]

        [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
        [SerializeField] protected bool m_useRootMotion = false;

        [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
        [SerializeField] protected bool m_rotateByWorld = false;

        [Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
        [SerializeField] protected bool m_useContinuousSprint = true;

        [Tooltip("Check this to sprint always in free movement")]
        [SerializeField] protected bool m_sprintOnlyFree = true;

        [SerializeField] protected LocomotionType m_locomotionType = LocomotionType.FreeWithStrafe;

        [SerializeField] protected MovementSpeed m_freeSpeed;
        [SerializeField] protected MovementSpeed m_strafeSpeed;

        [Header("Airborne")]

        [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
        [SerializeField] private bool m_jumpWithRigidbodyForce = false;

        [Tooltip("Rotate or not while airborne")]
        [SerializeField] private bool m_jumpAndRotate = true;

        [Tooltip("How much time the character will be jumping")]
        [SerializeField] protected float m_jumpTimer = 0.3f;

        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        [SerializeField] private float m_jumpHeight = 4f;

        [Tooltip("Speed that the character will move while airborne")]
        [SerializeField] private float m_airSpeed = 5f;

        [Tooltip("Smoothness of the direction while airborne")]
        [SerializeField] private float m_airSmooth = 6f;

        [Tooltip("Apply extra gravity when the character is not grounded")]
        [SerializeField] private float m_extraGravity = -10f;

        [Header("Ground")]

        [Tooltip("Layers that the character can walk on")]
        [SerializeField] private LayerMask m_groundLayer = 1 << 0;

        [Tooltip("Distance to became not grounded")]
        [SerializeField] private float m_groundMinDistance = 0.25f;

        [SerializeField] private float m_groundMaxDistance = 0.5f;

        [Tooltip("Max angle to walk")]
        [Range(30, 80)]
        [SerializeField] private float m_slopeLimit = 75f;

        // ── 运行时状态 ──────────────────────────────────────────

        [HideInInspector] public Vector3 input;
        [HideInInspector] public Transform rotateTarget;

        protected Animator m_animator;
        private Rigidbody m_rigidbody;
        private PhysicsMaterial m_frictionPhysics, m_maxFrictionPhysics, m_slippyPhysics;
        private CapsuleCollider m_capsuleCollider;

        protected float m_inputMagnitude;
        protected float m_verticalSpeed;
        protected float m_horizontalSpeed;
        private float m_moveSpeed;
        private float m_verticalVelocity;
        private float m_colliderRadius, m_colliderHeight;
        private Vector3 m_colliderCenter;
        private float m_heightReached;
        protected float m_jumpCounter;
        protected float m_groundDistance;
        private RaycastHit m_groundHit;
        protected bool m_lockMovement = false;
        protected bool m_lockRotation = false;
        protected Vector3 m_inputSmooth;
        protected Vector3 m_moveDirection;

        // ── 属性 ─────────────────────────────────────────────────

        public bool IsStrafing { get; set; }
        public bool IsGrounded { get; protected set; }
        public bool IsSprinting { get; set; }
        public bool IsJumping { get; protected set; }
        public bool StopMove { get; protected set; }
        public float SlopeLimit => m_slopeLimit;

        // ── 生命周期 ─────────────────────────────────────────────

        public void Init()
        {
            m_animator = GetComponent<Animator>();
            m_animator.updateMode = AnimatorUpdateMode.Fixed;

            m_frictionPhysics = new PhysicsMaterial
            {
                name = "frictionPhysics",
                staticFriction = .25f,
                dynamicFriction = .25f,
                frictionCombine = PhysicsMaterialCombine.Multiply
            };

            m_maxFrictionPhysics = new PhysicsMaterial
            {
                name = "maxFrictionPhysics",
                staticFriction = 1f,
                dynamicFriction = 1f,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };

            m_slippyPhysics = new PhysicsMaterial
            {
                name = "slippyPhysics",
                staticFriction = 0f,
                dynamicFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };

            m_rigidbody = GetComponent<Rigidbody>();
            m_capsuleCollider = GetComponent<CapsuleCollider>();

            m_colliderCenter = m_capsuleCollider.center;
            m_colliderRadius = m_capsuleCollider.radius;
            m_colliderHeight = m_capsuleCollider.height;

            IsGrounded = true;
        }

        public virtual void UpdateMotor()
        {
            CheckGround();
            CheckSlopeLimit();
            ControlJumpBehaviour();
            AirControl();
        }

        // ── 移动逻辑 ─────────────────────────────────────────────

        public virtual void SetControllerMoveSpeed(MovementSpeed speed)
        {
            if (speed.walkByDefault)
                m_moveSpeed = Mathf.Lerp(m_moveSpeed, IsSprinting ? speed.runningSpeed : speed.walkSpeed, speed.movementSmooth * Time.deltaTime);
            else
                m_moveSpeed = Mathf.Lerp(m_moveSpeed, IsSprinting ? speed.sprintSpeed : speed.runningSpeed, speed.movementSmooth * Time.deltaTime);
        }

        public virtual void MoveCharacter(Vector3 direction)
        {
            m_inputSmooth = Vector3.Lerp(m_inputSmooth, input, (IsStrafing ? m_strafeSpeed.movementSmooth : m_freeSpeed.movementSmooth) * Time.deltaTime);

            if (!IsGrounded || IsJumping) return;

            direction.y = 0;
            direction.x = Mathf.Clamp(direction.x, -1f, 1f);
            direction.z = Mathf.Clamp(direction.z, -1f, 1f);
            if (direction.magnitude > 1f)
                direction.Normalize();

            Vector3 targetPosition = (m_useRootMotion ? m_animator.rootPosition : m_rigidbody.position) + direction * (StopMove ? 0 : m_moveSpeed) * Time.deltaTime;
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

            targetVelocity.y = m_rigidbody.linearVelocity.y;
            m_rigidbody.linearVelocity = targetVelocity;
        }

        public virtual void CheckSlopeLimit()
        {
            if (input.sqrMagnitude < 0.1) return;

            RaycastHit hitinfo;
            var hitAngle = 0f;

            if (Physics.Linecast(transform.position + Vector3.up * (m_capsuleCollider.height * 0.5f), transform.position + m_moveDirection.normalized * (m_capsuleCollider.radius + 0.2f), out hitinfo, m_groundLayer))
            {
                hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                var targetPoint = hitinfo.point + m_moveDirection.normalized * m_capsuleCollider.radius;
                if ((hitAngle > m_slopeLimit) && Physics.Linecast(transform.position + Vector3.up * (m_capsuleCollider.height * 0.5f), targetPoint, out hitinfo, m_groundLayer))
                {
                    hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                    if (hitAngle > m_slopeLimit && hitAngle < 85f)
                    {
                        StopMove = true;
                        return;
                    }
                }
            }
            StopMove = false;
        }

        public virtual void RotateToPosition(Vector3 position)
        {
            Vector3 desiredDirection = position - transform.position;
            RotateToDirection(desiredDirection.normalized);
        }

        public virtual void RotateToDirection(Vector3 direction)
        {
            RotateToDirection(direction, IsStrafing ? m_strafeSpeed.rotationSpeed : m_freeSpeed.rotationSpeed);
        }

        public virtual void RotateToDirection(Vector3 direction, float rotationSpeed)
        {
            if (!m_jumpAndRotate && !IsGrounded) return;
            direction.y = 0f;
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction.normalized, rotationSpeed * Time.deltaTime, .1f);
            Quaternion newRotation = Quaternion.LookRotation(desiredForward);
            transform.rotation = newRotation;
        }

        // ── 跳跃 ─────────────────────────────────────────────────

        protected virtual void ControlJumpBehaviour()
        {
            if (!IsJumping) return;

            m_jumpCounter -= Time.deltaTime;
            if (m_jumpCounter <= 0)
            {
                m_jumpCounter = 0;
                IsJumping = false;
            }
            var vel = m_rigidbody.linearVelocity;
            vel.y = m_jumpHeight;
            m_rigidbody.linearVelocity = vel;
        }

        public virtual void AirControl()
        {
            if ((IsGrounded && !IsJumping)) return;
            if (transform.position.y > m_heightReached) m_heightReached = transform.position.y;
            m_inputSmooth = Vector3.Lerp(m_inputSmooth, input, m_airSmooth * Time.deltaTime);

            if (m_jumpWithRigidbodyForce && !IsGrounded)
            {
                m_rigidbody.AddForce(m_moveDirection * m_airSpeed * Time.deltaTime, ForceMode.VelocityChange);
                return;
            }

            m_moveDirection.y = 0;
            m_moveDirection.x = Mathf.Clamp(m_moveDirection.x, -1f, 1f);
            m_moveDirection.z = Mathf.Clamp(m_moveDirection.z, -1f, 1f);

            Vector3 targetPosition = m_rigidbody.position + (m_moveDirection * m_airSpeed) * Time.deltaTime;
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

            targetVelocity.y = m_rigidbody.linearVelocity.y;
            m_rigidbody.linearVelocity = Vector3.Lerp(m_rigidbody.linearVelocity, targetVelocity, m_airSmooth * Time.deltaTime);
        }

        protected virtual bool JumpFwdCondition
        {
            get
            {
                Vector3 p1 = transform.position + m_capsuleCollider.center + Vector3.up * -m_capsuleCollider.height * 0.5F;
                Vector3 p2 = p1 + Vector3.up * m_capsuleCollider.height;
                return Physics.CapsuleCastAll(p1, p2, m_capsuleCollider.radius * 0.5f, transform.forward, 0.6f, m_groundLayer).Length == 0;
            }
        }

        // ── 地面检测 ─────────────────────────────────────────────

        protected virtual void CheckGround()
        {
            CheckGroundDistance();
            ControlMaterialPhysics();

            if (m_groundDistance <= m_groundMinDistance)
            {
                IsGrounded = true;
                if (!IsJumping && m_groundDistance > 0.05f)
                    m_rigidbody.AddForce(transform.up * (m_extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);

                m_heightReached = transform.position.y;
            }
            else
            {
                if (m_groundDistance >= m_groundMaxDistance)
                {
                    IsGrounded = false;
                    m_verticalVelocity = m_rigidbody.linearVelocity.y;
                    if (!IsJumping)
                    {
                        m_rigidbody.AddForce(transform.up * m_extraGravity * Time.deltaTime, ForceMode.VelocityChange);
                    }
                }
                else if (!IsJumping)
                {
                    m_rigidbody.AddForce(transform.up * (m_extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
                }
            }
        }

        protected virtual void ControlMaterialPhysics()
        {
            m_capsuleCollider.material = (IsGrounded && GroundAngle() <= m_slopeLimit + 1) ? m_frictionPhysics : m_slippyPhysics;

            if (IsGrounded && input == Vector3.zero)
                m_capsuleCollider.material = m_maxFrictionPhysics;
            else if (IsGrounded && input != Vector3.zero)
                m_capsuleCollider.material = m_frictionPhysics;
            else
                m_capsuleCollider.material = m_slippyPhysics;
        }

        protected virtual void CheckGroundDistance()
        {
            if (m_capsuleCollider != null)
            {
                float radius = m_capsuleCollider.radius * 0.9f;
                var dist = 10f;
                Ray ray2 = new Ray(transform.position + new Vector3(0, m_colliderHeight / 2, 0), Vector3.down);
                if (Physics.Raycast(ray2, out m_groundHit, (m_colliderHeight / 2) + dist, m_groundLayer) && !m_groundHit.collider.isTrigger)
                    dist = transform.position.y - m_groundHit.point.y;
                if (dist >= m_groundMinDistance)
                {
                    Vector3 pos = transform.position + Vector3.up * (m_capsuleCollider.radius);
                    Ray ray = new Ray(pos, -Vector3.up);
                    if (Physics.SphereCast(ray, radius, out m_groundHit, m_capsuleCollider.radius + m_groundMaxDistance, m_groundLayer) && !m_groundHit.collider.isTrigger)
                    {
                        Physics.Linecast(m_groundHit.point + (Vector3.up * 0.1f), m_groundHit.point + Vector3.down * 0.15f, out m_groundHit, m_groundLayer);
                        float newDist = transform.position.y - m_groundHit.point.y;
                        if (dist > newDist) dist = newDist;
                    }
                }
                m_groundDistance = (float)System.Math.Round(dist, 2);
            }
        }

        public virtual float GroundAngle()
        {
            var groundAngle = Vector3.Angle(m_groundHit.normal, Vector3.up);
            return groundAngle;
        }

        public virtual float GroundAngleFromDirection()
        {
            var dir = IsStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.z).normalized : transform.forward;
            var movementAngle = Vector3.Angle(dir, m_groundHit.normal) - 90;
            return movementAngle;
        }

        // ── 序列化配置类 ─────────────────────────────────────────

        public enum LocomotionType
        {
            FreeWithStrafe,
            OnlyStrafe,
            OnlyFree,
        }

        [System.Serializable]
        public class MovementSpeed
        {
            [Range(1f, 20f)]
            public float movementSmooth = 6f;
            [Range(0f, 1f)]
            public float animationSmooth = 0.2f;
            [Tooltip("Rotation speed of the character")]
            public float rotationSpeed = 16f;
            [Tooltip("Character will limit the movement to walk instead of running")]
            public bool walkByDefault = false;
            [Tooltip("Rotate with the Camera forward when standing idle")]
            public bool rotateWithCamera = false;
            [Tooltip("Speed to Walk using rigidbody or extra speed if you're using RootMotion")]
            public float walkSpeed = 2f;
            [Tooltip("Speed to Run using rigidbody or extra speed if you're using RootMotion")]
            public float runningSpeed = 4f;
            [Tooltip("Speed to Sprint using rigidbody or extra speed if you're using RootMotion")]
            public float sprintSpeed = 6f;
        }
    }
}
