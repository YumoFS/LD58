using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbilityTrigger : MonoBehaviour
{
    [Header("解锁的移动方向")]
    public string directionToUnlock = "backward"; // 在Inspector中设置要解锁的方向
    
    [Header("触发设置")]
    public bool destroyAfterTrigger = true; // 触发后是否销毁物体
    public GameObject visualEffect;
    public AudioClip soundEffect;
    [Header("插画收集")]
    public int levelNum = 1;
    public GameObject illustration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Move_Controller moveController = other.GetComponent<Move_Controller>();
            if (moveController != null)
            {
                moveController.UnlockMovementDirection(directionToUnlock);

                // 播放视觉效果
                if (visualEffect != null)
                {
                    Instantiate(visualEffect, transform.position, transform.rotation);
                    AudioSource.PlayClipAtPoint(soundEffect, transform.position);
                }

                // 显示插画
                if (illustration != null)
                {
                    illustration.SetActive(true);
                }

                // 触发后销毁
                if (destroyAfterTrigger)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}