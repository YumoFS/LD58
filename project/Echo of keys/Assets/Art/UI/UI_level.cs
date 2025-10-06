using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_level : MonoBehaviour
{
    public int levelNum = 1;
    private int thisLevelNum = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (thisLevelNum != levelNum)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
