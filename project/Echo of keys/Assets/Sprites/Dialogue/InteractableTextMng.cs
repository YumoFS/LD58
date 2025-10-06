using UnityEngine;

public class InteractableTextMng : MonoBehaviour
{
    public static InteractableTextMng Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }
}
