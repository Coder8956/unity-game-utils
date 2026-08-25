using UnityEngine;
using ZNGTetris.Logic;

namespace ZNGTetris.View
{
    /// <summary>
    /// 2D 显示层，使用 <see cref="SpriteRenderer"/> 渲染方块格子。
    /// 通过 <see cref="TetrisViewBase.m_cellSize"/> 配置单位元素边长，
    /// 自动根据精灵原生尺寸计算缩放，使渲染后的世界尺寸精确等于配置值。
    /// 挂载到场景中的 GameObject 上，并赋值给 TetrisGame 的 m_viewComponent。
    /// </summary>
    public class Tetris2DView : TetrisViewBase
    {
        // ── Inspector 字段 ────────────────────────────────────────

        [Header("2D 配置")]
        [Tooltip("格子精灵（为空时使用默认白色方块）")]
        [SerializeField] private Sprite m_cellSprite;

        [Tooltip("下落方块的 SortingOrder（确保显示在棋盘格子上方）")]
        [SerializeField] private int m_pieceSortingOrder = 1;

        [Tooltip("棋盘格子的 SortingOrder")]
        [SerializeField] private int m_boardSortingOrder = 0;

        // ── 私有静态字段 ──────────────────────────────────────────

        private static Sprite DefaultSprite;

        // ── 保护方法（实现基类抽象）──────────────────────────────

        protected override GameObject CreateCellVisual(TetrisBlockType type, Vector3 localPos, bool isPiece)
        {
            Color color = m_colorConfig != null ? m_colorConfig.GetColor(type) : Color.white;
            int sortingOrder = isPiece ? m_pieceSortingOrder : m_boardSortingOrder;
            return CreateSpriteGameObject($"Cell_{type}", localPos, color, sortingOrder);
        }

        protected override void SetCellVisualType(GameObject cell, TetrisBlockType type)
        {
            if (cell.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.color = m_colorConfig != null ? m_colorConfig.GetColor(type) : Color.white;
            }
        }

        protected override GameObject CreateBorderElement(bool isHorizontal, bool isCorner, Vector3 localPos)
        {
            GameObject prefab = isCorner ? m_borderCornerPrefab :
                                 isHorizontal ? m_borderHorizontalPrefab :
                                 m_borderVerticalPrefab;

            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, transform, false);
                go.transform.localPosition = localPos;
                go.transform.localScale = Vector3.one * m_cellSize;
                return go;
            }

            string name = isCorner ? "Border_Corner" : isHorizontal ? "Border_H" : "Border_V";
            return CreateSpriteGameObject(name, localPos, m_borderColor, m_boardSortingOrder);
        }

        // ── 内部工具 ──────────────────────────────────────────────

        /// <summary>
        /// 获取当前使用的精灵，m_cellSprite 为空时返回缓存的默认白色精灵。
        /// </summary>
        private Sprite GetCellSprite()
        {
            if (m_cellSprite != null)
                return m_cellSprite;

            if (DefaultSprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                DefaultSprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    tex.width);
                DefaultSprite.name = "DefaultWhite";
            }

            return DefaultSprite;
        }

        /// <summary>
        /// 根据精灵原生尺寸计算缩放，使渲染后的世界尺寸等于 m_cellSize × m_cellSize。
        /// </summary>
        private Vector3 GetCellScale()
        {
            Vector3 size = GetCellSprite().bounds.size;
            return new Vector3(
                size.x > 0f ? m_cellSize / size.x : m_cellSize,
                size.y > 0f ? m_cellSize / size.y : m_cellSize,
                1f);
        }

        /// <summary>
        /// 创建带 SpriteRenderer 的 GameObject，使用配置的精灵和缩放。
        /// 游戏元素和默认边框元素共用此方法。
        /// </summary>
        private GameObject CreateSpriteGameObject(string name, Vector3 localPos, Color color, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = GetCellScale();

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCellSprite();
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            return go;
        }
    }
}
