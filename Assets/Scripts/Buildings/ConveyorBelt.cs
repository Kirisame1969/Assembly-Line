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

    public void TryJoinLine()
    {
        Debug.Log($"[TryJoinLine] 传送带 {name} 尝试加入线路");
        if (line != null) return; // 已经在线路中

        Vector2Int gridPos = gridManager.WorldToGrid(transform.position);
        Direction dir = fixedDirection;
        Vector2Int[] neighborOffsets = GetParallelOffsets(dir);

        HashSet<ConveyorLine> neighborLines = new HashSet<ConveyorLine>();

        foreach (var offset in neighborOffsets)
        {
            Vector2Int neighborPos = gridPos + offset;
            if (!gridManager.IsWithinBounds(neighborPos)) continue;
                GameObject neighborObj = gridManager.GetObjectAt(neighborPos);
            if (neighborObj == null) continue;
                ConveyorBelt neighborBelt = neighborObj.GetComponent<ConveyorBelt>();
            if (neighborBelt != null && neighborBelt.fixedDirection == dir && neighborBelt.line != null)
            {
                neighborLines.Add(neighborBelt.line);
            }
        }

        if (neighborLines.Count == 0)
        {
            // 无相邻线路，不处理
        }
        else if (neighborLines.Count == 1)
        {
            ConveyorLine lineToJoin = neighborLines.First();
            Debug.Log($"  加入现有线路 {lineToJoin.id}");
            lineToJoin.AddBelt(this);
        }
        else // 2条不同线路
        {
            ConveyorLine lineA = neighborLines.ElementAt(0);
            ConveyorLine lineB = neighborLines.ElementAt(1);
            Debug.Log($"  检测到两条不同线路 {lineA.id} 和 {lineB.id}，准备合并");
            ConveyorLine merged = ConveyorLine.Merge(lineA, lineB);
            merged.AddBelt(this);
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
}