using System.Collections.Generic;

namespace ZNGTetris.Logic
{
    /// <summary>
    /// 消行系统，负责检测和清除已填满的行，并将上方方块下移。
    /// </summary>
    public class TetrisLineSystem
    {
        // ── 公共入口 ──────────────────────────────────────────────

        /// <summary>
        /// 检测棋盘中所有已填满的行，返回行号数组（从下到上排序）。
        /// </summary>
        public int[] FindFullRows(TetrisBoard board)
        {
            List<int> rows = new List<int>();

            for (int y = 0; y < board.Height; y++)
            {
                if (board.IsRowFull(y))
                {
                    rows.Add(y);
                }
            }

            return rows.ToArray();
        }

        /// <summary>
        /// 清除指定行并下移上方方块。
        /// </summary>
        public void ClearSpecificRows(TetrisBoard board, int[] rows)
        {
            // 从下到上处理，避免下移时覆盖
            for (int i = 0; i < rows.Length; i++)
            {
                int clearedY = rows[i];

                // 将清除行上方的所有行下移一格
                for (int y = clearedY; y < board.Height - 1; y++)
                {
                    board.MoveRow(y + 1, y);
                }

                // 顶部行清空
                board.ClearRow(board.Height - 1);

                // 后续行号需要补偿下移
                for (int j = i + 1; j < rows.Length; j++)
                {
                    rows[j]--;
                }
            }
        }
    }
}
