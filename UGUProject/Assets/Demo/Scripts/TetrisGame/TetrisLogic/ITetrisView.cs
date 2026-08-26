namespace ZNGTetris.Logic
{
    /// <summary>
    /// 显示层接口，由 2D 或 3D 显示层实现。
    /// 逻辑层通过此接口通知显示层状态变更，显示层负责将逻辑坐标转换为视觉表现。
    /// </summary>
    public interface ITetrisView
    {
        /// <summary>游戏开始时调用，传入棋盘尺寸供显示层初始化</summary>
        void OnGameStart(int boardWidth, int boardHeight);

        /// <summary>新方块生成时调用</summary>
        void OnPieceSpawned(TetrisPiece piece);

        /// <summary>方块移动时调用</summary>
        void OnPieceMoved(TetrisPiece piece);

        /// <summary>方块旋转时调用</summary>
        void OnPieceRotated(TetrisPiece piece);

        /// <summary>方块固定到棋盘时调用，显示层可据此更新棋盘视觉</summary>
        void OnPieceLocked(TetrisPiece piece);

        /// <summary>消行完成时调用，传入被清除的行号数组和当前棋盘状态</summary>
        void OnLinesCleared(int[] clearedRows, TetrisBoard board);

        /// <summary>游戏结束时调用，传入最终棋盘状态供显示层更新视觉</summary>
        void OnGameOver(TetrisBoard board);

        /// <summary>设置编辑模式预览的棋盘尺寸（由 TetrisGame 调用）</summary>
        void SetPreviewBoardSize(int width, int height);
    }
}
