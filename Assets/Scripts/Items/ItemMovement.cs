using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemMovement : MonoBehaviour
{
    public float defaultSpeed = 1f;             // 默认速度（格子/秒），当无线路时使用（可选）

    private GridManager gridManager;
    private Vector2Int currentGridPos;          // 当前所在的逻辑网格坐标
    private Vector2Int targetGridPos;           // 正在前往的目标网格坐标
    private float moveProgress;                  // 移动进度 (0~1)
    private float currentMoveSpeed;              // 当前实际移动速度（格子/秒），从线路获取
    private bool isInitialized = false;

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("ItemMovement: 找不到GridManager！");
        }
    }

    void Start()
    {
        if (!isInitialized)
        {
            // 根据当前世界坐标初始化
            currentGridPos = gridManager.WorldToGrid(transform.position);
            targetGridPos = currentGridPos;
            transform.position = gridManager.GridToWorld(currentGridPos);
            moveProgress = 0f;
            UpdateSpeedFromLine();
        }
    }

    void FixedUpdate()
    {
        // 每 tick 更新速度（线路速度可能变化）
        UpdateSpeedFromLine();

        if (targetGridPos != currentGridPos)
        {
            // 正在移动
            if (currentMoveSpeed > 0f)
            {
                // 进度增加：每 tick 移动的距离 = 速度 * 固定时间步长（格子/秒 * 秒/tick）
                moveProgress += currentMoveSpeed * Time.fixedDeltaTime;
                if (moveProgress >= 1f)
                {
                    // 到达目标格子
                    moveProgress = 1f;
                    // 更新逻辑位置
                    currentGridPos = targetGridPos;
                    targetGridPos = currentGridPos; // 临时设相同，表示到达
                    // 确保位置精确对齐格子中心（避免浮点误差）
                    transform.position = gridManager.GridToWorld(currentGridPos);

                    // 到达后处理（例如检查是否继续移动）
                    OnArriveAtTarget();
                }
            }
            // 如果速度=0，进度不变，物品停在半路
        }
        else
        {
            // 静止，尝试开始移动
            TryStartMoving();
        }
    }

    void Update()
    {
        // 视觉平滑：根据逻辑位置和进度插值
        if (targetGridPos != currentGridPos)
        {
            Vector3 startPos = gridManager.GridToWorld(currentGridPos);
            Vector3 endPos = gridManager.GridToWorld(targetGridPos);
            transform.position = Vector3.Lerp(startPos, endPos, moveProgress);
        }
        else
        {
            // 确保完全对齐（可能因速度0而停在半路，但逻辑位置相同，这里强制对齐）
            transform.position = gridManager.GridToWorld(currentGridPos);
        }
    }

    // 从当前格子所在线路更新移动速度
    private void UpdateSpeedFromLine()
    {
        GameObject cellObj = gridManager.GetObjectAt(currentGridPos);
        if (cellObj != null)
        {
            ConveyorBelt belt = cellObj.GetComponent<ConveyorBelt>();
            if (belt != null && belt.line != null && belt.line.isOperational)
            {
                currentMoveSpeed = belt.line.speed;
            }
            else
            {
                currentMoveSpeed = 0f;
            }
        }
        else
        {
            currentMoveSpeed = 0f;
        }
    }

    // 尝试开始向当前格子传送带的方向移动
    private void TryStartMoving()
    {
        // 获取当前格子上的传送带
        GameObject cellObj = gridManager.GetObjectAt(currentGridPos);
        if (cellObj == null) return;
        ConveyorBelt belt = cellObj.GetComponent<ConveyorBelt>();
        if (belt == null) return;
        if (belt.line == null || !belt.line.isOperational) return; // 线路不工作

        // 获取移动方向（当前从传送带的固定方向获取，后续可由电机决定）
        Vector2Int dir = GetDirectionVector(belt.fixedDirection);
        Vector2Int nextGrid = currentGridPos + dir;

        // 检查下一个格子是否有效
        if (!gridManager.IsWithinBounds(nextGrid)) return;
        GameObject nextObj = gridManager.GetObjectAt(nextGrid);
        if (nextObj == null) return;
        ConveyorBelt nextBelt = nextObj.GetComponent<ConveyorBelt>();
        if (nextBelt == null) return;
        // 必须属于同一线路且线路工作
        if (nextBelt.line != belt.line) return;
        if (!nextBelt.line.isOperational) return;

        // 检查目标格子是否有物品
        if (nextBelt.HasItem()) return;

        // 开始移动
        targetGridPos = nextGrid;
        moveProgress = 0f;

        // 更新传送带引用
        belt.SetItem(null);
        nextBelt.SetItem(this.gameObject);
    }

    private void OnArriveAtTarget()
    {
        // 到达后，下一次 FixedUpdate 会自动尝试继续移动
        // 可以在这里添加额外效果，如触发到达事件
    }

    // 由外部调用（如 ItemPlacer）设置物品的初始网格
    public void SetCurrentGrid(Vector2Int gridPos)
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        currentGridPos = gridPos;
        targetGridPos = gridPos;
        transform.position = gridManager.GridToWorld(gridPos);
        moveProgress = 0f;
        isInitialized = true;
        UpdateSpeedFromLine();
    }

    // 方向转换辅助
    private Vector2Int GetDirectionVector(ConveyorBelt.Direction dir)
    {
        switch (dir)
        {
            case ConveyorBelt.Direction.Up:    return Vector2Int.up;
            case ConveyorBelt.Direction.Down:  return Vector2Int.down;
            case ConveyorBelt.Direction.Left:  return Vector2Int.left;
            case ConveyorBelt.Direction.Right: return Vector2Int.right;
            default: return Vector2Int.right;
        }
    }
}