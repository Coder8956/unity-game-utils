using UnityEngine;
using UnityEngine.UI;
using ZNGTetris.Logic;

namespace ZNGTetris.View
{
    /// <summary>
    /// UI 显示层，使用 Canvas 下的 <see cref="Image"/> 组件渲染方块格子。
    /// 挂载到 Canvas 子物体上，并赋值给 TetrisGame 的 m_viewComponent。
    /// </summary>
    public class TetrisUIView : TetrisViewBase
    {
        // ── Inspector 字段 ────────────────────────────────────────

        [Header("UI 配置")]
        [Tooltip("格子精灵（为空时使用纯色 Image）")]
        [SerializeField] private Sprite m_cellSprite;

        // ── 保护方法（实现基类抽象）──────────────────────────────

        protected override GameObject CreateCellVisual(TetrisBlockType type, Vector3 localPos, bool isPiece)
        {
            GameObject go = new GameObject($"Cell_{type}");
            SetupRectTransform(go, localPos);

            Image image = go.AddComponent<Image>();
            image.sprite = m_cellSprite;
            image.color = m_colorConfig != null ? m_colorConfig.GetColor(type) : Color.white;

            return go;
        }

        protected override void SetCellVisualType(GameObject cell, TetrisBlockType type)
        {
            if (cell.TryGetComponent<Image>(out var image))
            {
                image.color = m_colorConfig != null ? m_colorConfig.GetColor(type) : Color.white;
            }
        }

        protected override GameObject CreateBorderElement(bool isHorizontal, bool isCorner, Vector3 localPos)
        {
            GameObject prefab = isCorner ? m_borderCornerPrefab :
                                 isHorizontal ? m_borderHorizontalPrefab :
                                 m_borderVerticalPrefab;

            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab, transform, false);
                SetupRectTransform(go, localPos);
            }
            else
            {
                go = new GameObject(isCorner ? "Border_Corner" : isHorizontal ? "Border_H" : "Border_V");
                SetupRectTransform(go, localPos);

                Image image = go.AddComponent<Image>();
                image.sprite = m_cellSprite;
                image.color = m_borderColor;
            }

            return go;
        }

        // ── 内部工具 ──────────────────────────────────────────────

        /// <summary>
        /// 配置 RectTransform 的锚点、轴心、尺寸和位置。
        /// 锚点设为中心 (0.5, 0.5)，使 anchoredPosition 与 BoardToWorld 的居中坐标一致。
        /// </summary>
        private void SetupRectTransform(GameObject go, Vector3 localPos)
        {
            go.transform.SetParent(transform, false);

            RectTransform rt = go.transform as RectTransform;
            if (rt == null)
            {
                // new GameObject 默认只有 Transform，需要替换为 RectTransform
                rt = go.AddComponent<RectTransform>();
            }

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.one * m_cellSize;
            rt.anchoredPosition = new Vector2(localPos.x, localPos.y);
        }
    }
}
