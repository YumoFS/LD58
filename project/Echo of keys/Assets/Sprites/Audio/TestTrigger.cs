using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    public string target;
    void Start()
    {
        AudioMng.Instance.PlayMusic(target);
    }
}
