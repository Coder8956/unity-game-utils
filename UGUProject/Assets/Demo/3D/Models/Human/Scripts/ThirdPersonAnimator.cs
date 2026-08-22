using UnityEngine;

namespace Invector.CharacterController
{
    public class ThirdPersonAnimator : ThirdPersonMotor
    {
        // ── 常量 ──────────────────────────────────────────────────

        public const float WalkSpeed = 0.5f;
        public const float RunningSpeed = 1f;
        public const float SprintSpeed = 1.5f;

        // ── 动画更新 ──────────────────────────────────────────────

        public virtual void UpdateAnimator()
        {
            if (m_animator == null || !m_animator.enabled) return;

            m_animator.SetBool(AnimatorParameters.IsStrafing, IsStrafing);
            m_animator.SetBool(AnimatorParameters.IsSprinting, IsSprinting);
            m_animator.SetBool(AnimatorParameters.IsGrounded, IsGrounded);
            m_animator.SetFloat(AnimatorParameters.GroundDistance, m_groundDistance);

            if (IsStrafing)
            {
                m_animator.SetFloat(AnimatorParameters.InputHorizontal, StopMove ? 0 : m_horizontalSpeed, m_strafeSpeed.animationSmooth, Time.deltaTime);
                m_animator.SetFloat(AnimatorParameters.InputVertical, StopMove ? 0 : m_verticalSpeed, m_strafeSpeed.animationSmooth, Time.deltaTime);
            }
            else
            {
                m_animator.SetFloat(AnimatorParameters.InputVertical, StopMove ? 0 : m_verticalSpeed, m_freeSpeed.animationSmooth, Time.deltaTime);
            }

            m_animator.SetFloat(AnimatorParameters.InputMagnitude, StopMove ? 0f : m_inputMagnitude, IsStrafing ? m_strafeSpeed.animationSmooth : m_freeSpeed.animationSmooth, Time.deltaTime);
        }

        public virtual void SetAnimatorMoveSpeed(MovementSpeed speed)
        {
            Vector3 relativeInput = transform.InverseTransformDirection(m_moveDirection);
            m_verticalSpeed = relativeInput.z;
            m_horizontalSpeed = relativeInput.x;

            var newInput = new Vector2(m_verticalSpeed, m_horizontalSpeed);

            if (speed.walkByDefault)
                m_inputMagnitude = Mathf.Clamp(newInput.magnitude, 0, IsSprinting ? RunningSpeed : WalkSpeed);
            else
                m_inputMagnitude = Mathf.Clamp(IsSprinting ? newInput.magnitude + 0.5f : newInput.magnitude, 0, IsSprinting ? SprintSpeed : RunningSpeed);
        }
    }

    public static partial class AnimatorParameters
    {
        public static int InputHorizontal = Animator.StringToHash("InputHorizontal");
        public static int InputVertical = Animator.StringToHash("InputVertical");
        public static int InputMagnitude = Animator.StringToHash("InputMagnitude");
        public static int IsGrounded = Animator.StringToHash("IsGrounded");
        public static int IsStrafing = Animator.StringToHash("IsStrafing");
        public static int IsSprinting = Animator.StringToHash("IsSprinting");
        public static int GroundDistance = Animator.StringToHash("GroundDistance");
    }
}
