using System;
using UnityEngine;

namespace ZNGTetris.Logic
{
    /// <summary>
    /// 方块形状数据库，提供 I/O/T/S/Z/J/L 七种方块在 4 个旋转状态下的 Cell 偏移坐标。
    /// 所有坐标均为相对于 Piece.Position 的局部偏移。
    /// </summary>
    public static class TetrisShape
    {
        // ── 常量 ─────────────────────────────────────────────────

        /// <summary>旋转状态数量</summary>
        public const int RotationCount = 4;

        // ── 形状定义 ──────────────────────────────────────────────

        // I: XXXX / .... / .... / ....  (4 个旋转状态，0 和 2 相同，1 和 3 相同)
        private static readonly Vector2Int[] IRot0 =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0)
        };
        private static readonly Vector2Int[] IRot1 =
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3)
        };

        // O: XX / XX  (所有旋转状态相同)
        private static readonly Vector2Int[] ORot =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)
        };

        // T:
        // Rot0: .X. / XXX    Rot1: X. / XX / X.    Rot2: XXX / .X.    Rot3: .X / XX / .X
        private static readonly Vector2Int[] TRot0 =
        {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)
        };
        private static readonly Vector2Int[] TRot1 =
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2)
        };
        private static readonly Vector2Int[] TRot2 =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1)
        };
        private static readonly Vector2Int[] TRot3 =
        {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2)
        };

        // S:
        // Rot0: .XX / XX.    Rot1: X. / XX / .X
        private static readonly Vector2Int[] SRot0 =
        {
            new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)
        };
        private static readonly Vector2Int[] SRot1 =
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2)
        };

        // Z:
        // Rot0: XX. / .XX    Rot1: .X / XX / X.
        private static readonly Vector2Int[] ZRot0 =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1)
        };
        private static readonly Vector2Int[] ZRot1 =
        {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2)
        };

        // J:
        // Rot0: X.. / XXX    Rot1: XX / X. / X.    Rot2: XXX / ..X    Rot3: .X / .X / XX
        private static readonly Vector2Int[] JRot0 =
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)
        };
        private static readonly Vector2Int[] JRot1 =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)
        };
        private static readonly Vector2Int[] JRot2 =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1)
        };
        private static readonly Vector2Int[] JRot3 =
        {
            new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 2), new Vector2Int(1, 2)
        };

        // L:
        // Rot0: ..X / XXX    Rot1: X. / X. / XX    Rot2: XXX / X..    Rot3: XX / .X / .X
        private static readonly Vector2Int[] LRot0 =
        {
            new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)
        };
        private static readonly Vector2Int[] LRot1 =
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2)
        };
        private static readonly Vector2Int[] LRot2 =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1)
        };
        private static readonly Vector2Int[] LRot3 =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2)
        };

        // ── 旋转查表 ──────────────────────────────────────────────

        private static readonly Vector2Int[][] IRotations = { IRot0, IRot1, IRot0, IRot1 };
        private static readonly Vector2Int[][] ORotations = { ORot, ORot, ORot, ORot };
        private static readonly Vector2Int[][] TRotations = { TRot0, TRot1, TRot2, TRot3 };
        private static readonly Vector2Int[][] SRotations = { SRot0, SRot1, SRot0, SRot1 };
        private static readonly Vector2Int[][] ZRotations = { ZRot0, ZRot1, ZRot0, ZRot1 };
        private static readonly Vector2Int[][] JRotations = { JRot0, JRot1, JRot2, JRot3 };
        private static readonly Vector2Int[][] LRotations = { LRot0, LRot1, LRot2, LRot3 };

        // ── 公共入口 ──────────────────────────────────────────────

        /// <summary>
        /// 获取指定方块类型和旋转状态下的 Cell 偏移坐标数组。
        /// </summary>
        public static Vector2Int[] GetCells(TetrisBlockType type, int rotation)
        {
            int rot = rotation % RotationCount;
            return GetRotationTable(type)[rot];
        }

        // ── 内部工具 ──────────────────────────────────────────────

        private static Vector2Int[][] GetRotationTable(TetrisBlockType type)
        {
            switch (type)
            {
                case TetrisBlockType.I: return IRotations;
                case TetrisBlockType.O: return ORotations;
                case TetrisBlockType.T: return TRotations;
                case TetrisBlockType.S: return SRotations;
                case TetrisBlockType.Z: return ZRotations;
                case TetrisBlockType.J: return JRotations;
                case TetrisBlockType.L: return LRotations;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type,
                        $"不支持的方块类型: {type}");
            }
        }
    }
}
