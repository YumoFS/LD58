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
    public Vector3 offset = new Vector3(0, 5f, -3f); // 默认相机偏移

    [Header("Zoom Settings")]
    public float zoomSpeed = 0f;     // 滚轮缩放灵敏度
    public float minFOV = 60f;       // 最小FOV（放大到最大）
    public float maxFOV = 60f;       // 最大FOV（缩小到最远）
    private float zoomInput;
    private float currentDistance;   // 固定距离（用于跟随计算）

    [Header("Follow Settings")]
    public float followSmooth = 70f;  // 跟随平滑度

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
        if (cam == null) return;
        
        // 基于摄像机 FOV 的缩放计算
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            cam.fieldOfView -= zoomInput * zoomSpeed * Time.deltaTime;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
        }
    }

    void HandleFollow()
    {
        // 使用固定距离计算位置（缩放由FOV处理）
        Vector3 dir = offset.normalized * currentDistance;
        Vector3 desiredPos = target.position + dir;

        // 平滑跟随
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        // 始终看向角色
        transform.LookAt(target.position + Vector3.up * 1.5f);
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
        
        // 关键修改：初始化FOV为最小值（放大到最大）
        if (cam != null)
        {
            cam.fieldOfView = maxFOV;
        }
        
        // 固定距离（跟随计算使用）
        currentDistance = offset.magnitude;

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