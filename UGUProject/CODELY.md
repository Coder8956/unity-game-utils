

## Codely Structured Memories

### User

### Feedback
- [2026-08-25 21:29:13] [feedback] 编辑模式下使用 ExecuteAlways 时，所有 Destroy 调用必须替换为 SafeDestroy（编辑模式用 DestroyImmediate，运行时用 Destroy）。Unity 不允许在编辑模式下调用 Destroy。
- [2026-08-25 21:29:13] [feedback] Tetris 项目的边框预览要求在 Scene 和 Game 窗口都能看到实际渲染效果，Gizmos 线框不满足要求。使用 HideAndDontSave 标记预览对象防止写入场景文件。
- [2026-08-25 23:37:31] [feedback] [ExecuteAlways] 脚本退出 Play 模式时 GameObject 残留：OnDestroy 中 SafeDestroy 用 Destroy()（延迟），在 Play→Edit 转换中不执行；追踪数组被置 null 后 OnEnable 的 ClearAll 遍历到 null 跳过销毁。正确做法：用 EditorApplication.playModeStateChanged 监听 EnteredEditMode，在回调中 DestroyAllChildren（直接遍历 transform.children 用 DestroyImmediate）+ 重置状态。不能用 bool m_wasPlaying 在 Update 中检测——域重载会丢失标志。
- [2026-08-26 19:32:49] InputAction 构造函数的 binding 参数在 Input System 1.19.0 中不会按逗号拆分——整个字符串作为单个 InputBinding.path，逗号分隔的多键绑定（如 "<Keyboard>/leftArrow,<Keyboard>/a"）是无效路径，无法匹配任何控件。正确做法：构造函数只传主键，再用 AddBinding() 添加副键；或在 Reset() 方法中统一配置多键默认绑定。AddBinding 会正确同步 m_SingletonActionBindings（[SerializeField]）供 Inspector 序列化。
- [2026-08-26 22:16:16] EditorApplication.delayCall 回调（OnValidate 中注册）可能在域重载后 MonoBehaviour 已被销毁时才执行，访问 transform 抛 MissingReferenceException。必须在回调入口加 `if (this == null) return;` 守卫。同理 RefreshBorderPreview 等被 delayCall 调用的方法都需要此检查。

### Project
- [2026-08-25 21:29:13] [project] Tetris 项目设计文档位于 Design/Tetris/ 下，用户会反复更新文档并要求重新阅读后优化代码。文档可能因编码问题需要用 PowerShell Get-Content -Encoding UTF8 读取。

### Reference

