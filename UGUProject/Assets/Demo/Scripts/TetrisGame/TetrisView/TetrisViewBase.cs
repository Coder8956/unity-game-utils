using UnityEngine;
using ZNGTetris.Logic;

namespace ZNGTetris.View
{
    /// <summary>
    /// 显示层基类，实现 <see cref="ITetrisView"/> 接口的核心逻辑。
    /// 管理棋盘格子视觉和当前下落方块视觉，由 2D/3D 子类提供具体的渲染方式。
    /// 使用 <see cref="ExecuteAlways"/> 支持编辑模式下边框实际效果预览。
    /// </summary>
    [ExecuteAlways]
    public abstract class TetrisViewBase : MonoBehaviour, ITetrisView
    {
        // ── Inspector 字段 ────────────────────────────────────────

        [Header("显示配置")]
        [Tooltip("每个格子的世界尺寸")]
        [SerializeField] protected float m_cellSize = 1f;

        [Tooltip("方块颜色配置")]
        [SerializeField] protected TetrisBlockColorConfig m_colorConfig;

        [Header("边框配置")]
        [Tooltip("是否显示游戏区域边框")]
        [SerializeField] protected bool m_showBorder = true;

        [Tooltip("边框颜色")]
        [SerializeField] protected Color m_borderColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        [Tooltip("水平边框元素预制体（为空时使用默认单位元素）")]
        [SerializeField] protected GameObject m_borderHorizontalPrefab;

        [Tooltip("竖直边框元素预制体（为空时使用默认单位元素）")]
        [SerializeField] protected GameObject m_borderVerticalPrefab;

        [Tooltip("拐角边框元素预制体（为空时使用默认单位元素）")]
        [SerializeField] protected GameObject m_borderCornerPrefab;

        [Header("预览配置")]
        [Tooltip("是否在编辑模式下开启边框预览（Scene 和 Game 窗口均可见）")]
        [SerializeField] protected bool m_showBorderPreview = true;

        // ── 私有字段（运行时状态）────────────────────────────────

        protected int m_boardWidth;
        protected int m_boardHeight;
        protected GameObject[,] m_boardCells;
        protected GameObject[] m_pieceCells = new GameObject[4];
        protected GameObject[] m_borderElements;
        protected bool m_initialized;

        private int m_previewWidth;
        private int m_previewHeight;

        // ── 属性 ─────────────────────────────────────────────────

        /// <summary>棋盘宽度</summary>
        public int BoardWidth => m_boardWidth;

        /// <summary>棋盘高度</summary>
        public int BoardHeight => m_boardHeight;

        // ── 内部工具 ──────────────────────────────────────────────

        /// <summary>
        /// 销毁对象，编辑模式下使用 DestroyImmediate，运行时使用 Destroy。
        /// </summary>
        protected static void SafeDestroy(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        // ── 生命周期 ─────────────────────────────────────────────

        protected virtual void OnDestroy()
        {
            ClearAll();
        }

        protected virtual void OnEnable()
        {
            if (!Application.isPlaying)
                RefreshBorderPreview();
        }

        protected virtual void OnDisable()
        {
            if (!Application.isPlaying)
                ClearBorder();
        }

        // ── ITetrisView 实现 ────────────────────────────────────

        public void OnGameStart(int boardWidth, int boardHeight)
        {
            ClearAll();

            m_boardWidth = boardWidth;
            m_boardHeight = boardHeight;
            m_boardCells = new GameObject[boardWidth, boardHeight];
            m_pieceCells = new GameObject[4];
            m_initialized = true;

            BuildBorder(boardWidth, boardHeight);
        }

        public void OnPieceSpawned(TetrisPiece piece)
        {
            UpdatePieceVisuals(piece);
        }

        public void OnPieceMoved(TetrisPiece piece)
        {
            UpdatePieceVisuals(piece);
        }

        public void OnPieceRotated(TetrisPiece piece)
        {
            UpdatePieceVisuals(piece);
        }

        public void OnPieceLocked(TetrisPiece piece)
        {
            HidePieceVisuals();

            Vector2Int[] worldCells = piece.GetWorldCells();
            for (int i = 0; i < worldCells.Length; i++)
            {
                Vector2Int cell = worldCells[i];
                if (cell.x >= 0 && cell.x < m_boardWidth && cell.y >= 0 && cell.y < m_boardHeight)
                {
                    if (m_boardCells[cell.x, cell.y] == null)
                    {
                        m_boardCells[cell.x, cell.y] = CreateCellVisual(
                            piece.Type, BoardToWorld(cell.x, cell.y), false);
                    }
                    else
                    {
                        SetCellVisualType(m_boardCells[cell.x, cell.y], piece.Type);
                        m_boardCells[cell.x, cell.y].SetActive(true);
                    }
                }
            }
        }

        public void OnLinesCleared(int[] clearedRows, TetrisBoard board)
        {
            RebuildBoardVisuals(board);
        }

        public virtual void OnGameOver()
        {
            // 子类可重写以添加游戏结束视觉效果
        }

        /// <summary>
        /// 设置编辑模式预览的棋盘尺寸（由 TetrisGame 调用）。
        /// </summary>
        public void SetPreviewBoardSize(int width, int height)
        {
            m_previewWidth = width;
            m_previewHeight = height;
            RefreshBorderPreview();
        }

        // ── 保护方法 ─────────────────────────────────────────────

        /// <summary>
        /// 将棋盘坐标转换为相对于本 Transform 的局部坐标。
        /// 棋盘居中于原点，(0,0) 在左下角。
        /// </summary>
        protected virtual Vector3 BoardToWorld(int x, int y)
        {
            return BoardToWorld(x, y, m_boardWidth, m_boardHeight);
        }

        /// <summary>
        /// 将棋盘坐标转换为相对于本 Transform 的局部坐标（使用指定棋盘尺寸）。
        /// 棋盘居中于原点，(0,0) 在左下角。
        /// </summary>
        protected Vector3 BoardToWorld(int x, int y, int width, int height)
        {
            float offsetX = -(width - 1) * m_cellSize * 0.5f;
            float offsetY = -(height - 1) * m_cellSize * 0.5f;
            return new Vector3(
                offsetX + x * m_cellSize,
                offsetY + y * m_cellSize,
                0f
            );
        }

        /// <summary>
        /// 创建格子视觉对象（由子类实现具体的渲染方式）。
        /// </summary>
        /// <param name="isPiece">true 表示当前下落方块，false 表示已固定棋盘格子</param>
        protected abstract GameObject CreateCellVisual(TetrisBlockType type, Vector3 localPos, bool isPiece);

        /// <summary>
        /// 更新已有格子视觉对象的方块类型（颜色等）。
        /// </summary>
        protected abstract void SetCellVisualType(GameObject cell, TetrisBlockType type);

        // ── 边框方法 ─────────────────────────────────────────────

        /// <summary>
        /// 创建边框元素视觉对象（由子类实现具体的渲染方式）。
        /// </summary>
        /// <param name="isHorizontal">true 表示水平方向（上下边），false 表示竖直方向（左右边）</param>
        /// <param name="isCorner">true 表示拐角元素</param>
        protected virtual GameObject CreateBorderElement(bool isHorizontal, bool isCorner, Vector3 localPos)
        {
            return null;
        }

        /// <summary>
        /// 根据棋盘尺寸构建边框元素。
        /// 边框由水平元素（上下边）、竖直元素（左右边）和四个拐角元素组成。
        /// </summary>
        protected void BuildBorder(int width, int height)
        {
            ClearBorder();

            if (!m_showBorder || width <= 0 || height <= 0)
                return;

            // 水平: 上边 width + 下边 width
            // 竖直: 左边 height + 右边 height
            // 拐角: 4 个
            int count = 2 * width + 2 * height + 4;
            m_borderElements = new GameObject[count];

            int index = 0;

            // 下边水平元素 (y = -1)
            for (int x = 0; x < width; x++)
                m_borderElements[index++] = CreateBorderElement(true, false, BoardToWorld(x, -1, width, height));

            // 上边水平元素 (y = height)
            for (int x = 0; x < width; x++)
                m_borderElements[index++] = CreateBorderElement(true, false, BoardToWorld(x, height, width, height));

            // 左边竖直元素 (x = -1)
            for (int y = 0; y < height; y++)
                m_borderElements[index++] = CreateBorderElement(false, false, BoardToWorld(-1, y, width, height));

            // 右边竖直元素 (x = width)
            for (int y = 0; y < height; y++)
                m_borderElements[index++] = CreateBorderElement(false, false, BoardToWorld(width, y, width, height));

            // 四个拐角
            m_borderElements[index++] = CreateBorderElement(false, true, BoardToWorld(-1, -1, width, height));
            m_borderElements[index++] = CreateBorderElement(false, true, BoardToWorld(width, -1, width, height));
            m_borderElements[index++] = CreateBorderElement(false, true, BoardToWorld(-1, height, width, height));
            m_borderElements[index++] = CreateBorderElement(false, true, BoardToWorld(width, height, width, height));
        }

        /// <summary>
        /// 清除所有边框元素。
        /// </summary>
        protected void ClearBorder()
        {
            if (m_borderElements == null)
                return;

            for (int i = 0; i < m_borderElements.Length; i++)
            {
                if (m_borderElements[i] != null)
                    SafeDestroy(m_borderElements[i]);
            }

            m_borderElements = null;
        }

        // ── 编辑器预览 ───────────────────────────────────────────

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
                return;

            UnityEditor.EditorApplication.delayCall -= RefreshBorderPreview;
            UnityEditor.EditorApplication.delayCall += RefreshBorderPreview;
        }
#endif

        /// <summary>
        /// 刷新编辑模式下的边框预览。
        /// 在非运行状态且未初始化时，根据 m_showBorderPreview 和已接收的棋盘尺寸创建或清除实际边框 GameObject。
        /// 棋盘尺寸由 TetrisGame 通过 SetPreviewBoardSize 推送，显示层不主动查找逻辑类。
        /// 预览对象设置 <see cref="HideAndDontSave"/> 标记，不会写入场景文件。
        /// </summary>
        protected void RefreshBorderPreview()
        {
            if (Application.isPlaying || m_initialized)
                return;

            ClearBorder();

            if (!m_showBorderPreview || m_previewWidth <= 0 || m_previewHeight <= 0)
                return;

            BuildBorder(m_previewWidth, m_previewHeight);

            if (m_borderElements != null)
            {
                for (int i = 0; i < m_borderElements.Length; i++)
                {
                    if (m_borderElements[i] != null)
                        m_borderElements[i].hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }

        // ── 内部工具 ──────────────────────────────────────────────

        protected void UpdatePieceVisuals(TetrisPiece piece)
        {
            if (!m_initialized)
                return;

            Vector2Int[] cells = piece.GetCells();
            for (int i = 0; i < m_pieceCells.Length; i++)
            {
                if (i < cells.Length)
                {
                    Vector2Int worldCell = piece.Position + cells[i];
                    Vector3 localPos = BoardToWorld(worldCell.x, worldCell.y);

                    if (m_pieceCells[i] == null)
                    {
                        m_pieceCells[i] = CreateCellVisual(piece.Type, localPos, true);
                    }
                    else
                    {
                        m_pieceCells[i].transform.localPosition = localPos;
                        SetCellVisualType(m_pieceCells[i], piece.Type);
                        m_pieceCells[i].SetActive(true);
                    }
                }
                else
                {
                    if (m_pieceCells[i] != null)
                        m_pieceCells[i].SetActive(false);
                }
            }
        }

        protected void HidePieceVisuals()
        {
            for (int i = 0; i < m_pieceCells.Length; i++)
            {
                if (m_pieceCells[i] != null)
                    m_pieceCells[i].SetActive(false);
            }
        }

        protected void RebuildBoardVisuals(TetrisBoard board)
        {
            for (int x = 0; x < m_boardWidth; x++)
            {
                for (int y = 0; y < m_boardHeight; y++)
                {
                    TetrisBlockType type = board.GetCell(x, y);
                    if (type != TetrisBlockType.Empty)
                    {
                        if (m_boardCells[x, y] == null)
                        {
                            m_boardCells[x, y] = CreateCellVisual(type, BoardToWorld(x, y), false);
                        }
                        else
                        {
                            SetCellVisualType(m_boardCells[x, y], type);
                            m_boardCells[x, y].SetActive(true);
                        }
                    }
                    else
                    {
                        if (m_boardCells[x, y] != null)
                            m_boardCells[x, y].SetActive(false);
                    }
                }
            }
        }

        protected void ClearAll()
        {
            ClearBorder();

            if (m_pieceCells != null)
            {
                for (int i = 0; i < m_pieceCells.Length; i++)
                {
                    if (m_pieceCells[i] != null)
                        SafeDestroy(m_pieceCells[i]);
                    m_pieceCells[i] = null;
                }
            }

            if (m_boardCells != null)
            {
                for (int x = 0; x < m_boardCells.GetLength(0); x++)
                {
                    for (int y = 0; y < m_boardCells.GetLength(1); y++)
                    {
                        if (m_boardCells[x, y] != null)
                            SafeDestroy(m_boardCells[x, y]);
                    }
                }
            }

            m_boardCells = null;
            m_initialized = false;
        }
    }
}
