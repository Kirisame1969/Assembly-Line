using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickSpawner : MonoBehaviour
{
    // 要生成的预制体，需要在Inspector中指定,实则是把做好的东西放到资源文件夹里面
    public GameObject objectToSpawn;
    // 定义一个公共的GameObject变量,名为objectToSpawn，用来存放你要生成的物体预制体。
    // 因为是public，你可以在Unity编辑器中将预制体拖拽到这个字段上。
    private GridManager gridManager;           // 引用GridManager

    // 记录上一次放置的网格坐标（可空类型，初始为null）
    private Vector2Int? lastPlacedGridPos = null;

    
    public enum PlacementMode
    {
        Building,   // 放置建筑（传送带）
        Item        // 放置物品（矿石）
    }

    public PlacementMode currentMode = PlacementMode.Building; // 当前模式，默认为建筑
    
    // 切换到建筑模式
    public void SetBuildingMode()
    {
        currentMode = PlacementMode.Building;
        Debug.Log("切换到：放置传送带");
    }

    // 切换到物品模式
    public void SetItemMode()
    {
        currentMode = PlacementMode.Item;
        Debug.Log("切换到：放置物品");
    }

    void Start()
    {
        // 获取场景中的GridManager组件（假设挂载在某个对象上）
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("场景中没有GridManager！请创建一个空物体并挂载GridManager脚本。");
        }
    }

    void Update()
    {
        // 检测鼠标左键是否按住（0表示左键）
        if (Input.GetMouseButton(0))
        {
            // 鼠标位置转世界坐标
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // 获取当前鼠标所在的网格坐标
            Vector2Int currentGridPos = gridManager.WorldToGrid(mouseWorldPos);

            // 检查该格子是否在网格范围内且未被占用
            if (gridManager.IsWithinBounds(currentGridPos) && !gridManager.IsCellOccupied(currentGridPos))
            {
                // 如果是第一次放置，或者移动到了一个新的格子（不是上次放置的格子），才放置
                if (lastPlacedGridPos == null || currentGridPos != lastPlacedGridPos.Value)
                {
                    // 计算放置位置（格子中心）
                    Vector3 spawnPos = gridManager.GridToWorld(currentGridPos);

                    // 生成物体
                    GameObject newObj = Instantiate(objectToSpawn, spawnPos, Quaternion.identity);

                    // 将物体注册到网格管理器中
                    bool placed = gridManager.PlaceObject(currentGridPos, newObj);
                    if (placed)
                    {
                        // 记录这次放置的网格位置
                        lastPlacedGridPos = currentGridPos;
                    }
                    else
                    {
                        // 放置失败（理论上不会发生，因为前面已经检查过占用），销毁生成的物体
                        Destroy(newObj);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 松开左键时，重置上次放置记录，以便下次重新开始
            lastPlacedGridPos = null;
        }
    
    
        

        if (Input.GetMouseButtonDown(1)) // 右键
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2Int gridPos = gridManager.WorldToGrid(mouseWorldPos);
    
            // 获取该格子上的物体（如果存在）
            GameObject obj = gridManager.GetObjectAt(gridPos); // 需要先添加这个方法
            if (obj != null)
            {
                Destroy(obj); // 销毁物体会自动触发GridObject的OnDestroy，从而移除记录
            }
        }
    }
}

