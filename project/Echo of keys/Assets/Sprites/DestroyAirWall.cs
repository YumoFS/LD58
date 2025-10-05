using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAirWall : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
