using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{
    public float maxSpeed = 2f;      // 最高速度（格子/秒）
    public int capacity = 5;          // 可驱动的传送带数量
    public float currentSpeed { get; private set; } // 当前运行速度，由线路同步

    public ConveyorLine line;         // 所属线路

    public void Initialize(ConveyorLine targetLine)
    {
        line = targetLine;
        line.AddMotor(this);
        SyncSpeedFromLine();
    }

    public void SyncSpeedFromLine()
    {
        if (line != null)
            currentSpeed = line.speed;
    }

    void OnDestroy()
    {
        if (line != null)
            line.RemoveMotor(this);
    }
}