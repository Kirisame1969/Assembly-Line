using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemMovement : MonoBehaviour
{
    private GridManager gridManager;
    private Vector2Int currentGridPos;      // 当前所在的逻辑网格坐标
    private Vector2Int targetGridPos;       // 正在前往的目标网格坐标
    private float moveProgress;              // 移动进度 (0~1)
    private float currentSpeed;              // 当前实际移动速度（格子/秒），从线路获取
    private bool isInitialized = false;

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
            Debug.LogError("ItemMovement: 找不到GridManager！");
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
        // 每tick更新速度（线路速度可能变化）
        UpdateSpeedFromLine();

        if (targetGridPos != currentGridPos)
        {
            // 正在移动
            if (currentSpeed > 0f)
            {
                moveProgress += currentSpeed * Time.fixedDeltaTime; // 速度 * 每tick时间
                if (moveProgress >= 1f)
                {
                    // 到达目标格子
                    moveProgress = 1f;
                    // 更新逻辑位置
                    currentGridPos = targetGridPos;
                    targetGridPos = currentGridPos; // 暂时设相同，表示到达
                    // 精确对齐格子中心
                    transform.position = gridManager.GridToWorld(currentGridPos);

                    // 到达后处理（检查是否继续移动）
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
            // 确保完全对齐
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
                currentSpeed = Mathf.Abs(belt.line.speed); // 取绝对值用于进度计算
            }
            else
            {
                currentSpeed = 0f;
            }
        }
        else
        {
            currentSpeed = 0f;
        }
    }

    // 尝试开始向当前格子传送带的方向移动
    private void TryStartMoving()
    {
        GameObject cellObj = gridManager.GetObjectAt(currentGridPos);
        if (cellObj == null) return;
        ConveyorBelt belt = cellObj.GetComponent<ConveyorBelt>();
        if (belt == null) return;
        if (belt.line == null || !belt.line.isOperational) return;
        
        Debug.Log($"[TryStartMoving] 物品在格子 {currentGridPos}，线路ID: {belt.line.id}，速度: {currentSpeed}");

        Vector2Int dir = belt.GetMovementDirection(); // 获取实际方向（考虑符号）
        Vector2Int nextGrid = currentGridPos + dir;

        


        if (!gridManager.IsWithinBounds(nextGrid))
        {
            Debug.Log("  下一格超出边界");
            return;
        }
        GameObject nextObj = gridManager.GetObjectAt(nextGrid);
        if (nextObj == null)
        {
            Debug.Log("  下一格无建筑");
            return;
        }
        ConveyorBelt nextBelt = nextObj.GetComponent<ConveyorBelt>();
        if (nextBelt == null)
        {
            Debug.Log("  下一格不是传送带");
            return;
        }
        if (nextBelt.line != belt.line)
        {
            Debug.Log($"  线路不一致：当前线路 {belt.line.id}，下一格线路 {(nextBelt.line == null ? "null" : nextBelt.line.id.ToString())}");
            return;
        }
        if (!nextBelt.line.isOperational)
        {
            Debug.Log("  下一格线路不工作");
            return;
        }
        if (nextBelt.HasItem())
        {
            Debug.Log("  下一格已有物品");
            return;
        }

        Debug.Log("  开始移动！");
        targetGridPos = nextGrid;
        moveProgress = 0f;
        belt.SetItem(null);
        nextBelt.SetItem(this.gameObject);
}

    private void OnArriveAtTarget()
    {
        // 到达后，下一次 FixedUpdate 会自动尝试继续移动
        // 可以在此添加额外效果，如触发到达事件
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