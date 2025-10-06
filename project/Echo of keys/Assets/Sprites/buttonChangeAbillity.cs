using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonChangeAbillity : MonoBehaviour
{
    private Move_Controller moveController;
    void Start()
    {
        moveController = FindObjectOfType<Move_Controller>();
    }

    void Update()
    {
        if (moveController == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            moveController.ChangeMovementDirectionLock("t/r");
            AudioMng.Instance.PlaySound("equip");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            moveController.ChangeMovementDirectionLock("d/a");
            AudioMng.Instance.PlaySound("equip");
        }
    }
}
