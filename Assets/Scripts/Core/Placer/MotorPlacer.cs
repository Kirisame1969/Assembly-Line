using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MotorPlacer : MonoBehaviour
{
    public GameObject motorPrefab;   // 电机预制体
    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2Int gridPos = gridManager.WorldToGrid(mouseWorldPos);

        // 第一步：检测该位置是否已有电机
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Motor>() != null)
            {
                Debug.Log("该格子上已有电机");
                return;
            }
        }

        // 第二步：检查是否有传送带
        GameObject cellObj = gridManager.GetObjectAt(gridPos);
        if (cellObj == null)
        {
            Debug.Log("该格子没有建筑");
            return;
        }
        ConveyorBelt belt = cellObj.GetComponent<ConveyorBelt>();
        if (belt == null)
        {
            Debug.Log("该格子上的建筑不是传送带");
            return;
        }

        // 第三步：放置电机
        PlaceMotor(belt, gridPos);
    }
}

    private void PlaceMotor(ConveyorBelt startBelt, Vector2Int gridPos)
    {
        // 扫描整条直线，获取所有连续同向传送带
        List<ConveyorBelt> lineBelts = ScanFullLine(startBelt);
        // 收集这些传送带所属的线路（去重）
        HashSet<ConveyorLine> involvedLines = new HashSet<ConveyorLine>();
        foreach (var b in lineBelts)
            if (b.line != null) involvedLines.Add(b.line);

        // 根据情况创建/加入/合并线路
        if (involvedLines.Count == 0)
        {
            // 创建新线路
            ConveyorLine newLine = LineManager.CreateLine();
            foreach (var b in lineBelts)
                newLine.AddBelt(b);
            // 创建电机并初始化
            GameObject motorObj = Instantiate(motorPrefab, gridManager.GridToWorld(gridPos), Quaternion.identity);
            Motor motor = motorObj.GetComponent<Motor>();
            motor.Initialize(newLine);
        }
        else if (involvedLines.Count == 1)
        {
            ConveyorLine line = involvedLines.First();
            // 将尚未加入线路的传送带加入
            foreach (var b in lineBelts)
                if (b.line == null)
                    line.AddBelt(b);
            // 创建电机并初始化
            GameObject motorObj = Instantiate(motorPrefab, gridManager.GridToWorld(gridPos), Quaternion.identity);
            Motor motor = motorObj.GetComponent<Motor>();
            motor.Initialize(line);
        }
        else // involvedLines.Count == 2
        {
            // 合并两条线路
            ConveyorLine lineA = involvedLines.ElementAt(0);
            ConveyorLine lineB = involvedLines.ElementAt(1);
            Debug.Log($"准备合并线路 {lineA.id} 和 {lineB.id}");
            ConveyorLine merged = ConveyorLine.Merge(lineA, lineB);
            // 确保所有传送带都在合并后的线路中
            foreach (var b in lineBelts)
                if (b.line == null)
                    merged.AddBelt(b);
            // 创建电机并初始化
            GameObject motorObj = Instantiate(motorPrefab, gridManager.GridToWorld(gridPos), Quaternion.identity);
            Motor motor = motorObj.GetComponent<Motor>();
            motor.Initialize(merged);
        }
    }

    // 扫描整条直线（两个方向），返回所有连续且方向相同的传送带
    private List<ConveyorBelt> ScanFullLine(ConveyorBelt startBelt)
    {
        List<ConveyorBelt> result = new List<ConveyorBelt>();
        ConveyorBelt.Direction dir = startBelt.fixedDirection;
        Vector2Int startPos = gridManager.WorldToGrid(startBelt.transform.position);

        // 向正方向扫描
        Vector2Int step = GetDirectionVector(dir);
        Vector2Int pos = startPos;
        while (true)
        {
            GameObject obj = gridManager.GetObjectAt(pos);
            if (obj == null) break;
            ConveyorBelt belt = obj.GetComponent<ConveyorBelt>();
            if (belt == null || belt.fixedDirection != dir) break;
            if (!result.Contains(belt)) result.Add(belt);
            pos += step;
        }

        // 向反方向扫描（不包括起点，避免重复）
        step = -step;
        pos = startPos + step;
        while (true)
        {
            GameObject obj = gridManager.GetObjectAt(pos);
            if (obj == null) break;
            ConveyorBelt belt = obj.GetComponent<ConveyorBelt>();
            if (belt == null || belt.fixedDirection != dir) break;
            if (!result.Contains(belt)) result.Add(belt);
            pos += step;
        }

        return result;
    }

    private Vector2Int GetDirectionVector(ConveyorBelt.Direction dir)
    {
        switch (dir)
        {
            case ConveyorBelt.Direction.Up: return Vector2Int.up;
            case ConveyorBelt.Direction.Down: return Vector2Int.down;
            case ConveyorBelt.Direction.Left: return Vector2Int.left;
            case ConveyorBelt.Direction.Right: return Vector2Int.right;
            default: return Vector2Int.right;
        }
    }
}