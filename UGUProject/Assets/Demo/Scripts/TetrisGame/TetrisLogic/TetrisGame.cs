using UnityEngine;
using UnityEngine.InputSystem;

namespace ZNGTetris.Logic
{
    /// <summary>
    /// 俄罗斯方块游戏控制器（逻辑层核心）。
    /// 作为 MonoBehaviour 挂载到 GameObject，统筹棋盘、方块、碰撞、消行、生成等子系统。
    /// 通过 <see cref="ITetrisView"/> 接口与显示层通信，不直接操作 Transform / GameObject。
    /// </summary>
    public class TetrisGame : MonoBehaviour
    {
        // ── Inspector 字段 ────────────────────────────────────────

        [Header("棋盘配置")]
        [Tooltip("棋盘宽度（列数）")]
        [SerializeField] private int m_boardWidth = 10;

        [Tooltip("棋盘高度（行数）")]
        [SerializeField] private int m_boardHeight = 20;

        [Header("下落配置")]
        [Tooltip("基础下落间隔（秒）")]
        [SerializeField] private float m_baseFallInterval = 0.8f;

        [Tooltip("每升一级减少的下落间隔（秒）")]
        [SerializeField] private float m_levelSpeedStep = 0.05f;

        [Tooltip("最小下落间隔（秒）")]
        [SerializeField] private float m_minFallInterval = 0.05f;

        [Tooltip("每多少行升一级")]
        [SerializeField] private int m_linesPerLevel = 10;

        [Header("游戏流程")]
        [Tooltip("是否在 Start 时自动开始游戏")]
        [SerializeField] private bool m_autoStart;

        [Header("输入配置")]
        [Tooltip("左移输入动作（Left Arrow / A）")]
        [SerializeField] private InputAction m_moveLeftAction = new InputAction("Move Left", expectedControlType: "Button", binding: "<Keyboard>/leftArrow");

        [Tooltip("右移输入动作（Right Arrow / D）")]
        [SerializeField] private InputAction m_moveRightAction = new InputAction("Move Right", expectedControlType: "Button", binding: "<Keyboard>/rightArrow");

        [Tooltip("旋转输入动作（Up Arrow / W）")]
        [SerializeField] private InputAction m_rotateAction = new InputAction("Rotate", expectedControlType: "Button", binding: "<Keyboard>/upArrow");

        [Tooltip("软降输入动作（Down Arrow / S，按住持续触发）")]
        [SerializeField] private InputAction m_softDropAction = new InputAction("Soft Drop", expectedControlType: "Button", binding: "<Keyboard>/downArrow");

        [Tooltip("硬降输入动作（Space）")]
        [SerializeField] private InputAction m_hardDropAction = new InputAction("Hard Drop", expectedControlType: "Button", binding: "<Keyboard>/space");

        [Tooltip("开始/重新开始输入动作（Enter）")]
        [SerializeField] private InputAction m_startAction = new InputAction("Start", expectedControlType: "Button", binding: "<Keyboard>/enter");

        [Header("重复输入配置")]
        [Tooltip("DAS 延迟（秒）：按住方向键后多久开始自动重复")]
        [SerializeField] private float m_dasDelay = 0.15f;

        [Tooltip("ARR 速率（秒）：自动重复间隔")]
        [SerializeField] private float m_arrRate = 0.05f;

        [Tooltip("软降间隔（秒）：按住软降键时每次下落的间隔")]
        [SerializeField] private float m_softDropInterval = 0.05f;

        [Header("显示层")]
        [Tooltip("实现 ITetrisView 接口的 MonoBehaviour 组件")]
        [SerializeField] private MonoBehaviour m_viewComponent;

        [Header("Debug")]
        [Tooltip("游戏结束时在控制台打印棋盘状态（O=有元素, +=无元素）")]
        [SerializeField] private bool m_printBoardOnOver;

        // ── 私有字段（运行时状态）────────────────────────────────

        private TetrisBoard m_board;
        private TetrisPiece m_currentPiece;
        private TetrisCollision m_collision;
        private TetrisLineSystem m_lineSystem;
        private TetrisSpawner m_spawner;
        private ITetrisView m_view;

        private float m_fallTimer;
        private bool m_isGameOver;
        private bool m_isGameRunning;

        private float m_leftTimer;
        private float m_rightTimer;
        private float m_softDropTimer;
        private bool m_leftDASActive;
        private bool m_rightDASActive;

        // ── 私有常量 ─────────────────────────────────────────────

        /// <summary>消行得分表（索引 0~4 对应消除 0~4 行）</summary>
        private static readonly int[] ScoreTable = { 0, 40, 100, 300, 1200 };

        // ── 属性 ─────────────────────────────────────────────────

        /// <summary>棋盘宽度</summary>
        public int BoardWidth => m_boardWidth;

        /// <summary>棋盘高度</summary>
        public int BoardHeight => m_boardHeight;

        /// <summary>棋盘数据</summary>
        public TetrisBoard Board => m_board;

        /// <summary>当前下落方块</summary>
        public TetrisPiece CurrentPiece => m_currentPiece;

        /// <summary>下一个方块类型（用于预览）</summary>
        public TetrisBlockType NextPieceType => m_spawner != null ? m_spawner.NextType : TetrisBlockType.Empty;

        /// <summary>游戏是否结束</summary>
        public bool IsGameOver => m_isGameOver;

        /// <summary>游戏是否运行中</summary>
        public bool IsRunning => m_isGameRunning;

        /// <summary>当前分数</summary>
        public int Score { get; private set; }

        /// <summary>总消除行数</summary>
        public int Lines { get; private set; }

        /// <summary>当前等级</summary>
        public int Level { get; private set; }

        /// <summary>当前下落间隔（秒）</summary>
        public float FallInterval => Mathf.Max(m_minFallInterval, m_baseFallInterval - Level * m_levelSpeedStep);

        // ── 生命周期 ─────────────────────────────────────────────

        private void Awake()
        {
            if (m_viewComponent is ITetrisView view)
            {
                m_view = view;
            }
            else if (m_viewComponent != null)
            {
                Debug.LogError($"[TetrisGame] {m_viewComponent.GetType().Name} 未实现 ITetrisView 接口", this);
            }
        }

        private void Start()
        {
            if (m_autoStart)
            {
                StartGame();
            }
        }

        private void OnEnable()
        {
            m_moveLeftAction.Enable();
            m_moveRightAction.Enable();
            m_rotateAction.Enable();
            m_softDropAction.Enable();
            m_hardDropAction.Enable();
            m_startAction.Enable();
        }

        private void OnDisable()
        {
            m_moveLeftAction.Disable();
            m_moveRightAction.Disable();
            m_rotateAction.Disable();
            m_softDropAction.Disable();
            m_hardDropAction.Disable();
            m_startAction.Disable();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            // 设计文档要求的默认绑定：主键 + 副键
            m_moveLeftAction = CreateDefaultAction("Move Left", "<Keyboard>/leftArrow", "<Keyboard>/a");
            m_moveRightAction = CreateDefaultAction("Move Right", "<Keyboard>/rightArrow", "<Keyboard>/d");
            m_rotateAction = CreateDefaultAction("Rotate", "<Keyboard>/upArrow", "<Keyboard>/w");
            m_softDropAction = CreateDefaultAction("Soft Drop", "<Keyboard>/downArrow", "<Keyboard>/s");
            m_hardDropAction = new InputAction("Hard Drop", expectedControlType: "Button", binding: "<Keyboard>/space");
            m_startAction = new InputAction("Start", expectedControlType: "Button", binding: "<Keyboard>/enter");
        }

        private static InputAction CreateDefaultAction(string name, string primaryPath, string secondaryPath)
        {
            var action = new InputAction(name, expectedControlType: "Button", binding: primaryPath);
            action.AddBinding(secondaryPath);
            return action;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            UnityEditor.EditorApplication.delayCall -= DelayedPushPreviewSize;
            UnityEditor.EditorApplication.delayCall += DelayedPushPreviewSize;
        }

        private void DelayedPushPreviewSize()
        {
            if (this == null)
                return;

            if (m_viewComponent is ITetrisView view)
            {
                view.SetPreviewBoardSize(m_boardWidth, m_boardHeight);
            }
        }
#endif

        private void Update()
        {
            HandleStartInput();
            HandleMovementInput();
            HandleActionInput();

            if (!m_isGameRunning || m_isGameOver || m_currentPiece == null)
                return;

            m_fallTimer += Time.deltaTime;
            if (m_fallTimer >= FallInterval)
            {
                m_fallTimer = 0f;
                Fall();
            }
        }

        // ── 公共入口（游戏流程）──────────────────────────────────

        /// <summary>开始新游戏</summary>
        public void StartGame()
        {
            m_board = new TetrisBoard(m_boardWidth, m_boardHeight);
            m_collision = new TetrisCollision();
            m_lineSystem = new TetrisLineSystem();
            m_spawner = new TetrisSpawner(m_boardWidth, m_boardHeight);

            Score = 0;
            Lines = 0;
            Level = 0;
            m_fallTimer = 0f;
            m_isGameOver = false;
            m_isGameRunning = true;

            m_view?.OnGameStart(m_boardWidth, m_boardHeight);

            SpawnPiece();
        }

        /// <summary>设置显示层视图（代码方式注册）</summary>
        public void SetView(ITetrisView view)
        {
            m_view = view;
        }

        // ── 公共入口（玩家输入）──────────────────────────────────

        /// <summary>向左移动一格</summary>
        public void MoveLeft()
        {
            if (!CanAcceptInput())
                return;

            TryMove(new Vector2Int(-1, 0));
        }

        /// <summary>向右移动一格</summary>
        public void MoveRight()
        {
            if (!CanAcceptInput())
                return;

            TryMove(new Vector2Int(1, 0));
        }

        /// <summary>顺时针旋转</summary>
        public void Rotate()
        {
            if (!CanAcceptInput())
                return;

            TryRotate();
        }

        /// <summary>软降（加速下落一格）</summary>
        public void SoftDrop()
        {
            if (!CanAcceptInput())
                return;

            m_fallTimer = 0f;
            Fall();
        }

        /// <summary>硬降（直接落到底部并固定）</summary>
        public void HardDrop()
        {
            if (!CanAcceptInput())
                return;

            while (TryMove(Vector2Int.down))
            {
                // 持续下落直到无法移动
            }

            LockPiece();
        }

        // ── 逻辑方法（核心流程）──────────────────────────────────

        private void Fall()
        {
            if (!TryMove(Vector2Int.down))
            {
                LockPiece();
            }
        }

        private bool TryMove(Vector2Int direction)
        {
            Vector2Int targetPosition = m_currentPiece.Position + direction;
            Vector2Int[] cells = m_currentPiece.GetCells();

            if (!m_collision.IsValid(m_board, targetPosition, cells))
                return false;

            m_currentPiece.Position = targetPosition;
            m_view?.OnPieceMoved(m_currentPiece);
            return true;
        }

        private void TryRotate()
        {
            int nextRotation = (m_currentPiece.Rotation + 1) % TetrisShape.RotationCount;

            if (m_collision.TryRotateWithKick(m_board, m_currentPiece, nextRotation,
                    out Vector2Int outPosition, out int outRotation))
            {
                m_currentPiece.Position = outPosition;
                m_currentPiece.Rotation = outRotation;
                m_view?.OnPieceRotated(m_currentPiece);
            }
        }

        private void SpawnPiece()
        {
            m_currentPiece = m_spawner.Spawn();
            m_view?.OnPieceSpawned(m_currentPiece);
        }

        private void LockPiece()
        {
            // 方块部分在棋盘外（上方）→ 无法全部写入棋盘 → 游戏结束
            // 运算结束后，在棋盘内且不覆盖已写入位置的格子写入棋盘，在棋盘外的直接丢弃
            if (!IsPieceFullyInside())
            {
                Vector2Int[] cells = m_currentPiece.GetCells();
                for (int i = 0; i < cells.Length; i++)
                {
                    Vector2Int pos = m_currentPiece.Position + cells[i];
                    if (m_board.IsInside(pos.x, pos.y) && !m_board.IsOccupied(pos.x, pos.y))
                    {
                        m_board.SetCell(pos.x, pos.y, m_currentPiece.Type);
                    }
                }

                GameOver();
                return;
            }

            // 将当前方块写入棋盘
            Vector2Int[] lockCells = m_currentPiece.GetCells();
            for (int i = 0; i < lockCells.Length; i++)
            {
                Vector2Int pos = m_currentPiece.Position + lockCells[i];
                m_board.SetCell(pos.x, pos.y, m_currentPiece.Type);
            }

            m_view?.OnPieceLocked(m_currentPiece);

            // 检查消行
            int[] clearedRows = m_lineSystem.FindFullRows(m_board);
            if (clearedRows.Length > 0)
            {
                m_lineSystem.ClearSpecificRows(m_board, clearedRows);
                Lines += clearedRows.Length;
                Score += ScoreTable[Mathf.Min(clearedRows.Length, 4)] * (Level + 1);
                Level = Lines / m_linesPerLevel;
                m_view?.OnLinesCleared(clearedRows, m_board);
            }

            m_currentPiece = null;
            m_fallTimer = 0f;

            // 生成下一个方块
            SpawnPiece();
        }

        private void GameOver()
        {
            m_isGameOver = true;
            m_isGameRunning = false;

            TetrisPiece lastPiece = m_currentPiece;
            m_currentPiece = null;

            if (m_printBoardOnOver)
            {
                PrintBoardState(lastPiece);
            }

            m_view?.OnGameOver();
        }

        // ── 内部工具（输入处理）──────────────────────────────────

        private void HandleStartInput()
        {
            if (m_startAction.WasPressedThisFrame() && (m_isGameOver || !m_isGameRunning))
            {
                StartGame();
            }
        }

        private void HandleMovementInput()
        {
            HandleDirectionalInput(m_moveLeftAction, ref m_leftTimer, ref m_leftDASActive, MoveLeft);
            HandleDirectionalInput(m_moveRightAction, ref m_rightTimer, ref m_rightDASActive, MoveRight);
        }

        private void HandleActionInput()
        {
            if (!m_isGameRunning || m_isGameOver)
                return;

            if (m_rotateAction.WasPressedThisFrame())
            {
                Rotate();
            }

            if (m_hardDropAction.WasPressedThisFrame())
            {
                HardDrop();
            }

            if (m_softDropAction.IsPressed())
            {
                m_softDropTimer += Time.deltaTime;
                if (m_softDropTimer >= m_softDropInterval)
                {
                    SoftDrop();
                    m_softDropTimer = 0f;
                }
            }
            else
            {
                m_softDropTimer = 0f;
            }
        }

        /// <summary>
        /// 处理方向键输入：首次按下立即移动，按住超过 DAS 延迟后以 ARR 速率自动重复。
        /// </summary>
        private void HandleDirectionalInput(
            InputAction inputAction,
            ref float timer,
            ref bool dasActive,
            System.Action callback)
        {
            if (inputAction.WasPressedThisFrame())
            {
                callback.Invoke();
                timer = 0f;
                dasActive = false;
            }
            else if (inputAction.IsPressed())
            {
                timer += Time.deltaTime;

                if (!dasActive && timer >= m_dasDelay)
                {
                    dasActive = true;
                    timer = 0f;
                }

                if (dasActive && timer >= m_arrRate)
                {
                    callback.Invoke();
                    timer = 0f;
                }
            }
            else if (inputAction.WasReleasedThisFrame())
            {
                timer = 0f;
                dasActive = false;
            }
        }

        // ── 内部工具 ──────────────────────────────────────────────

        private bool CanAcceptInput()
        {
            return m_isGameRunning && !m_isGameOver && m_currentPiece != null && IsPiecePartiallyInside();
        }

        /// <summary>
        /// 判断当前方块是否有任意 Cell 在棋盘内（用于输入门控）。
        /// 方块在棋盘外生成，逐步向棋盘内移动，部分进入棋盘后才接受玩家输入。
        /// </summary>
        private bool IsPiecePartiallyInside()
        {
            if (m_currentPiece == null)
                return false;

            Vector2Int[] cells = m_currentPiece.GetCells();
            for (int i = 0; i < cells.Length; i++)
            {
                Vector2Int pos = m_currentPiece.Position + cells[i];
                if (m_board.IsInside(pos.x, pos.y))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断当前方块是否所有 Cell 都在棋盘内（用于游戏结束判定）。
        /// 方块无法全部写入棋盘时触发游戏结束。
        /// </summary>
        private bool IsPieceFullyInside()
        {
            if (m_currentPiece == null)
                return false;

            Vector2Int[] cells = m_currentPiece.GetCells();
            for (int i = 0; i < cells.Length; i++)
            {
                Vector2Int pos = m_currentPiece.Position + cells[i];
                if (!m_board.IsInside(pos.x, pos.y))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 在控制台打印棋盘最终状态（O=有元素, +=无元素），从顶部到底部逐行输出。
        /// 包含列号头和行号，行列格式化对齐。
        /// 在棋盘状态之外，用 O 和 + 打印最后一个方块的图案，并标注越界 Cells。
        /// </summary>
        private void PrintBoardState(TetrisPiece lastPiece)
        {
            if (m_board == null)
                return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("[TetrisGame] 棋盘最终状态:");

            // 行号最大显示宽度
            int rowLabelWidth = (m_board.Height - 1).ToString().Length;
            // 每列显示宽度（列号最大位数 + 1 空格）
            int colWidth = (m_board.Width - 1).ToString().Length + 1;

            // 打印列头
            sb.Append(' ', rowLabelWidth + 2);
            for (int x = 0; x < m_board.Width; x++)
            {
                sb.Append(x.ToString().PadLeft(colWidth));
            }

            sb.AppendLine();

            // 从顶部到底部逐行打印
            for (int y = m_board.Height - 1; y >= 0; y--)
            {
                sb.Append(y.ToString().PadLeft(rowLabelWidth)).Append(" |");
                for (int x = 0; x < m_board.Width; x++)
                {
                    char c = m_board.GetCell(x, y) != TetrisBlockType.Empty ? 'O' : '+';
                    sb.Append(c.ToString().PadLeft(colWidth));
                }

                sb.AppendLine();
            }

            // 在棋盘状态之外，用 O 和 + 打印最后一个方块的图案
            if (lastPiece != null)
            {
                Vector2Int[] cells = lastPiece.GetCells();

                // 计算方块局部坐标的边界范围
                int minX = int.MaxValue, maxX = int.MinValue;
                int minY = int.MaxValue, maxY = int.MinValue;
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i].x < minX) minX = cells[i].x;
                    if (cells[i].x > maxX) maxX = cells[i].x;
                    if (cells[i].y < minY) minY = cells[i].y;
                    if (cells[i].y > maxY) maxY = cells[i].y;
                }

                sb.AppendLine();
                sb.AppendLine("最后一个方块:");
                sb.AppendLine($"  类型: {lastPiece.Type}  位置: ({lastPiece.Position.x}, {lastPiece.Position.y})  旋转: {lastPiece.Rotation}");
                sb.AppendLine("  方块图案:");

                // 列号显示宽度
                int pieceColWidth = maxX.ToString().Length + 1;
                int pieceRowLabelWidth = maxY.ToString().Length;

                // 打印列头
                sb.Append("  ");
                sb.Append(' ', pieceRowLabelWidth + 2);
                for (int x = minX; x <= maxX; x++)
                {
                    sb.Append(x.ToString().PadLeft(pieceColWidth));
                }
                sb.AppendLine();

                // 从顶部到底部逐行打印图案
                for (int y = maxY; y >= minY; y--)
                {
                    sb.Append("  ");
                    sb.Append(y.ToString().PadLeft(pieceRowLabelWidth)).Append(" |");
                    for (int x = minX; x <= maxX; x++)
                    {
                        bool isOccupied = false;
                        for (int i = 0; i < cells.Length; i++)
                        {
                            if (cells[i].x == x && cells[i].y == y)
                            {
                                isOccupied = true;
                                break;
                            }
                        }
                        char c = isOccupied ? 'O' : '+';
                        sb.Append(c.ToString().PadLeft(pieceColWidth));
                    }
                    sb.AppendLine();
                }

                // 打印棋盘坐标及越界标记
                sb.Append("  棋盘坐标: ");
                for (int i = 0; i < cells.Length; i++)
                {
                    Vector2Int worldCell = lastPiece.Position + cells[i];
                    bool inBoard = m_board.IsInside(worldCell.x, worldCell.y);
                    sb.Append($"({worldCell.x},{worldCell.y}){(inBoard ? "" : "[越界]")}");
                    if (i < cells.Length - 1)
                        sb.Append(" ");
                }
                sb.AppendLine();
            }

            Debug.Log(sb.ToString(), this);
        }
    }
}
