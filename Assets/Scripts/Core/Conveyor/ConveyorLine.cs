using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//该脚本用来管理传送带线路的各种属性

public class ConveyorLine
{
    public int id;
    public float speed = 1f;                // 当前运行速度（格子/秒）
    public int totalCapacity;                 // 所有电机容量之和
    public int beltCount => belts.Count;      // 线路上的传送带数量
    public bool isOperational => totalCapacity >= beltCount; // 是否可工作

    private float minMaxSpeed = float.MaxValue; // 所有电机maxSpeed的最小值
    public List<ConveyorBelt> belts = new List<ConveyorBelt>();
    public List<Motor> motors = new List<Motor>();

    public void AddMotor(Motor motor)
    {
        if (!motors.Contains(motor))
        {
            motors.Add(motor);
            totalCapacity += motor.capacity;
            RecalcMinMaxSpeed();
        }
    }

    public void RemoveMotor(Motor motor)
    {
        if (motors.Remove(motor))
        {
            totalCapacity -= motor.capacity;
            RecalcMinMaxSpeed();
        }
    }

    public void AddBelt(ConveyorBelt belt)
    {
        if (!belts.Contains(belt))
        {
            belts.Add(belt);
            belt.line = this;
        }
    }

    public void RemoveBelt(ConveyorBelt belt)
    {
        if (belts.Remove(belt))
        {
            belt.line = null;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Clamp(newSpeed, -minMaxSpeed, minMaxSpeed);
        // 可选：通知电机更新显示
        foreach (var motor in motors)
            motor.SyncSpeedFromLine();
    }

    private void RecalcMinMaxSpeed()
    {
        if (motors.Count == 0)
            minMaxSpeed = 0;
        else
        {
            minMaxSpeed = float.MaxValue;
            foreach (var m in motors)
                if (m.maxSpeed < minMaxSpeed)
                    minMaxSpeed = m.maxSpeed;
        }
        // 如果当前速度超过新上限，则自动调低
        if (speed > minMaxSpeed)
            SetSpeed(minMaxSpeed);
    }

    // 合并两条线路的静态方法,先占位
    public static ConveyorLine Merge(ConveyorLine a, ConveyorLine b)
    {
        Debug.Log($"合并线路 {a.id} 和 {b.id}");
        Debug.Log($"线路A: 容量={a.totalCapacity}, 长度={a.beltCount}, 电机数={a.motors.Count}");
        Debug.Log($"线路B: 容量={b.totalCapacity}, 长度={b.beltCount}, 电机数={b.motors.Count}");
        // 创建新线路
        ConveyorLine merged = LineManager.CreateLine();

        // 合并所有传送带（去重）
        var allBelts = new HashSet<ConveyorBelt>(a.belts);
        allBelts.UnionWith(b.belts);
        foreach (var belt in allBelts)
        {
            // 从原线路移除引用
            if (belt.line == a) a.belts.Remove(belt);
            if (belt.line == b) b.belts.Remove(belt);
            merged.AddBelt(belt); // AddBelt会自动设置belt.line
        }

        // 合并所有电机
        var allMotors = new HashSet<Motor>(a.motors);
        allMotors.UnionWith(b.motors);
        foreach (var motor in allMotors)
        {
            motor.line = merged;
            merged.motors.Add(motor);
        }

        // 计算合并后的总容量和最小最大速度
        merged.totalCapacity = a.totalCapacity + b.totalCapacity;
        merged.RecalcMinMaxSpeed(); // 会重新计算minMaxSpeed
        // 速度重置为0，等待玩家调节
        merged.speed = 0;

        // 从LineManager中移除原线路
        LineManager.RemoveLine(a);
        LineManager.RemoveLine(b);

        Debug.Log($"合并后新线路 {merged.id}: 容量={merged.totalCapacity}, 长度={merged.beltCount}, 工作={merged.isOperational}");
        return merged;
    }
}