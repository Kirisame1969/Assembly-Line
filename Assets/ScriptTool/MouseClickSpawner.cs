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

    void Start()
    {
        // 获取场景中的GridManager组件（假设挂载在某个对象上）
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("场景中没有GridManager！请创建一个空物体并挂载GridManager脚本。");
        }
    }
    void Update()// 每一帧自动调用检测
    {
        // 检测鼠标左键是否被按下，按下返回true。0是左键，1是右键，2是中键。
        if (Input.GetMouseButtonDown(0))
        {
            // 将鼠标屏幕坐标转换为世界坐标
            // 获取场景中主摄像机Camera.main
            // ScreenToWorldPoint方法 将当前鼠标位置Input.mousePosition转换为世界坐标。
            // 屏幕坐标系原点在左下角，而世界坐标系以场景中心为原点
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // z轴设为0，毕竟平面
            mouseWorldPos.z = 0;

            // 将世界坐标转换为网格坐标
            Vector2Int gridPos = gridManager.WorldToGrid(mouseWorldPos);
            // 检查格子是否可放置（在范围内且未被占用）
            if (gridManager.IsCellOccupied(gridPos))
            {
                Debug.Log("该格子已有物体，无法放置");
                return; // 不执行生成
            }
            
            // 再将网格坐标转回世界坐标（格子中心）
            Vector3 spawnPos = gridManager.GridToWorld(gridPos);

            // 在鼠标位置的格子中心生成物体
            // Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
            // Instantiate 在指定位置和旋转下克隆预制体。Quaternion.identity表示无旋转。
            GameObject newObj = Instantiate(objectToSpawn, spawnPos, Quaternion.identity);

            // 重要：将物体注册到网格管理器中
            bool placed = gridManager.PlaceObject(gridPos, newObj);
            if (!placed)
            {
                // 如果放置失败（理论上不会，因为前面已经检查过），则销毁刚生成的物体
                Destroy(newObj);
            }
            Debug.Log("玩家放置了一个物体");
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

