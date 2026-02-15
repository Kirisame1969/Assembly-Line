using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    
    public float moveSpeed = 5f; // 公共变量，表示移动的速度，可在面板中调整

    // Update()：Unity的生命周期方法，每帧自动调用一次（通常每秒60次左右）。
    // 用于处理需要持续更新的逻辑，比如移动、输入检测。
    void Update()
    {
        // 获取输入
        float moveX = Input.GetAxis("Horizontal"); // A/D ← →
        float moveY = Input.GetAxis("Vertical");   // W/S ↑ ↓
        // Unity的输入系统方法，返回一个介于-1到1之间的浮点数，表示水平轴输入。
        // 默认情况下，按左方向键或A键返回-1，右方向键或D键返回1，无输入返回0。
        // 它平滑过渡，适合移动控制。


        // 计算移动方向
        Vector2 movement = new Vector2(moveX, moveY);
        // Vector2表示一个二维向量的结构体，包含x和y两个浮点数
        // 创建一个新的Vector2实例movement，将moveX赋值给它的x分量，moveY赋值给y分量。
        // 这里movement代表了移动的方向和大小，即方向向量。
        // 移动物体 (修改Transform)

        transform.Translate(movement * moveSpeed * Time.deltaTime);
        // transform是当前脚本所挂载对象的Transform组件。每个游戏对象都有，存储位置、旋转、缩放。
        // movement是上文方向向量，moveSpeed是速度（单位/秒）。
        // Time.deltaTime是上一帧到当前帧所经过的时间（秒）。
        // 乘以它可以将速度从“单位/秒”转换为“单位/帧”，保证移动速度不依赖于帧率

        //Debug.Log($"玩家当前位置: {transform.position}");
    }
}