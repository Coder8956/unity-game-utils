using UnityEngine;

namespace ZNGTetris.Logic
{
    /// <summary>
    /// 当前正在下落的方块状态，包含类型、位置、旋转状态。
    /// 通过 <see cref="GetCells"/> 获取当前旋转下的 Cell 偏移坐标。
    /// </summary>
    public class TetrisPiece
    {
        // ── 属性 ─────────────────────────────────────────────────

        /// <summary>方块类型</summary>
        public TetrisBlockType Type { get; set; }

        /// <summary>方块在棋盘上的位置（左下角基准）</summary>
        public Vector2Int Position { get; set; }

        /// <summary>旋转状态 (0~3)</summary>
        public int Rotation { get; set; }

        // ── 构造 ─────────────────────────────────────────────────

        public TetrisPiece(TetrisBlockType type, Vector2Int position, int rotation = 0)
        {
            Type = type;
            Position = position;
            Rotation = rotation;
        }

        // ── 公共入口 ──────────────────────────────────────────────

        /// <summary>
        /// 获取当前旋转状态下的 Cell 偏移坐标数组。
        /// </summary>
        public Vector2Int[] GetCells()
        {
            return TetrisShape.GetCells(Type, Rotation);
        }

        /// <summary>
        /// 获取指定旋转状态下的 Cell 偏移坐标数组（不修改当前状态）。
        /// </summary>
        public Vector2Int[] GetCells(int rotation)
        {
            return TetrisShape.GetCells(Type, rotation);
        }

        /// <summary>
        /// 获取当前方块在棋盘上的实际占用坐标（Position + Cell 偏移）。
        /// </summary>
        public Vector2Int[] GetWorldCells()
        {
            Vector2Int[] cells = GetCells();
            Vector2Int[] worldCells = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                worldCells[i] = Position + cells[i];
            }

            return worldCells;
        }
    }
}
