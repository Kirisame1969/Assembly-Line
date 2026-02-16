using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }
    public Direction direction = Direction.Right; // 默认向右

    private GameObject currentItem; // 当前格子上的物品
    private GridManager gridManager;
    private Vector2Int gridPos;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        // 获取自己的网格坐标
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
        switch (direction)
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
}