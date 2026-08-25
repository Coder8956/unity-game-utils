using System;
using UnityEngine;
using ZNGTetris.Logic;

namespace ZNGTetris.View
{
    /// <summary>
    /// 俄罗斯方块颜色配置，为每种方块类型指定显示颜色。
    /// 可在 Inspector 中自定义，由 2D/3D/UI 显示层共用。
    /// </summary>
    [Serializable]
    public class TetrisBlockColorConfig
    {
        // ── Inspector 字段 ────────────────────────────────────────

        [Header("方块颜色")]
        [Tooltip("I 形方块颜色")]
        [SerializeField] private Color m_colorI = new Color(0f, 1f, 1f, 1f);

        [Tooltip("O 形方块颜色")]
        [SerializeField] private Color m_colorO = new Color(1f, 1f, 0f, 1f);

        [Tooltip("T 形方块颜色")]
        [SerializeField] private Color m_colorT = new Color(0.5f, 0f, 0.5f, 1f);

        [Tooltip("S 形方块颜色")]
        [SerializeField] private Color m_colorS = new Color(0f, 1f, 0f, 1f);

        [Tooltip("Z 形方块颜色")]
        [SerializeField] private Color m_colorZ = new Color(1f, 0f, 0f, 1f);

        [Tooltip("J 形方块颜色")]
        [SerializeField] private Color m_colorJ = new Color(0f, 0.3f, 1f, 1f);

        [Tooltip("L 形方块颜色")]
        [SerializeField] private Color m_colorL = new Color(1f, 0.5f, 0f, 1f);

        // ── 公共入口 ──────────────────────────────────────────────

        /// <summary>
        /// 获取指定方块类型对应的颜色。
        /// </summary>
        public Color GetColor(TetrisBlockType type)
        {
            switch (type)
            {
                case TetrisBlockType.I: return m_colorI;
                case TetrisBlockType.O: return m_colorO;
                case TetrisBlockType.T: return m_colorT;
                case TetrisBlockType.S: return m_colorS;
                case TetrisBlockType.Z: return m_colorZ;
                case TetrisBlockType.J: return m_colorJ;
                case TetrisBlockType.L: return m_colorL;
                default: return Color.clear;
            }
        }
    }
}
