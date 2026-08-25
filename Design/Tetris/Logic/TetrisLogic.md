# Tetris Logic Design

## 代码规范

- 代码命名空间`ZNGTetris.Logic`
- 代码规范参考`ugu-codingstandards`skill
- 逻辑层代码目录`Assets\Demo\Scripts\TetrisGame\TetrisLogic`
- 参考设计文档目录`.\references`

## 代码设计

### 核心逻辑

- 类名`TetrisGame`
- 必须继承`MonoBehaviour`

#### 输入

- 使用`Unity 6`新输入系统
- 支持在Inspector配置输入按键(下降\左移\右移等逻辑代码支持的玩家操作),每个按键都要有一个默认值.

#### 游戏流程

##### 开始

- 支持自动开始
- 支持调用`开始`方法后开始