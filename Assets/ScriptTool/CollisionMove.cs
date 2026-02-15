using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionMove : MonoBehaviour
{
    // 当有物体进入碰撞器时，此方法会被自动调用
    // OnCollisionEnter2D()  这是Unity的碰撞事件方法。
    // 当当前对象（挂载此脚本的物体）的2D碰撞器与其他物体的2D碰撞器发生接触时，此方法会被自动调用一次。
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞到的物体是不是名为"Player"
        // collision.gameObject是指与当前对象发生碰撞的那个物体,.name是它的名字属性。
        // 也可以使用标签来实现， collision.gameObject.CompareTag("Player")，前提是玩家对象有设置标签为"Player"。
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("Player hit the target!");
            // 在控制台输出信息
            Destroy(gameObject);
            // 销毁当前物体（即Target）
            // Unity的一个函数，用于销毁游戏对象、组件或资源。
            // 这里gameObject指的是当前脚本所挂载的对象（即普通方块自身）。
        }
    }
}
