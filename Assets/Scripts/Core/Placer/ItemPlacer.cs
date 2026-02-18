using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//此脚本负责 物品放置 的相关逻辑

public class ItemPlacer : MonoBehaviour
{
    public GameObject itemPrefab;               // 物品预制体
    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("ItemPlacer: 找不到GridManager！");
        }
    }

    void Update()
    {
        // 物品放置逻辑：单击生成
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2Int gridPos = gridManager.WorldToGrid(mouseWorldPos);

            if (gridManager.IsWithinBounds(gridPos))
            {
            // 检查该格子是否已经有物品（通过传送带是否有物品来判断，避免重叠）
            GameObject cellObj = gridManager.GetObjectAt(gridPos);
            if (cellObj != null && cellObj.CompareTag("Conveyor"))
            {
                ConveyorBelt conveyor = cellObj.GetComponent<ConveyorBelt>();
                if (conveyor != null && conveyor.HasItem())
                {
                    Debug.Log("该传送带格子上已有物品，不能放置");
                    return;
                }
            }

            // 生成物品
            GameObject newItem = Instantiate(itemPrefab, gridManager.GridToWorld(gridPos), Quaternion.identity);
            ItemMovement itemMove = newItem.GetComponent<ItemMovement>();
            if (itemMove != null)
            {
                itemMove.SetCurrentGrid(gridPos);

                // 如果该格子有传送带且空闲，交给传送带
                if (cellObj != null && cellObj.CompareTag("Conveyor"))
                {
                    ConveyorBelt conveyor = cellObj.GetComponent<ConveyorBelt>();
                    if (conveyor != null && !conveyor.HasItem())
                    {
                        conveyor.SetItem(newItem);
                        // 不再手动调用 StartMovingInDirection
                    }
                }
                else
                {
                    Debug.Log("物品放在非传送带格子，不会移动");
                }
            }
        }
        }
    }

    public void SetItemPrefab(GameObject prefab)
    {
        itemPrefab = prefab;
    }
}
