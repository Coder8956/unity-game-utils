using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 第三人称相机控制器。
/// 负责相机的跟随、旋转、碰撞避让（裁剪）等逻辑，
/// 使相机始终以目标角色为中心进行平滑环绕拍摄。
/// </summary>
public class TPCamera : MonoBehaviour
{
    // ── Inspector 配置 ────────────────────────────────────────

    [Tooltip("相机跟随的目标变换（通常是角色根节点）")]
    [SerializeField] private Transform m_target;

    [Tooltip("相机状态之间的旋转插值速度，值越大旋转越快")]
    [SerializeField] private float m_smoothCameraRotation = 12f;

    [Tooltip("相机裁剪检测层，这些层上的物体将触发碰撞避让")]
    [SerializeField] private LayerMask m_cullingLayer = 1 << 0;

    [Tooltip("调试用：锁定相机在角色正后方，便于对齐相机状态")]
    [SerializeField] private bool m_lockCamera;

    [Tooltip("相机相对于目标右侧的偏移量")]
    [SerializeField] private float m_rightOffset = 0f;

    [Tooltip("相机与目标之间的默认距离")]
    [SerializeField] private float m_defaultDistance = 2.5f;

    [Tooltip("相机相对于目标的高度偏移")]
    [SerializeField] private float m_height = 1.4f;

    [Tooltip("相机位置跟随目标的平滑速度")]
    [SerializeField] private float m_smoothFollow = 10f;

    [Tooltip("鼠标 X 轴（水平）旋转灵敏度")]
    [SerializeField] private float m_xMouseSensitivity = 3f;

    [Tooltip("鼠标 Y 轴（垂直）旋转灵敏度")]
    [SerializeField] private float m_yMouseSensitivity = 3f;

    [Tooltip("垂直方向最小俯角限制（度）")]
    [SerializeField] private float m_yMinLimit = -40f;

    [Tooltip("垂直方向最大仰角限制（度）")]
    [SerializeField] private float m_yMaxLimit = 80f;

    [Header("Mouse Input")]
    [Tooltip("鼠标移动缩放系数，用于将原始像素增量转换为旋转量")]
    [SerializeField] private float m_mouseDeltaScale = 0.1f;

    // ── 公共属性（运行时状态，外部只读） ─────────────

    public int IndexList { get; private set; }
    public int IndexLookPoint { get; private set; }
    public float OffSetPlayerPivot { get; private set; }
    public string CurrentStateName { get; private set; }
    public Transform CurrentTarget { get; private set; }
    public Vector2 MovementSpeed { get; private set; }

    // ── 运行时状态 ──────────────────────────────────────────

    /// <summary>相机实际注视的虚拟目标点（由鼠标旋转驱动）</summary>
    private Transform m_targetLookAt;
    /// <summary>当前目标位置缓存（已加上 pivot 偏移）</summary>
    private Vector3 m_currentTargetPos;
    /// <summary>当前相机位置（经过高度调整后）</summary>
    private Vector3 m_currentCPos;
    /// <summary>期望的相机位置（未经过碰撞裁剪的理想位置）</summary>
    private Vector3 m_desiredCPos;
    private Camera m_camera;
    /// <summary>当前实际距离（受碰撞裁剪影响，会动态变化）</summary>
    private float m_distance = 5f;
    /// <summary>当前鼠标 Y 轴累积旋转角度</summary>
    private float m_mouseY = 0f;
    /// <summary>当前鼠标 X 轴累积旋转角度</summary>
    private float m_mouseX = 0f;
    /// <summary>当前相机高度（碰撞时会在 cullingHeight 与 m_height 间插值）</summary>
    private float m_currentHeight;
    /// <summary>裁剪检测中记录的最小碰撞距离</summary>
    private float m_cullingDistance;
    /// <summary>高度检测用的球体半径</summary>
    private float m_checkHeightRadius = 0.4f;
    /// <summary>近裁剪面边距，用于扩大碰撞检测范围</summary>
    private float m_clipPlaneMargin = 0f;
    /// <summary>相机前向方向系数（-1 表示在目标后方）</summary>
    private float m_forward = -1f;
    /// <summary>水平旋转最小限制（度）</summary>
    private float m_xMinLimit = -360f;
    /// <summary>水平旋转最大限制（度）</summary>
    private float m_xMaxLimit = 360f;
    /// <summary>碰撞裁剪时相机下压的最低高度</summary>
    private float m_cullingHeight = 0.2f;
    /// <summary>碰撞裁剪最小间距，低于此值会进一步压缩距离</summary>
    private float m_cullingMinDist = 0.1f;

    // ── 生命周期 ─────────────────────────────────────────────

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        var delta = mouse.delta.ReadValue() * m_mouseDeltaScale;
        RotateCamera(delta.x, delta.y);
    }

    private void FixedUpdate()
    {
        if (m_target == null || m_targetLookAt == null) return;

        CameraMovement();
    }

    // ── 公共接口 ──────────────────────────────────────────────

    /// <summary>
    /// 初始化相机：创建虚拟注视点并设置初始角度与距离。
    /// </summary>
    public void Init()
    {
        if (m_target == null)
            return;

        m_camera = GetComponent<Camera>();
        CurrentTarget = m_target;
        m_currentTargetPos = new Vector3(CurrentTarget.position.x, CurrentTarget.position.y + OffSetPlayerPivot, CurrentTarget.position.z);

        // 创建虚拟注视点对象，跟随目标位置与旋转
        m_targetLookAt = new GameObject("targetLookAt").transform;
        m_targetLookAt.position = CurrentTarget.position;
        m_targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        m_targetLookAt.rotation = CurrentTarget.rotation;

        // 初始化鼠标角度为目标朝向
        m_mouseY = CurrentTarget.eulerAngles.x;
        m_mouseX = CurrentTarget.eulerAngles.y;

        m_distance = m_defaultDistance;
        m_currentHeight = m_height;
    }

    /// <summary>
    /// 设置相机注视目标（不会重新初始化）。
    /// </summary>
    /// <param name="newTarget">新的目标变换；为 null 则回退到默认目标</param>
    public void SetTarget(Transform newTarget)
    {
        CurrentTarget = newTarget ? newTarget : m_target;
    }

    /// <summary>
    /// 设置主目标并重新初始化相机。
    /// </summary>
    /// <param name="newTarget">新的主目标变换</param>
    public void SetMainTarget(Transform newTarget)
    {
        m_target = newTarget;
        CurrentTarget = newTarget;
        m_mouseY = CurrentTarget.rotation.eulerAngles.x;
        m_mouseX = CurrentTarget.rotation.eulerAngles.y;
        Init();
    }

    /// <summary>
    /// 将屏幕坐标点转换为世界空间射线。
    /// </summary>
    /// <param name="point">屏幕坐标点</param>
    /// <returns>从相机发出的射线</returns>
    public Ray ScreenPointToRay(Vector3 point)
    {
        return m_camera.ScreenPointToRay(point);
    }

    /// <summary>
    /// 根据鼠标输入旋转相机。
    /// 鼠标 X 控制水平旋转，Y 控制垂直俯仰；
    /// 非锁定模式下会限制角度范围。
    /// </summary>
    /// <param name="x">鼠标 X 轴增量</param>
    /// <param name="y">鼠标 Y 轴增量</param>
    public void RotateCamera(float x, float y)
    {
        // 累积鼠标输入并应用灵敏度
        m_mouseX += x * m_xMouseSensitivity;
        m_mouseY -= y * m_yMouseSensitivity;

        // 记录移动速度供外部使用
        MovementSpeed = new Vector2(x, -y);
        if (!m_lockCamera)
        {
            // 非锁定模式：限制角度范围
            m_mouseY = ClampAngle(m_mouseY, m_yMinLimit, m_yMaxLimit);
            m_mouseX = ClampAngle(m_mouseX, m_xMinLimit, m_xMaxLimit);
        }
        else
        {
            // 锁定模式：角度与角色根节点朝向一致
            m_mouseY = CurrentTarget.root.localEulerAngles.x;
            m_mouseX = CurrentTarget.root.localEulerAngles.y;
        }
    }

    // ── 相机逻辑 ─────────────────────────────────────────────

    /// <summary>
    /// 相机移动核心逻辑：
    /// 1. 根据鼠标角度旋转虚拟注视点；
    /// 2. 基于注视点前方方向计算相机期望位置；
    /// 3. 通过近裁剪面四角射线检测碰撞并压缩距离；
    /// 4. 最终设置相机位置与朝向。
    /// </summary>
    private void CameraMovement()
    {
        if (CurrentTarget == null)
            return;

        // 距离平滑回弹到默认值
        m_distance = Mathf.Lerp(m_distance, m_defaultDistance, m_smoothFollow * Time.deltaTime);
        m_cullingDistance = Mathf.Lerp(m_cullingDistance, m_distance, Time.deltaTime);

        // 计算相机相对注视点的方向：后向 + 右侧偏移
        var camDir = (m_forward * m_targetLookAt.forward) + (m_rightOffset * m_targetLookAt.right);
        camDir = camDir.normalized;

        // 计算目标位置（含 pivot 偏移）
        var targetPos = new Vector3(CurrentTarget.position.x, CurrentTarget.position.y + OffSetPlayerPivot, CurrentTarget.position.z);
        m_currentTargetPos = targetPos;

        // 期望位置 = 目标位置 + 高度偏移
        m_desiredCPos = targetPos + new Vector3(0, m_height, 0);
        // 当前位置 = 目标位置 + 当前高度（碰撞时可能更低）
        m_currentCPos = m_currentTargetPos + new Vector3(0, m_currentHeight, 0);

        RaycastHit hitInfo;

        // 计算近裁剪面四角在当前与期望位置的世界坐标
        ClipPlanePoints planePoints = NearClipPlanePoints(m_camera, m_currentCPos + (camDir * (m_distance)), m_clipPlaneMargin);
        ClipPlanePoints oldPoints = NearClipPlanePoints(m_camera, m_desiredCPos + (camDir * m_distance), m_clipPlaneMargin);

        // 头顶高度检测：如果上方有遮挡物，逐步降低相机高度
        if (Physics.SphereCast(targetPos, m_checkHeightRadius, Vector3.up, out hitInfo, m_cullingHeight + 0.2f, m_cullingLayer))
        {
            var t = hitInfo.distance - 0.2f;
            t -= m_height;
            t /= (m_cullingHeight - m_height);
            m_cullingHeight = Mathf.Lerp(m_height, m_cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
        }

        // 期望位置的裁剪检测：如果有遮挡则压缩距离
        if (CullingRayCast(m_desiredCPos, oldPoints, out hitInfo, m_distance + 0.2f, m_cullingLayer, Color.blue))
        {
            m_distance = hitInfo.distance - 0.2f;
            if (m_distance < m_defaultDistance)
            {
                // 距离过近时进一步降低相机高度
                var t = hitInfo.distance;
                t -= m_cullingMinDist;
                t /= m_cullingMinDist;
                m_currentHeight = Mathf.Lerp(m_cullingHeight, m_height, Mathf.Clamp(t, 0.0f, 1.0f));
                m_currentCPos = m_currentTargetPos + new Vector3(0, m_currentHeight, 0);
            }
        }
        else
        {
            // 无遮挡时恢复默认高度
            m_currentHeight = m_height;
        }

        // 当前位置的裁剪检测：将距离限制在有效范围内
        if (CullingRayCast(m_currentCPos, planePoints, out hitInfo, m_distance, m_cullingLayer, Color.cyan))
            m_distance = Mathf.Clamp(m_cullingDistance, 0.0f, m_defaultDistance);

        // 计算注视点：当前位置前方 + 右侧偏移分量
        var lookPoint = m_currentCPos + m_targetLookAt.forward * 2f;
        lookPoint += (m_targetLookAt.right * Vector3.Dot(camDir * (m_distance), m_targetLookAt.right));

        // 更新虚拟注视点位置
        m_targetLookAt.position = m_currentCPos;

        // 平滑旋转虚拟注视点
        Quaternion newRot = Quaternion.Euler(m_mouseY, m_mouseX, 0);
        m_targetLookAt.rotation = Quaternion.Slerp(m_targetLookAt.rotation, newRot, m_smoothCameraRotation * Time.deltaTime);

        // 设置相机最终位置
        transform.position = m_currentCPos + (camDir * (m_distance));

        // 相机朝向注视点
        var rotation = Quaternion.LookRotation((lookPoint) - transform.position);
        transform.rotation = rotation;

        MovementSpeed = Vector2.zero;
    }

    /// <summary>
    /// 从起点向近裁剪面四个角发射射线进行碰撞检测，
    /// 取最近碰撞距离作为裁剪距离。
    /// </summary>
    /// <param name="from">射线起点</param>
    /// <param name="to">近裁剪面四角坐标结构</param>
    /// <param name="hitInfo">输出碰撞信息</param>
    /// <param name="distance">最大检测距离</param>
    /// <param name="cullingLayer">裁剪检测层</param>
    /// <param name="color">调试绘制颜色（仅用于 Editor 可视化）</param>
    /// <returns>是否发生碰撞</returns>
    private bool CullingRayCast(Vector3 from, ClipPlanePoints to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
    {
        bool value = false;

        // 左下角射线检测
        if (Physics.Raycast(from, to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            m_cullingDistance = hitInfo.distance;
        }

        // 右下角射线检测
        if (Physics.Raycast(from, to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
        }

        // 左上角射线检测
        if (Physics.Raycast(from, to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
        }

        // 右上角射线检测
        if (Physics.Raycast(from, to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
        }

        return hitInfo.collider && value;
    }

    // ── 工具方法 ─────────────────────────────────────────────

    /// <summary>
    /// 将角度限制在 [-360, 360] 范围内，再做 [min, max] 区间裁剪。
    /// </summary>
    /// <param name="angle">输入角度</param>
    /// <param name="min">最小限制</param>
    /// <param name="max">最大限制</param>
    /// <returns>限制后的角度</returns>
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

    /// <summary>
    /// 计算相机近裁剪面在世界空间的四个角坐标。
    /// 基于 FOV、宽高比、近裁剪面距离推算四个顶点位置。
    /// </summary>
    /// <param name="camera">目标相机</param>
    /// <param name="pos">裁剪面中心位置</param>
    /// <param name="clipPlaneMargin">边距扩展系数</param>
    /// <returns>包含四个角坐标的结构体</returns>
    private static ClipPlanePoints NearClipPlanePoints(Camera camera, Vector3 pos, float clipPlaneMargin)
    {
        var clipPlanePoints = new ClipPlanePoints();

        var transform = camera.transform;
        var halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
        var aspect = camera.aspect;
        var distance = camera.nearClipPlane;
        // 根据半 FOV 和近裁剪面距离计算裁剪面半高/半宽
        var height = distance * Mathf.Tan(halfFOV);
        var width = height * aspect;
        // 应用边距扩展
        height *= 1 + clipPlaneMargin;
        width *= 1 + clipPlaneMargin;

        // 右下角：+右 -上 +前
        clipPlanePoints.LowerRight = pos + transform.right * width;
        clipPlanePoints.LowerRight -= transform.up * height;
        clipPlanePoints.LowerRight += transform.forward * distance;

        // 左下角：-右 -上 +前
        clipPlanePoints.LowerLeft = pos - transform.right * width;
        clipPlanePoints.LowerLeft -= transform.up * height;
        clipPlanePoints.LowerLeft += transform.forward * distance;

        // 右上角：+右 +上 +前
        clipPlanePoints.UpperRight = pos + transform.right * width;
        clipPlanePoints.UpperRight += transform.up * height;
        clipPlanePoints.UpperRight += transform.forward * distance;

        // 左上角：-右 +上 +前
        clipPlanePoints.UpperLeft = pos - transform.right * width;
        clipPlanePoints.UpperLeft += transform.up * height;
        clipPlanePoints.UpperLeft += transform.forward * distance;

        return clipPlanePoints;
    }

    /// <summary>
    /// 近裁剪面四个角的世界坐标结构体。
    /// </summary>
    private struct ClipPlanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
    }
}
