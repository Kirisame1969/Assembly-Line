using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }
    public Direction fixedDirection;    // 铺设时的固定方向
    public ConveyorLine line;            // 所属线路（可能为null）

    private GameObject currentItem; // 当前格子上的物品
    private GridManager gridManager;
    private Vector2Int gridPos;

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
            Debug.LogError("ConveyorBelt: 找不到GridManager！");
    }

    void Start()
    {
        
        //gridManager = FindObjectOfType<GridManager>();
        //获取自己的网格坐标
        gridPos = gridManager.WorldToGrid(transform.position);
    }

    void Update()
    {
        // 如果当前有物品，并且物品处于静止状态（即isMoving为false），则通知物品开始移动
        if (currentItem != null)
        {
            ItemMovement itemMove = currentItem.GetComponent<ItemMovement>();
            if (itemMove != null)
            {
                // 检查物品是否到达中心并等待
                // 我们可以让物品自己处理移动，传送带只需在物品刚进入时触发一次
                // 但为了简单，我们可以每帧检查物品是否在中心且未移动，然后让它开始移动
                // 这里我们采用在物品进入时主动启动的方式（在SetItem中启动一次）
            }
        }
    }

    // 当物品进入此传送带时调用
    public void SetItem(GameObject item)
    {
        currentItem = item;
        /*
        if (item != null)
        {
            // 通知物品设置当前位置，并启动移动
            ItemMovement itemMove = item.GetComponent<ItemMovement>();
            if (itemMove != null)
            {
                itemMove.SetCurrentGrid(gridPos);
                // 让物品开始移动
                itemMove.StartMovingInDirection(GetDirectionVector());
            }
        }
        */
    }

    public bool HasItem()
    {
        return currentItem != null;
    }

    public Vector2Int GetDirectionVector()
    {
        switch (fixedDirection)
        {
            case Direction.Up:    return Vector2Int.up;
            case Direction.Down:  return Vector2Int.down;
            case Direction.Left:  return Vector2Int.left;
            case Direction.Right: return Vector2Int.right;
            default: return Vector2Int.right;
        }
    }

    // 可选：在编辑器中显示方向箭头（方便调试）
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector2Int dir2D = GetDirectionVector();
        Vector3 dir = new Vector3(dir2D.x, dir2D.y, 0);
        Gizmos.DrawRay(transform.position, dir * 0.5f);
    }

        // 在类中添加 using System.Linq; 如果还没有

    public void TryJoinLine()
    {
        Debug.Log($"[TryJoinLine] 传送带 {name} 尝试加入线路，当前线路: {(line == null ? "null" : line.id.ToString())}");

        if (line != null) return;

        Vector2Int gridPos = gridManager.WorldToGrid(transform.position);
        Direction dir = fixedDirection;
        Vector2Int[] offsets = GetParallelOffsets(dir);

        ConveyorLine leftLine = null;  // 负方向（左/下）
        ConveyorLine rightLine = null; // 正方向（右/上）
        Vector2Int? leftOffset = null;
        Vector2Int? rightOffset = null;

        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = gridPos + offset;
            if (!gridManager.IsWithinBounds(neighborPos)) continue;
            GameObject neighborObj = gridManager.GetObjectAt(neighborPos);
            if (neighborObj == null) continue;
            ConveyorBelt neighborBelt = neighborObj.GetComponent<ConveyorBelt>();
            if (neighborBelt == null || neighborBelt.fixedDirection != dir) continue;

            if (neighborBelt.line != null)
            {
                if (offset.x < 0 || offset.y < 0) // 左或下
                {
                    leftLine = neighborBelt.line;
                    leftOffset = offset;
                }
                else if (offset.x > 0 || offset.y > 0) // 右或上
                {
                    rightLine = neighborBelt.line;
                    rightOffset = offset;
                }
            }
        }

        if (leftLine == null && rightLine == null)
        {
            Debug.Log("  无相邻线路，不加入");
        }
        else if (leftLine != null && rightLine == null)
        {
            Debug.Log($"  左侧有线路 {leftLine.id}，加入并向右侧扩展");
            leftLine.AddBelt(this);
            ExtendLine(leftLine, gridPos, -leftOffset.Value); // 向相反方向扩展
        }
        else if (leftLine == null && rightLine != null)
        {
            Debug.Log($"  右侧有线路 {rightLine.id}，加入并向左侧扩展");
            rightLine.AddBelt(this);
            ExtendLine(rightLine, gridPos, -rightOffset.Value);
        }
        else // 两侧都有线路
        {
            if (leftLine == rightLine)
            {
                Debug.Log($"  两侧是同一线路 {leftLine.id}，填补空缺");
                leftLine.AddBelt(this);
                // 不需要扩展，因为已经是完整线路
            }
            else
            {
                Debug.Log($"  两侧不同线路 {leftLine.id} 和 {rightLine.id}，准备合并");
                ConveyorLine merged = ConveyorLine.Merge(leftLine, rightLine);
                merged.AddBelt(this);
                // 合并后，新线路已包含所有传送带，无需扩展
            }
        }
    }

    // 向指定方向扩展线路，将所有同向且无线路的传送带加入 line
    private void ExtendLine(ConveyorLine line, Vector2Int startPos, Vector2Int direction)
    {
        Vector2Int pos = startPos + direction;
        while (true)
        {
            if (!gridManager.IsWithinBounds(pos)) break;
            GameObject obj = gridManager.GetObjectAt(pos);
            if (obj == null) break;
            ConveyorBelt belt = obj.GetComponent<ConveyorBelt>();
            if (belt == null || belt.fixedDirection != fixedDirection) break;

            if (belt.line == null)
            {
                Debug.Log($"    扩展加入传送带 {belt.name} 到线路 {line.id}");
                line.AddBelt(belt);
            }
            else if (belt.line != line)
            {
                // 如果遇到其他线路，理论上应该合并，但此处先警告并停止扩展
                Debug.LogWarning($"扩展时遇到其他线路 {belt.line.id}，停止扩展");
                break;
            }
            pos += direction;
        }
    }

    private Vector2Int[] GetParallelOffsets(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
            case Direction.Down:
                return new Vector2Int[] { Vector2Int.up, Vector2Int.down };
            case Direction.Left:
            case Direction.Right:
                return new Vector2Int[] { Vector2Int.left, Vector2Int.right };
            default:
                return new Vector2Int[0];
        }
    }

    // 根据线路速度符号返回实际移动方向
    public Vector2Int GetMovementDirection()
    {
        Vector2Int baseDir = GetDirectionVector(); // 使用无参版本，内部应基于 fixedDirection
        if (line != null)
            return line.speed >= 0 ? baseDir : -baseDir;
        else
            return baseDir;
    }
}