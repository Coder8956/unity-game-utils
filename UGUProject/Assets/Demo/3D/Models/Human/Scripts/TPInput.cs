using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 第三人称输入处理器。
/// 负责读取键盘/鼠标输入并将其转发给控制器和相机，
/// 是输入与逻辑之间的桥梁。挂载在角色 GameObject 上。
/// </summary>
public class TPInput : MonoBehaviour
{
    // ── Inspector 配置 ────────────────────────────────────────

    [Header("Camera Input")]
    [Tooltip("鼠标移动缩放系数，用于将原始像素增量转换为旋转量")]
    [SerializeField] private float m_mouseDeltaScale = 0.1f;

    public TPController CC { get; private set; }
    public TPCamera TpCamera { get; private set; }
    public Camera CameraMain { get; private set; }

    // ── 生命周期 ─────────────────────────────────────────────

    /// <summary>初始化：获取控制器并调用其 Init</summary>
    protected virtual void Start()
    {
        InitilizeController();
        InitializeTpCamera();
    }

    /// <summary>固定更新：驱动运动器与移动/旋转类型控制</summary>
    protected virtual void FixedUpdate()
    {
        CC.UpdateMotor();
        CC.ControlLocomotionType();
        CC.ControlRotationType();
    }

    /// <summary>每帧更新：处理输入并更新动画</summary>
    protected virtual void Update()
    {
        InputHandle();
        CC.UpdateAnimator();
    }

    /// <summary>动画根位移回调：由 Animator 驱动角色位移</summary>
    public virtual void OnAnimatorMove()
    {
        CC.ControlAnimatorRootMotion();
    }

    // ── 输入处理 ──────────────────────────────────────────────

    /// <summary>
    /// 获取并初始化 TPController 组件。
    /// </summary>
    protected virtual void InitilizeController()
    {
        CC = GetComponent<TPController>();

        if (CC != null)
            CC.Init();
    }

    /// <summary>
    /// 查找场景中的 TPCamera 并将其主目标设为当前角色。
    /// </summary>
    protected virtual void InitializeTpCamera()
    {
        if (TpCamera == null)
        {
            TpCamera = FindFirstObjectByType<TPCamera>();
            if (TpCamera == null)
                return;
            if (TpCamera)
            {
                TpCamera.SetMainTarget(this.transform);
                TpCamera.Init();
            }
        }
    }

    /// <summary>
    /// 每帧输入总入口，依次处理移动、相机、冲刺、瞄准、跳跃。
    /// </summary>
    protected virtual void InputHandle()
    {
        MoveInput();
        CameraInput();
        SprintInput();
        StrafeInput();
        JumpInput();
    }

    /// <summary>
    /// 读取 WASD/方向键输入，写入控制器的 Input 向量。
    /// X 轴：A/D（左右），Z 轴：W/S（前后）。
    /// </summary>
    public virtual void MoveInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        var newInput = CC.Input;
        newInput.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                   - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
        newInput.z = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                   - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        CC.Input = newInput;
    }

    /// <summary>
    /// 处理相机相关输入：
    /// 1. 查找主相机并设置控制器的旋转参考目标；
    /// 2. 根据相机朝向更新角色移动方向；
    /// 3. 读取鼠标移动量并驱动相机旋转。
    /// </summary>
    protected virtual void CameraInput()
    {
        if (!CameraMain)
        {
            if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
            else
            {
                CameraMain = Camera.main;
                CC.RotateTarget = CameraMain.transform;
            }
        }

        if (CameraMain)
        {
            CC.UpdateMoveDirection(CameraMain.transform);
        }

        if (TpCamera == null)
            return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // 读取鼠标增量并缩放后传给相机
        var delta = mouse.delta.ReadValue() * m_mouseDeltaScale;
        TpCamera.RotateCamera(delta.x, delta.y);
    }

    /// <summary>
    /// Tab 键切换瞄准/横移模式。
    /// </summary>
    protected virtual void StrafeInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.tabKey.wasPressedThisFrame)
            CC.Strafe();
    }

    /// <summary>
    /// 左 Shift 键按下/松开切换冲刺状态。
    /// </summary>
    protected virtual void SprintInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.leftShiftKey.wasPressedThisFrame)
            CC.Sprint(true);
        else if (keyboard.leftShiftKey.wasReleasedThisFrame)
            CC.Sprint(false);
    }

    /// <summary>
    /// 跳跃条件判定：着地、坡度小于限制、非跳跃中、非停止移动。
    /// </summary>
    /// <returns>是否满足跳跃条件</returns>
    protected virtual bool JumpConditions()
    {
        return CC.IsGrounded && CC.GroundAngle() < CC.SlopeLimit && !CC.IsJumping && !CC.StopMove;
    }

    /// <summary>
    /// 空格键触发跳跃（满足条件时）。
    /// </summary>
    protected virtual void JumpInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame && JumpConditions())
            CC.Jump();
    }
}
