using UnityEngine;

namespace ZNGTetris.Logic
{
    /// <summary>
    /// 碰撞检测系统，负责判断方块在给定位置和旋转状态下是否合法。
    /// 核心算法：逐个检查 Cell 的棋盘坐标是否越界或与已固定方块重叠。
    /// 允许方块部分位于棋盘上方（出生区域），仅检查棋盘内的碰撞。
    /// </summary>
    public class TetrisCollision
    {
        // ── 私有静态字段 ──────────────────────────────────────────

        /// <summary>
        /// 旋转踢墙偏移量（简化版 SRS）。
        /// 当原位置旋转失败时，按顺序尝试这些偏移。
        /// </summary>
        private static readonly Vector2Int[] WallKickOffsets =
        {
            new Vector2Int(0, 0),    // 原位置
            new Vector2Int(-1, 0),   // 左移 1
            new Vector2Int(1, 0),    // 右移 1
            new Vector2Int(-2, 0),   // 左移 2
            new Vector2Int(2, 0),    // 右移 2
            new Vector2Int(0, -1),   // 下移 1
        };

        // ── 公共入口 ──────────────────────────────────────────────

        /// <summary>
        /// 判断给定 Cells 在指定位置下是否合法。
        /// 允许方块部分位于棋盘上方（y >= Height），仅检查水平越界和棋盘内碰撞。
        /// </summary>
        public bool IsValid(TetrisBoard board, Vector2Int position, Vector2Int[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                Vector2Int boardPos = position + cells[i];

                // 水平越界始终非法
                if (boardPos.x < 0 || boardPos.x >= board.Width)
                    return false;

                // 棋盘下方越界始终非法
                if (boardPos.y < 0)
                    return false;

                // 棋盘上方的 Cell（出生区域）跳过碰撞检查
                if (boardPos.y >= board.Height)
                    continue;

                // 棋盘内的 Cell 检查是否已被占用
                if (board.IsOccupied(boardPos.x, boardPos.y))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试旋转并返回踢墙后的合法位置。
        /// 成功时 outPosition 为最终位置，outRotation 为最终旋转状态。
        /// 失败时保持原状态不变。
        /// </summary>
        public bool TryRotateWithKick(
            TetrisBoard board,
            TetrisPiece piece,
            int nextRotation,
            out Vector2Int outPosition,
            out int outRotation)
        {
            Vector2Int[] cells = TetrisShape.GetCells(piece.Type, nextRotation);

            for (int i = 0; i < WallKickOffsets.Length; i++)
            {
                Vector2Int testPos = piece.Position + WallKickOffsets[i];

                if (IsValid(board, testPos, cells))
                {
                    outPosition = testPos;
                    outRotation = nextRotation;
                    return true;
                }
            }

            outPosition = piece.Position;
            outRotation = piece.Rotation;
            return false;
        }
    }
}
