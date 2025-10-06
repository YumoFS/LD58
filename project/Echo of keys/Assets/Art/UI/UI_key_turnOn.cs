using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_key_turnOn : MonoBehaviour
{
    public char keyNum = 'w';
    public bool isOn = false;
    public Move_Controller moveController;
    public Sprite onImage;
    public Sprite offImage;
    public Sprite haveImage = null;
    //GameObject gameObject = GetComponent<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldBeOn = false;
        bool have = false;
        if (moveController != null)
        {
            switch (keyNum)
            {

                case 'W':
                    shouldBeOn = moveController.canMoveForward;
                    break;
                case 'S':
                    shouldBeOn = moveController.canMoveBackward;
                    break;
                case 'A':
                    shouldBeOn = moveController.canMoveLeft;
                    break;
                case 'D':
                    shouldBeOn = moveController.canMoveRight;
                    break;
                // case 'j':
                //     shouldBeOn = moveController.canJump;
                //     break;
                case 'r':
                    shouldBeOn = moveController.canRun;
                    break;
                case 't':
                    shouldBeOn = moveController.canTeleport;
                    have = moveController.haveTeleport;
                    break;
                case 'c':
                    shouldBeOn = moveController.canRecall;
                    have = moveController.haveRecall;
                    break;
                case 'a':
                    shouldBeOn = moveController.canAdd;
                    have = moveController.haveAdd;
                    break;
                case 'd':
                    shouldBeOn = moveController.canDelete;
                    have = moveController.haveDelete;
                    break;
                default:
                    Debug.LogWarning("Invalid keyNum: " + keyNum);
                    break;
            }
            if (shouldBeOn != isOn)
            {
                isOn = shouldBeOn;
                gameObject.GetComponent<UnityEngine.UI.Image>().sprite = isOn ? onImage : offImage;
            }
            else if (have != false && haveImage != null && isOn == false)
            {
                gameObject.GetComponent<UnityEngine.UI.Image>().sprite = haveImage;
            }
        }
    }
}
