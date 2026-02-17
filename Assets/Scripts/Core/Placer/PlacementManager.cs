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

    void Update()
    {
        // 可以根据需要动态切换，但这里我们保持简单：通过按钮切换时调用SetMode方法
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