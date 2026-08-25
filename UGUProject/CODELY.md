

## Codely Structured Memories

### User

### Feedback
- [2026-08-25 21:29:13] [feedback] 编辑模式下使用 ExecuteAlways 时，所有 Destroy 调用必须替换为 SafeDestroy（编辑模式用 DestroyImmediate，运行时用 Destroy）。Unity 不允许在编辑模式下调用 Destroy。
- [2026-08-25 21:29:13] [feedback] Tetris 项目的边框预览要求在 Scene 和 Game 窗口都能看到实际渲染效果，Gizmos 线框不满足要求。使用 HideAndDontSave 标记预览对象防止写入场景文件。

### Project
- [2026-08-25 21:29:13] [project] Tetris 项目设计文档位于 Design/Tetris/ 下，用户会反复更新文档并要求重新阅读后优化代码。文档可能因编码问题需要用 PowerShell Get-Content -Encoding UTF8 读取。

### Reference

