# Tetris Logic Design

## 代码规范

- 代码命名空间`ZNGTetris.Logic`
- 代码规范参考`ugu-codingstandards`skill
- 逻辑层代码目录`Assets\Demo\Scripts\TetrisGame\TetrisLogic`
- 参考设计文档目录`.\references`

## 代码设计

### 核心逻辑类

- 类名`TetrisGame`
- 必须继承`MonoBehaviour`

#### 输入

- 使用`Unity 6`新输入系统
- 支持在Inspector配置输入按键(下降\左移\右移等逻辑代码支持的玩家操作),每个按键都要有一个默认值.
- 默认值
    - 硬下降
        - keyboard `Space`
    - 软下降
        - keyboard `Down Arrow`
        - keyboard `S`
    - 左移
        - keyboard `Left Arrow`
        - keyboard `A`
    - 右移
        - keyboard `Right Arrow`
        - keyboard `D`
    - 旋转
        - keyboard `Up Arrow`
        - keyboard `W`
    - 开始游戏
        - keyboard `Enter`

#### 生成方块

- 方块在棋盘之外生成,逐步向棋盘内移动.
- 方块(局部)出现在棋盘内之后,才接受玩家输入控制
- 边框就是出生线

#### 游戏流程

##### 开始

- 支持自动开始
- 支持调用`开始`方法后开始

##### 结束

- 如果最后一个元素无法全部写入棋盘,在保持元素的当前姿态能且只能在垂直方向尝试向下移动之后,将能写入棋盘的写入棋盘,不能写入棋盘的忽略丢弃.
- 不允许覆盖已经写入的位置
- 运算时方块元素要保持整体性,组成方块的元素不能独立运算.
- 运算结束后,方块元素坐标在棋盘内的写入棋盘,在棋盘外的直接丢弃
- 例最后一个块是
```
X
XXX
```
棋盘结束前的状态是
```
..........
.....X....
....XX....
.....X....
....XXXX..
....XXXX..
```
那么只写入
```
XXX
```
那么棋盘的结束状态就是
```
....XXX...
.....X....
....XX....
.....X....
....XXX...
....XXX...
```

#### Debug

##### 打印棋盘最后的状态

- 添加一个debug变量`m_printBoardOnOver`
- 变量开启,在游戏结束时,在控制台打印出棋盘的行列状态
    - 有元素的用`O`表示
    - 没有元素的用`+`表示
    - 行列要格式化对齐,每一行每一列要与行列号对齐.
    - 打印出最后棋盘状态时,要在棋盘状态之外打印出最后一个元素(用`O`和`+`打印出状态图案)