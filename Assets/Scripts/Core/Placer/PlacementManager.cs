using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//此脚本为点击逻辑总控制器

public class PlacementManager : MonoBehaviour
{
    public enum PlacementMode
    {
        Building,
        Item,
        Motor,
    }

    public PlacementMode currentMode = PlacementMode.Building;

    private BuildingPlacer buildingPlacer;
    private ItemPlacer itemPlacer;
    private MotorPlacer motorPlacer;

    void Start()
    {
        // 获取子脚本组件（假设它们挂载在同一个GameObject上）
        buildingPlacer = GetComponent<BuildingPlacer>();
        itemPlacer = GetComponent<ItemPlacer>();
        motorPlacer = GetComponent<MotorPlacer>();
        if (motorPlacer == null)
            Debug.LogError("缺少MotorPlacer组件");
        if (buildingPlacer == null || itemPlacer == null)
        {
            Debug.LogError("PlacementManager: 缺少BuildingPlacer或ItemPlacer组件！");
        }

        // 初始化启用正确的脚本
        UpdateMode();
    }

    private Motor selectedMotor;

    void Update()
    {
        // 鼠标左键选中物体
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("左键按下"); // 新增日志
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null)
            {
                Debug.Log($"点击到物体: {hit.gameObject.name}"); // 新增
                selectedMotor = hit.GetComponent<Motor>();
                if (selectedMotor != null)
                {
                    Debug.Log($"选中电机，线路ID: {(selectedMotor.line == null ? "null" : selectedMotor.line.id.ToString())}, 速度: {selectedMotor.line?.speed}");
                }
                else
                {
                    Debug.Log("点击到的物体不是电机");
                }
            }
            else
            {
                Debug.Log("点击空白处");
                selectedMotor = null;
            }
        }

        // 快捷键调节速度
        // 快捷键调节速度（上下箭头）
        if (selectedMotor != null)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("上箭头按下");
                if (selectedMotor.line != null)
                {
                    selectedMotor.line.SetSpeed(selectedMotor.line.speed + 0.5f);
                    Debug.Log($"速度调整为：{selectedMotor.line.speed}");
                }
                else
                {
                    Debug.LogError("选中电机的line为null");
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("下箭头按下");
                if (selectedMotor.line != null)
                {
                    selectedMotor.line.SetSpeed(selectedMotor.line.speed - 0.5f);
                    Debug.Log($"速度调整为：{selectedMotor.line.speed}");
                }
                else
                {
                    Debug.LogError("选中电机的line为null");
                }
            }
        }
        else
        {
            // 可选：打印未选中状态，但会刷屏，暂时不加
        }
    }

    public void SetBuildingMode()
    {
        currentMode = PlacementMode.Building;
        UpdateMode();
        Debug.Log("切换到：放置建筑");
    }

    public void SetItemMode()
    {
        currentMode = PlacementMode.Item;
        UpdateMode();
        Debug.Log("切换到：放置物品");
    }

    private void UpdateMode()
    {
        // 启用当前模式的脚本，禁用另一个
        buildingPlacer.enabled = (currentMode == PlacementMode.Building);
        itemPlacer.enabled = (currentMode == PlacementMode.Item);
        if (motorPlacer != null)
            motorPlacer.enabled = (currentMode == PlacementMode.Motor);
    }

    // 可选：提供设置预制体的方法，方便后续扩展
    public void SetBuildingPrefab(GameObject prefab)
    {
        if (buildingPlacer != null)
            buildingPlacer.SetBuildingPrefab(prefab);
    }

    public void SetItemPrefab(GameObject prefab)
    {
        if (itemPlacer != null)
            itemPlacer.SetItemPrefab(prefab);
    }

    public void SetMotorMode()
    {
        currentMode = PlacementMode.Motor;
        UpdateMode();
        //DeselectObject(); 
        // 如果有选中物体，取消选中
    }
}