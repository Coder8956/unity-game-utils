using UnityEngine;
using UnityEngine.InputSystem;

namespace Invector.CharacterController
{
    public class ThirdPersonInput : MonoBehaviour
    {
        // ── Inspector 配置 ────────────────────────────────────────

        [Header("Camera Input")]
        [Tooltip("鼠标移动缩放系数，用于将原始像素增量转换为旋转量")]
        [SerializeField] private float m_mouseDeltaScale = 0.1f;

        [HideInInspector] public ThirdPersonController cc;
        [HideInInspector] public ThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        // ── 生命周期 ─────────────────────────────────────────────

        protected virtual void Start()
        {
            InitilizeController();
            InitializeTpCamera();
        }

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();
            cc.ControlLocomotionType();
            cc.ControlRotationType();
        }

        protected virtual void Update()
        {
            InputHandle();
            cc.UpdateAnimator();
        }

        public virtual void OnAnimatorMove()
        {
            cc.ControlAnimatorRootMotion();
        }

        // ── 输入处理 ──────────────────────────────────────────────

        protected virtual void InitilizeController()
        {
            cc = GetComponent<ThirdPersonController>();

            if (cc != null)
                cc.Init();
        }

        protected virtual void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = FindFirstObjectByType<ThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        protected virtual void InputHandle()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();
        }

        public virtual void MoveInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            cc.input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                       - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            cc.input.z = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                       - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        }

        protected virtual void CameraInput()
        {
            if (!cameraMain)
            {
                if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                else
                {
                    cameraMain = Camera.main;
                    cc.rotateTarget = cameraMain.transform;
                }
            }

            if (cameraMain)
            {
                cc.UpdateMoveDirection(cameraMain.transform);
            }

            if (tpCamera == null)
                return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            var delta = mouse.delta.ReadValue() * m_mouseDeltaScale;
            tpCamera.RotateCamera(delta.x, delta.y);
        }

        protected virtual void StrafeInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.tabKey.wasPressedThisFrame)
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.leftShiftKey.wasPressedThisFrame)
                cc.Sprint(true);
            else if (keyboard.leftShiftKey.wasReleasedThisFrame)
                cc.Sprint(false);
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        protected virtual bool JumpConditions()
        {
            return cc.IsGrounded && cc.GroundAngle() < cc.SlopeLimit && !cc.IsJumping && !cc.StopMove;
        }

        /// <summary>
        /// Input to trigger the Jump
        /// </summary>
        protected virtual void JumpInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.spaceKey.wasPressedThisFrame && JumpConditions())
                cc.Jump();
        }
    }
}
