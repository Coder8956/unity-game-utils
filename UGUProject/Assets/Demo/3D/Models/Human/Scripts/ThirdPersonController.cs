using UnityEngine;

namespace Invector.CharacterController
{
    public class ThirdPersonController : ThirdPersonAnimator
    {
        // ── 根位移控制 ────────────────────────────────────────────

        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled) return;

            if (m_inputSmooth == Vector3.zero)
            {
                transform.position = m_animator.rootPosition;
                transform.rotation = m_animator.rootRotation;
            }

            if (m_useRootMotion)
                MoveCharacter(m_moveDirection);
        }

        // ── 移动类型控制 ──────────────────────────────────────────

        public virtual void ControlLocomotionType()
        {
            if (m_lockMovement) return;

            if (m_locomotionType.Equals(LocomotionType.FreeWithStrafe) && !IsStrafing || m_locomotionType.Equals(LocomotionType.OnlyFree))
            {
                SetControllerMoveSpeed(m_freeSpeed);
                SetAnimatorMoveSpeed(m_freeSpeed);
            }
            else if (m_locomotionType.Equals(LocomotionType.OnlyStrafe) || m_locomotionType.Equals(LocomotionType.FreeWithStrafe) && IsStrafing)
            {
                IsStrafing = true;
                SetControllerMoveSpeed(m_strafeSpeed);
                SetAnimatorMoveSpeed(m_strafeSpeed);
            }

            if (!m_useRootMotion)
                MoveCharacter(m_moveDirection);
        }

        // ── 旋转控制 ──────────────────────────────────────────────

        public virtual void ControlRotationType()
        {
            if (m_lockRotation) return;

            bool validInput = input != Vector3.zero || (IsStrafing ? m_strafeSpeed.rotateWithCamera : m_freeSpeed.rotateWithCamera);

            if (validInput)
            {
                m_inputSmooth = Vector3.Lerp(m_inputSmooth, input, (IsStrafing ? m_strafeSpeed.movementSmooth : m_freeSpeed.movementSmooth) * Time.deltaTime);

                Vector3 dir = (IsStrafing && (!IsSprinting || m_sprintOnlyFree == false) || (m_freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : m_moveDirection;
                RotateToDirection(dir);
            }
        }

        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (input.magnitude <= 0.01)
            {
                m_moveDirection = Vector3.Lerp(m_moveDirection, Vector3.zero, (IsStrafing ? m_strafeSpeed.movementSmooth : m_freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (referenceTransform && !m_rotateByWorld)
            {
                var right = referenceTransform.right;
                right.y = 0;
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                m_moveDirection = (m_inputSmooth.x * right) + (m_inputSmooth.z * forward);
            }
            else
            {
                m_moveDirection = new Vector3(m_inputSmooth.x, 0, m_inputSmooth.z);
            }
        }

        // ── 动作输入 ──────────────────────────────────────────────

        public virtual void Sprint(bool value)
        {
            var sprintConditions = (input.sqrMagnitude > 0.1f && IsGrounded &&
                !(IsStrafing && !m_strafeSpeed.walkByDefault && (m_horizontalSpeed >= 0.5 || m_horizontalSpeed <= -0.5 || m_verticalSpeed <= 0.1f)));

            if (value && sprintConditions)
            {
                if (input.sqrMagnitude > 0.1f)
                {
                    if (IsGrounded && m_useContinuousSprint)
                    {
                        IsSprinting = !IsSprinting;
                    }
                    else if (!IsSprinting)
                    {
                        IsSprinting = true;
                    }
                }
                else if (!m_useContinuousSprint && IsSprinting)
                {
                    IsSprinting = false;
                }
            }
            else if (IsSprinting)
            {
                IsSprinting = false;
            }
        }

        public virtual void Strafe()
        {
            IsStrafing = !IsStrafing;
        }

        public virtual void Jump()
        {
            m_jumpCounter = m_jumpTimer;
            IsJumping = true;

            if (input.sqrMagnitude < 0.1f)
                m_animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                m_animator.CrossFadeInFixedTime("JumpMove", .2f);
        }
    }
}
