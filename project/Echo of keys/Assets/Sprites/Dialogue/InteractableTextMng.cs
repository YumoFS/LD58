using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableTextMng : MonoBehaviour
{
    public static InteractableTextMng Instance { get; private set; }
    void Start()
    {
        Instance = this;
    }
}
