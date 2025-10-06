using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dooropen : MonoBehaviour
{
    public string keyType = "none";
    public AudioClip openSound;
    public AudioClip accessDeniedSound;
    public GameObject deniedEffect;
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Move_Controller moveController = other.GetComponent<Move_Controller>();
        if (moveController != null)
        {
            if (CheckKey(moveController))
            {
                doorOpen();
            }
            else
            {
                DenyAccess();
            }
        }
    }

    bool CheckKey(Move_Controller controller)
    {
        switch (keyType.ToLower())
        {
            case "iron":
                return controller.haveIronKey;
            case "copper":
                return controller.haveCopperKey;
            case "silver":
                return controller.haveSilverKey;
            case "golden":
                return controller.haveGoldenKey;
            default:
                Debug.LogWarning($"未知的钥匙类型: {keyType}");
                return false;
        }
    }

    void doorOpen()
    {
        if (openSound != null) AudioSource.PlayClipAtPoint(openSound, transform.position);
        Destroy(gameObject);
    }
    
    void DenyAccess()
    {
        Debug.Log($"需要{keyType}钥匙才能打开这扇门！");
        
        if (accessDeniedSound != null)
        {
            AudioSource.PlayClipAtPoint(accessDeniedSound, transform.position);
        }
        
        if (deniedEffect != null)
        {
            Instantiate(deniedEffect, transform.position, Quaternion.identity);
        }
    }
}
