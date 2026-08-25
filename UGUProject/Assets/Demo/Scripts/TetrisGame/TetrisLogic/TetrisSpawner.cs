using UnityEngine;

namespace ZNGTetris.Logic
{
    /// <summary>
    /// 方块生成系统，负责随机生成下一个方块并指定初始位置。
    /// 方块从棋盘顶部中间位置生成。
    /// </summary>
    public class TetrisSpawner
    {
        // ── 私有字段 ──────────────────────────────────────────────

        private readonly int m_boardWidth;
        private readonly int m_boardHeight;

        // ── 属性 ─────────────────────────────────────────────────

        /// <summary>下一个方块的类型（用于显示层预览）</summary>
        public TetrisBlockType NextType { get; private set; }

        // ── 构造 ─────────────────────────────────────────────────

        public TetrisSpawner(int boardWidth, int boardHeight)
        {
            m_boardWidth = boardWidth;
            m_boardHeight = boardHeight;
            NextType = RandomType();
        }

        // ── 公共入口 ──────────────────────────────────────────────

        /// <summary>
        /// 生成下一个方块，返回新的 TetrisPiece 实例。
        /// 方块从棋盘顶部中间位置生成。
        /// </summary>
        public TetrisPiece Spawn()
        {
            TetrisBlockType type = NextType;
            NextType = RandomType();

            int spawnX = Mathf.FloorToInt(m_boardWidth / 2f) - 1;
            int spawnY = m_boardHeight - 2;
            Vector2Int position = new Vector2Int(spawnX, spawnY);

            return new TetrisPiece(type, position, 0);
        }

        // ── 内部工具 ──────────────────────────────────────────────

        private static TetrisBlockType RandomType()
        {
            // 7 种方块类型 I(1)~L(7)，不含 Empty(0)
            int value = Random.Range(1, 8);
            return (TetrisBlockType)value;
        }
    }
}
