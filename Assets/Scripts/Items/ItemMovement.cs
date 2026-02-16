using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMovement : MonoBehaviour
{
    public float moveSpeed = 2f;            // 移动速度
    private Vector3 targetPosition;          // 当前目标位置（格子中心）
    private bool isMoving = false;           // 是否正在移动
    private Vector2Int currentGridPos;       // 当前所在的网格坐标
    private GridManager gridManager;
    private bool isInitialized = false; // 标记是否已通过 SetCurrentGrid 初始化

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("ItemMovement: 找不到GridManager！");
        }
    }


    void Start()
    {
        Debug.Log($"[Item] Start: 物品 {name} 初始化，初始位置 {transform.position}");
        // 临时日志
        // 如果还没有通过 SetCurrentGrid 初始化，则根据当前位置初始化
        if (!isInitialized)
        {
            currentGridPos = gridManager.WorldToGrid(transform.position);
            targetPosition = gridManager.GridToWorld(currentGridPos);
            transform.position = targetPosition;
        }
        
    }
    

    void Update()
    {
        if (!isMoving) return;

        // 平滑移动到目标位置
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        Debug.Log($"移动距离：{Vector3.Distance(transform.position, targetPosition)}");
        //临时日志

        // 检查是否到达目标位置（距离很小则视为到达）
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            ArriveAtTarget();
        }
    }

    // 由传送带调用，让物品开始向某个方向移动
    public void StartMovingInDirection(Vector2Int direction)
    {
        Debug.Log($"[Item] StartMovingInDirection: 物品 {name} 当前网格 {currentGridPos}，方向 {direction}，isMoving={isMoving}");
        //临时日志

        // 计算下一个格子的网格坐标
        Vector2Int nextGridPos = currentGridPos + direction;
        
        Debug.Log($"[Item] 下一个格子 {nextGridPos}，是否有传送带？{gridManager.GetObjectAt(nextGridPos) != null}");
        //临时日志

        // 检查下一个格子是否在网格范围内
        if (gridManager.IsWithinBounds(nextGridPos))
        {
            GameObject nextCellObj = gridManager.GetObjectAt(nextGridPos); // 获取下一个格子上的建筑
            // 如果下一个格子有传送带，且该传送带当前没有物品
            if (nextCellObj != null && nextCellObj.CompareTag("Conveyor"))
            {
                ConveyorBelt nextConveyor = nextCellObj.GetComponent<ConveyorBelt>();
                
                if (nextConveyor != null && !nextConveyor.HasItem())
                {
                    // 转移物品：当前格子上的传送带失去物品
                    ConveyorBelt currentConveyor = gridManager.GetObjectAt(currentGridPos).GetComponent<ConveyorBelt>();
                    if (currentConveyor != null)
                        currentConveyor.SetItem(null);

                    // 设置新目标位置为下一格子中心
                    targetPosition = gridManager.GridToWorld(nextGridPos);
                    currentGridPos = nextGridPos;
                    isMoving = true;

                    // 告诉目标传送带它现在有这个物品
                    nextConveyor.SetItem(this.gameObject);
                    return;
                }
            }
        }

        // 如果没有可移动的下一个格子（无传送带或传送带被占用），则停止移动
        isMoving = false;
    }

    // 到达目标格子中心时调用
    private void ArriveAtTarget()
    {
        Debug.Log($"[Item] ArriveAtTarget: 物品 {name} 到达网格 {currentGridPos}，当前位置 {transform.position}");
        //临时日志

        // 查询当前格子上的建筑
        GameObject currentCellObj = gridManager.GetObjectAt(currentGridPos);

        Debug.Log($"[Item] 当前格子对象: {currentCellObj?.name}，标签: {currentCellObj?.tag}");
        //临时日志
        if (currentCellObj != null && currentCellObj.CompareTag("Conveyor"))
        {
            ConveyorBelt conveyor = currentCellObj.GetComponent<ConveyorBelt>();
            if (conveyor != null)
            {
                // 根据传送带方向决定下一个移动方向
                StartMovingInDirection(conveyor.GetDirectionVector());
            }
        }
        else
        {
            // 如果当前格子不是传送带（例如到达末端），停止移动
            isMoving = false;
        }
    }

    // 由外部调用（如 ItemPlacer）设置物品的初始网格
    public void SetCurrentGrid(Vector2Int gridPos)
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("SetCurrentGrid: 仍然找不到GridManager，无法设置位置");
                return;
            }
        }

        currentGridPos = gridPos;
        targetPosition = gridManager.GridToWorld(gridPos);
        transform.position = targetPosition;
        isMoving = false;
        isInitialized = true; // 标记已初始化
    }

}