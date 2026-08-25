using UnityEngine;

namespace ZNGTetris.Logic
{
    /// <summary>
    /// 俄罗斯方块棋盘，保存已固定方块的二维网格数据。
    /// 正在下落的当前方块不写入棋盘，由 TetrisPiece 单独维护。
    /// </summary>
    public class TetrisBoard
    {
        // ── 私有字段 ──────────────────────────────────────────────

        private readonly TetrisBlockType[,] m_cells;

        // ── 属性 ─────────────────────────────────────────────────

        public int Width { get; }
        public int Height { get; }

        // ── 构造 ─────────────────────────────────────────────────

        public TetrisBoard(int width, int height)
        {
            Width = width;
            Height = height;
            m_cells = new TetrisBlockType[width, height];
        }

        // ── 公共入口 ──────────────────────────────────────────────

        /// <summary>
        /// 读取指定坐标的格子类型。
        /// </summary>
        public TetrisBlockType GetCell(int x, int y)
        {
            return m_cells[x, y];
        }

        /// <summary>
        /// 设置指定坐标的格子类型。
        /// </summary>
        public void SetCell(int x, int y, TetrisBlockType type)
        {
            m_cells[x, y] = type;
        }

        /// <summary>
        /// 判断坐标是否在棋盘范围内。
        /// </summary>
        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// 判断指定坐标是否已有固定方块。
        /// 越界坐标视为已被占用（碰墙/碰底）。
        /// </summary>
        public bool IsOccupied(int x, int y)
        {
            if (!IsInside(x, y))
                return true;

            return m_cells[x, y] != TetrisBlockType.Empty;
        }

        /// <summary>
        /// 判断指定行是否已填满。
        /// </summary>
        public bool IsRowFull(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                if (m_cells[x, y] == TetrisBlockType.Empty)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 清空指定行的所有格子。
        /// </summary>
        public void ClearRow(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                m_cells[x, y] = TetrisBlockType.Empty;
            }
        }

        /// <summary>
        /// 将 fromY 行的内容移动到 toY 行，并清空 fromY 行。
        /// </summary>
        public void MoveRow(int fromY, int toY)
        {
            for (int x = 0; x < Width; x++)
            {
                m_cells[x, toY] = m_cells[x, fromY];
                m_cells[x, fromY] = TetrisBlockType.Empty;
            }
        }

        /// <summary>
        /// 清空整个棋盘。
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    m_cells[x, y] = TetrisBlockType.Empty;
                }
            }
        }
    }
}
