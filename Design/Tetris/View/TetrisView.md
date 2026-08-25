# Tetris View Design

- 代码命名空间`ZNGTetris.View`
- 代码规范参考`ugu-codingstandards`skill

### 边框配置

#### 边框元素

- 支持配置游戏区域边框
- 支持配置水平方向的边框元素
- 支持配置竖直方向的边框元素
- 支持配置四个拐角位置边框元素
- 都提供一个默认配置

#### 边框预览

- 用一个变量控制是否开启边框预览,在Inspector显示类不需要配置逻辑类.在逻辑类配置显示类之后,由逻辑类把参数传给显示类.显示类不允许自动查找逻辑类.
- 支持编辑模式下在Scene窗口和Game窗口实时预览实际效果
- 边框参数只能读取`TetrisGame`类的长宽参数

### 行列预览

- 加一个变量控制
- 变量打开可以在 Scene 窗口看到棋盘的行列辅助线和行列号

### 具体View设计文档

- [3D文档路径](.\details\3DView.md)
- [2D文档路径](.\details\2DView.md)
- [UI文档路径](.\details\UI(Canvas)View.md)

