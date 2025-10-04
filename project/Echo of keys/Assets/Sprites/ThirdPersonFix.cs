using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class ThirdPersonFix : MonoBehaviour
{
    PlayerInput playerInput;
    private Camera cam;

    [Header("Target")]
    public Transform target;         // 要跟随的角色
    public Vector3 offset = new Vector3(0, 5f, -8f); // 默认相机偏移

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;     // 滚轮缩放灵敏度
    public float minDistance = 3f;   // 最近距离
    public float maxDistance = 12f;  // 最远距离
    private float zoomInput;
    private float targetDistance;    // 缩放后的距离
    private float currentDistance;   // 实际距离（用于平滑过渡）

    [Header("Follow Settings")]
    public float followSmooth = 5f;  // 跟随平滑度


    void OnZoomInput(InputAction.CallbackContext context)
    {
        zoomInput = context.ReadValue<Vector2>().y;
    }

    void OnZoomCanceled(InputAction.CallbackContext context)
    {
        zoomInput = 0f;
    }

    void HandleZoom()
    {
        // 基于摄像机位置的放大计算
        // if (Mathf.Abs(zoomInput) > 0.01f)
        // {
        //     targetDistance -= zoomInput * zoomSpeed * Time.deltaTime;
        //     targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        // }

        // // 平滑过渡距离
        // currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 8f);

        // 基于摄像机 FOV 的放大计算（我更喜欢这个！）
        if (cam == null) return;
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            cam.fieldOfView -= zoomInput * 0.1f; // 用 zoomInput 调整FOV
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 30f, 90f);
        }
    }

    void HandleFollow()
    {
        // 计算缩放后的偏移（保持等距角度，只改变长度）
        Vector3 dir = offset.normalized * currentDistance;
        Vector3 desiredPos = target.position + dir;

        // 平滑跟随
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        // 始终看向角色
        transform.LookAt(target.position + Vector3.up * 1.5f); // 稍微看向角色头顶
    }

    void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();

        // 获取自身的 Camera 组件
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("这个物体上没有 Camera 组件！");
        }
        // 默认初始化
        targetDistance = offset.magnitude;
        currentDistance = targetDistance;

        // 绑定 zoom action
        playerInput.player.zoom.performed += OnZoomInput;
        playerInput.player.zoom.canceled += OnZoomCanceled;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return;
        HandleZoom();
        HandleFollow();
    }
}
