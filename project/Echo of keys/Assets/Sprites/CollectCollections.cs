using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectCollections : MonoBehaviour
{
    public int levelNum = 1;
    public GameObject illustration;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!illustration.GetComponent<PuzzlePutVFX>()) illustration.AddComponent<PuzzlePutVFX>();
            illustration.SetActive(true);
            // 更新UI或其他逻辑以反映收集状态

            AudioMng.Instance.PlaySound("keyCollect", true);

            // 销毁收集物体
            Destroy(gameObject);
        }
    }
}
