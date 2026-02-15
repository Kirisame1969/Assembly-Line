using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查该物体是否带有名为"Player"的标签。
        // 标签是Unity中给物体分类的标识，可以在Inspector中设置。
        // 使用标签比使用名称更可靠，因为名称可能重复或变化。
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Boss collided with Player! Player destroyed.");
            Destroy(collision.gameObject);  // 销毁玩家
            
        }
    }
}
