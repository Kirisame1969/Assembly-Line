using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // 每个格子的世界单位大小（1单位32像素，取决于精灵设置）
    public float cellSize = 1f;

    // 定义网格大小（可以根据需要调整，也可以动态扩展）
    public int gridWidth = 10;
    public int gridHeight = 10;

    // 二维数组，存储每个格子上的游戏对象（如果没有则为null）
    private GameObject[,] gridObjects;

    void Awake()
    {
        // 初始化二维数组
        gridObjects = new GameObject[gridWidth, gridHeight];
        
    }


    // 将世界坐标转换为网格坐标（返回整数网格索引）
    // 将世界坐标转换为网格索引。用FloorToInt取整
    // 例如世界坐标(2.3, 1.8)除以1后得到(2.3, 1.8)，向下取整得(2,1)，即网格(2,1)的格子。
    // 注意：格子左下角为整数坐标，右上角为整数坐标+1。
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    // 将网格坐标转换为世界坐标（返回格子中心位置）
    // 乘以cellSize得到格子左下角的世界坐标，再加上0.5*cellSize得到格子中心的世界坐标。
    // 例如网格坐标(2,1)乘以1得到(2,1)，加上(0.5,0.5)得到(2.5,1.5)，即格子(2,1)的中心位置。
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = (gridPos.x + 0.5f) * cellSize;
        float y = (gridPos.y + 0.5f) * cellSize;
        return new Vector3(x, y, 0);
    }


    // 检查格子是否在网格范围内
    public bool IsWithinBounds(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    // 检查格子是否被占用
    public bool IsCellOccupied(Vector2Int gridPos)
    {
        if (!IsWithinBounds(gridPos))
            return true; // 超出边界视为不可放置
        return gridObjects[gridPos.x, gridPos.y] != null;
    }

    // 在格子上放置物体
    public bool PlaceObject(Vector2Int gridPos, GameObject obj)
    {
        if (!IsWithinBounds(gridPos))
        {
            Debug.LogWarning("尝试在网格范围外放置物体");
            return false;
        }
        if (gridObjects[gridPos.x, gridPos.y] != null)
        {
            Debug.Log("格子已被占用，无法放置");
            return false;
        }
        gridObjects[gridPos.x, gridPos.y] = obj;
        return true;
    }

    // 移除格子上的物体（当物体被销毁时调用）
    public void RemoveObject(Vector2Int gridPos)
    {
        if (IsWithinBounds(gridPos))
        {
            gridObjects[gridPos.x, gridPos.y] = null;
        }
    }

    // 可选：根据物体获取它所在的格子（如果物体有记录自己的网格坐标会更方便）
    // 但为了简单，我们可以在物体被销毁时由外部传入它的网格坐标

    public GameObject GetObjectAt(Vector2Int gridPos)
    {
    if (IsWithinBounds(gridPos))
        return gridObjects[gridPos.x, gridPos.y];
    return null;
    }
    
}