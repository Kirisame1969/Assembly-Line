using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class GridObject : MonoBehaviour
{
    private GridManager gridManager;
    private Vector2Int gridPos;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        // 记录当前物体的网格坐标（假设它被放置在正确的格子中心）
        gridPos = gridManager.WorldToGrid(transform.position);
    }

    // 当物体被销毁时自动调用
    void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.RemoveObject(gridPos);
            Debug.Log("玩家销毁了一个物体");
        }
    }

}