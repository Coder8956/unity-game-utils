using UnityEngine;
using ZNGTetris.Logic;

namespace ZNGTetris.View
{
    /// <summary>
    /// 3D 显示层，使用 Cube 基本体渲染方块格子。
    /// 使用 <see cref="MaterialPropertyBlock"/> 设置颜色，兼容 Built-in / URP / HDRP 管线。
    /// 挂载到场景中的 GameObject 上，并赋值给 TetrisGame 的 m_viewComponent。
    /// </summary>
    public class Tetris3DView : TetrisViewBase
    {
        // ── 私有静态字段 ──────────────────────────────────────────

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        // ── Inspector 字段 ────────────────────────────────────────

        [Header("3D 配置")]
        [Tooltip("组成方块的元素预制体（为空时使用默认 Cube）")]
        [SerializeField] private GameObject m_cellPrefab;

        [Tooltip("下落方块相对于棋盘格子的 Z 轴偏移（避免 Z-fighting）")]
        [SerializeField] private float m_pieceZOffset = -0.05f;

        // ── 私有字段（运行时状态）────────────────────────────────

        private MaterialPropertyBlock m_propertyBlock;

        // ── 生命周期 ─────────────────────────────────────────────

        private void Awake()
        {
            m_propertyBlock = new MaterialPropertyBlock();
        }

        // ── 保护方法（实现基类抽象）──────────────────────────────

        protected override GameObject CreateCellVisual(TetrisBlockType type, Vector3 localPos, bool isPiece)
        {
            if (isPiece)
                localPos.z += m_pieceZOffset;

            Color color = m_colorConfig != null ? m_colorConfig.GetColor(type) : Color.white;

            GameObject go;
            if (m_cellPrefab != null)
            {
                go = Instantiate(m_cellPrefab, transform, false);
                go.name = $"Cell_{type}";
                go.transform.localPosition = localPos;
                go.transform.localScale = Vector3.one * m_cellSize;

                if (go.TryGetComponent<Renderer>(out var renderer))
                {
                    SetCellColor(renderer, color);
                }
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Cell_{type}";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = localPos;
                go.transform.localScale = Vector3.one * m_cellSize;

                Collider collider = go.GetComponent<Collider>();
                if (collider != null)
                    SafeDestroy(collider);

                Renderer renderer = go.GetComponent<Renderer>();

                SetCellColor(renderer, color);
            }

            return go;
        }

        protected override void SetCellVisualType(GameObject cell, TetrisBlockType type)
        {
            if (cell.TryGetComponent<Renderer>(out var renderer))
            {
                Color color = m_colorConfig != null ? m_colorConfig.GetColor(type) : Color.white;
                SetCellColor(renderer, color);
            }
        }

        protected override GameObject CreateBorderElement(bool isHorizontal, BorderCorner corner, Vector3 localPos)
        {
            GameObject prefab = corner switch
            {
                BorderCorner.BottomLeft  => m_borderCornerBottomLeftPrefab,
                BorderCorner.BottomRight => m_borderCornerBottomRightPrefab,
                BorderCorner.TopLeft     => m_borderCornerTopLeftPrefab,
                BorderCorner.TopRight    => m_borderCornerTopRightPrefab,
                _ => isHorizontal ? m_borderHorizontalPrefab : m_borderVerticalPrefab,
            };

            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab, transform, false);
                go.transform.localPosition = localPos;
                go.transform.localScale = Vector3.one * m_cellSize;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = corner != BorderCorner.None ? $"Border_Corner_{corner}" : isHorizontal ? "Border_H" : "Border_V";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = localPos;
                go.transform.localScale = Vector3.one * m_cellSize;

                Collider collider = go.GetComponent<Collider>();
                if (collider != null)
                    SafeDestroy(collider);

                Renderer renderer = go.GetComponent<Renderer>();

                SetCellColor(renderer, m_borderColor);
            }

            return go;
        }

        // ── 内部工具 ──────────────────────────────────────────────

        private void SetCellColor(Renderer renderer, Color color)
        {
            if (m_propertyBlock == null)
                m_propertyBlock = new MaterialPropertyBlock();

            renderer.GetPropertyBlock(m_propertyBlock);
            m_propertyBlock.SetColor(ColorId, color);
            m_propertyBlock.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(m_propertyBlock);
        }
    }
}
