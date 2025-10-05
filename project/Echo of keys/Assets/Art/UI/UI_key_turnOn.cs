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
    //GameObject gameObject = GetComponent<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldBeOn = false;
        if (moveController != null)
        {
            switch (keyNum)
            {
                case 'w':
                    shouldBeOn = moveController.canMoveForward;
                    break;
                case 's':
                    shouldBeOn = moveController.canMoveBackward;
                    break;
                case 'a':
                    shouldBeOn = moveController.canMoveLeft;
                    break;
                case 'd':
                    shouldBeOn = moveController.canMoveRight;
                    break;
                // case 'j':
                //     shouldBeOn = moveController.canJump;
                //     break;
                case 'r':
                    shouldBeOn = moveController.canRun;
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
        }
    }
}
