using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//此脚本负责 建筑放置 的拖拽连续放置逻辑

public class BuildingPlacer : MonoBehaviour
{
    public GameObject buildingPrefab;           // 要放置的建筑预制体
    private GridManager gridManager;
    private Vector2Int? lastPlacedGridPos = null; // 记录上一次放置的网格位置，防止同一格重复放置

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("BuildingPlacer: 找不到GridManager！");
        }
    }

    void Update()
{
    bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    if (shiftHeld)
    {
        // Shift按住：拖拽连续放置
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2Int currentGridPos = gridManager.WorldToGrid(mouseWorldPos);

            if (gridManager.IsWithinBounds(currentGridPos) && !gridManager.IsCellOccupied(currentGridPos))
            {
                if (lastPlacedGridPos == null || currentGridPos != lastPlacedGridPos.Value)
                {
                    Vector3 spawnPos = gridManager.GridToWorld(currentGridPos);
                    GameObject newObj = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
                    bool placed = gridManager.PlaceObject(currentGridPos, newObj);
                    if (placed)
                    {
                        lastPlacedGridPos = currentGridPos;
                    }
                    else
                    {
                        Destroy(newObj);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lastPlacedGridPos = null;
        }
    }
    else
    {
        // 没有Shift：单击放置（单次）
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2Int currentGridPos = gridManager.WorldToGrid(mouseWorldPos);

            if (gridManager.IsWithinBounds(currentGridPos) && !gridManager.IsCellOccupied(currentGridPos))
            {
                Vector3 spawnPos = gridManager.GridToWorld(currentGridPos);
                GameObject newObj = Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
                bool placed = gridManager.PlaceObject(currentGridPos, newObj);
                if (!placed)
                {
                    Destroy(newObj);
                }
                // 不记录lastPlacedGridPos，因为不连续
            }
        }
    }
}

    // 提供一个公共方法，允许外部设置建筑预制体（后续可用于切换不同建筑）
    public void SetBuildingPrefab(GameObject prefab)
    {
        buildingPrefab = prefab;
    }
}