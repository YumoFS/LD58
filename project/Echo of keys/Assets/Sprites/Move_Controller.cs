using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Move_Controller : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    [Header("Movement Settings")]
    public float WalkSpeed = 4.0f;
    public float RunSpeed = 6.0f;
    public float gravity = -9.81f;
    public float rotationFactorPerFrame = 5.0f;

    [Header("Animation Settings")]
    public float acceleration = 0.8f;
    public float deceleration = 0.8f;
    private float max_Run_Velocity = 1.0f;
    private float max_Walk_Velocity = 0.5f;

    [Header("Movement Abilities")]
    public bool canMoveForward = true;
    public bool canMoveBackward = false;
    public bool canMoveLeft = false;
    public bool canMoveRight = false;
    public bool canRun = false;
    public bool haveTeleport = false;
    public bool canTeleport = false;
    public bool haveRecall = false;
    public bool canRecall = false; // 新增召回能力
    public bool haveDelete = false;
    public bool canDelete = false;
    public bool haveAdd = false;
    public bool canAdd = false;
    public bool canInteraction = false;
    public bool haveIronKey = false;
    public bool haveCopperKey = false;
    public bool haveSilverKey = false;
    public bool haveGoldenKey = false;

    [Header("Teleport Settings")]
    public float teleportDistance = 5f;
    public float teleportCooldown = 1f;
    public LayerMask teleportLayerMask = 1;
    public GameObject teleportEffect;
    public GameObject unableTeleportEffect;

    [Header("Recall Settings")] // 新增召回设置
    public float recallCooldown = 2f;
    public GameObject recallEffect;
    public GameObject markEffect; // 标记位置的特效

    Vector2 currentMoveInput;
    Vector3 currentMove;
    float velocity_A = 0.0f;
    bool MovePressed;
    bool RunPressed;
    bool TeleportPressed;
    private Vector3 velocity;

    //传送相关变量
    private float lastTeleportTime;
    private bool isTeleporting = false;

    //召回相关变量
    private Vector3 recalledPosition; // 记录的位置
    private bool hasRecordedPosition = false; // 是否已记录位置
    private float lastRecallTime;
    private bool isRecalling = false;

    // Animator 参数哈希
    int VelocityHash;
    int GroundedHash;

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();

        Vector2 filteredInput = FilterMovementInput(currentMoveInput);

        currentMove.x = filteredInput.x;
        currentMove.z = filteredInput.y;
        MovePressed = filteredInput.x != 0 || filteredInput.y != 0;
    }

    void onRunInput(InputAction.CallbackContext context)
    {
        if (canRun)
        {
            RunPressed = context.ReadValue<float>() > 0.5f;
        }
    }

    void onTeleportInput(InputAction.CallbackContext context)
    {
        if (canTeleport) // 只有允许传送时才响应
        {
            TeleportPressed = context.ReadValue<float>() > 0.5f;
        }
    }

    // 新增召回输入处理
    void onRecallInput(InputAction.CallbackContext context)
    {
        if (canRecall && !canTeleport) // 只有允许召回且不允许传送时才响应
        {
            HandleRecall();
        }
    }

    private Vector2 FilterMovementInput(Vector2 input)
    {
        Vector2 filtered = Vector2.zero;

        if (input.x < 0 && canMoveLeft)
            filtered.x = input.x;
        else if (input.x > 0 && canMoveRight)
            filtered.x = input.x;

        if (input.y > 0 && canMoveForward)
            filtered.y = input.y;
        else if (input.y < 0 && canMoveBackward)
            filtered.y = input.y;

        return filtered;
    }

    public void UnlockMovementDirection(string direction)
    {
        switch (direction.ToLower())
        {
            case "forward":
                canMoveForward = true;
                break;
            case "backward":
                canMoveBackward = true;
                break;
            case "left":
                canMoveLeft = true;
                break;
            case "right":
                canMoveRight = true;
                break;
            case "run":
                canRun = true;
                break;
            case "teleport":
                haveTeleport = true;
                canTeleport = true;
                canRecall = false;
                break;
            case "recall": 
                canRecall = true;
                haveRecall = true;
                canTeleport = false;
                break;
            case "delete":
                haveDelete = true;
                canDelete = true;
                canAdd = false;
                break;
            case "add": 
                canAdd = true;
                haveAdd = true;
                canDelete = false;
                break;
            case "interaction": 
                canInteraction = true;
                break;
            case "copper": 
                haveCopperKey = true;
                break;
            case "silver": 
                haveSilverKey = true;
                break;
            case "golden": 
                haveGoldenKey = true;
                break;
            case "iron": 
                haveIronKey = true;
                break;
            default:
                Debug.LogWarning("未知的移动方向: " + direction);
                break;
        }
    }

    public void ChangeMovementDirectionLock(string direction)
    {
        switch (direction.ToLower())
        {
            case "forward":
                canMoveForward = !canMoveForward;
                break;
            case "backward":
                canMoveBackward = !canMoveBackward;
                break;
            case "left":
                canMoveLeft = !canMoveLeft;
                break;
            case "right":
                canMoveRight = !canMoveRight;
                break;
            case "run":
                canRun = !canRun;
                break;
            case "teleport":
                if (haveTeleport) canTeleport = !canTeleport;
                break;
            case "recall":
                if (haveRecall) canRecall = !canRecall;
                break;
            case "delete":
                if (haveDelete) canDelete = !canDelete;
                break;
            case "add":
                if (haveAdd) canAdd = !canAdd;
                break;
            default:
                Debug.LogWarning("未知的移动方向: " + direction);
                break;
        }
    }

    public bool HasMovementAbility(string direction)
    {
        switch (direction.ToLower())
        {
            case "forward": return canMoveForward;
            case "backward": return canMoveBackward;
            case "left": return canMoveLeft;
            case "right": return canMoveRight;
            case "run": return canRun;
            case "teleport": return canTeleport;
            case "recall": return canRecall; 
            default: return false;
        }
    }

    void handleMovement()
    {
        Vector3 move = Vector3.zero;
        if (MovePressed)
        {
            float speed = RunPressed ? RunSpeed : WalkSpeed;

            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            move = (camRight * currentMove.x + camForward * currentMove.z) * speed;
        }

        characterController.Move(move * Time.deltaTime);
    }
    
    // 传送功能
    void HandleTeleport()
    {
        if (!canTeleport || isTeleporting) return;
        
        // 检查冷却时间
        if (Time.time - lastTeleportTime < teleportCooldown) return;
        
        if (TeleportPressed)
        {
            StartCoroutine(PerformTeleport());
        }
    }
    
    IEnumerator PerformTeleport()
    {
        isTeleporting = true;
        
        Vector3 teleportDirection = transform.forward;
        teleportDirection.y = 0; 
        teleportDirection.Normalize();
        
        if (Mathf.Abs(teleportDirection.x) > Mathf.Abs(teleportDirection.z))
        {
            teleportDirection = new Vector3(Mathf.Sign(teleportDirection.x), 0, 0);
        }
        else
        {
            teleportDirection = new Vector3(0, 0, Mathf.Sign(teleportDirection.z));
        }
        
        Vector3 rayStart = transform.position - Vector3.up * 0.5f;
        Vector3 stoneRayStart = transform.position + Vector3.up * 0.5f;

        RaycastHit[] stoneHits = Physics.RaycastAll(stoneRayStart, teleportDirection, teleportDistance, teleportLayerMask);

        foreach (RaycastHit h in stoneHits)
        {
            if (h.collider.CompareTag("Stone"))
            {
                Debug.Log("无法传送通过障碍");
                if (unableTeleportEffect != null) Instantiate(unableTeleportEffect, transform.position, Quaternion.identity);
                yield break;
            }
        }
        
        // 使用RaycastAll获取所有命中物体
        RaycastHit[] hits = Physics.RaycastAll(rayStart, teleportDirection, teleportDistance, teleportLayerMask);
        
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        // 找到第一个不是AirWall的物体
        RaycastHit? validHit = null;
        foreach (RaycastHit h in hits)
        {
            if (!h.collider.CompareTag("AirWall"))
            {
                validHit = h;
                break;
            }
        }
        
        if (validHit.HasValue)
        {
            RaycastHit hit = validHit.Value;
            
            Vector3 targetPosition = hit.point;
            
            targetPosition.y = hit.collider.bounds.max.y;
            targetPosition += teleportDirection;
            
            yield return new WaitForSeconds(0.1f);
            
            characterController.enabled = false;
            transform.position = targetPosition;
            characterController.enabled = true;
            
            Debug.Log("传送到位置: " + targetPosition + "\n传送物体名称: " + hit.collider.tag);
        }
        else
        {
            Debug.Log("前方没有可传送的方块");
        }
        
        lastTeleportTime = Time.time;
        isTeleporting = false;
    }

    // 检查玩家脚下是否为Block
    private bool IsOnBlock()
    {
        // 从玩家位置向下发射射线检测脚下的方块
        RaycastHit hit;
        float raycastDistance = 1.5f; // 射线距离，根据角色高度调整
        
        // 射线起点在玩家中心位置稍微上方，方向向下
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance))
        {
            // 检查命中的物体是否为Block
            if (hit.collider.CompareTag("Block"))
            {
                return true;
            }
        }
        
        return false;
    }

    // 新增召回功能
    void HandleRecall()
    {
        if (!canRecall || canTeleport || isRecalling) return;
        
        // 检查冷却时间
        if (Time.time - lastRecallTime < recallCooldown) return;
        
        if (!hasRecordedPosition)
        {
            // 检查玩家是否站在Block上
            if (!IsOnBlock())
            {
                Debug.Log("无法记录位置：玩家必须站在Block上才能记录");
                return;
            }
            
            // 记录当前位置
            recalledPosition = transform.position;
            hasRecordedPosition = true;
            
            // 播放标记特效
            if (markEffect != null)
            {
                Instantiate(markEffect, recalledPosition, Quaternion.identity);
            }
            
            Debug.Log("位置已记录: " + recalledPosition);
        }
        else
        {
            // 执行召回
            StartCoroutine(PerformRecall());
        }
        
        lastRecallTime = Time.time;
    }
    
    IEnumerator PerformRecall()
    {
        isRecalling = true;
        
        // 播放召回特效（开始）
        if (recallEffect != null)
        {
            Instantiate(recallEffect, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.1f);
        
        characterController.enabled = false;
        transform.position = recalledPosition;
        characterController.enabled = true;
        
        // 播放召回特效（结束）
        if (recallEffect != null)
        {
            Instantiate(recallEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("召回至位置: " + recalledPosition);
        
        // 召回后重置记录状态
        hasRecordedPosition = false;
        isRecalling = false;
    }
    
    // 可视化传送检测射线和脚下检测（调试用）
    void OnDrawGizmosSelected()
    {
        if (canTeleport && !isTeleporting)
        {
            Gizmos.color = Color.blue;
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;
            
            // 使用与传送相同的方向计算逻辑
            Vector3 teleportDirection = transform.forward;
            teleportDirection.y = 0;
            teleportDirection.Normalize();
            
            // 限制只能向X轴或Z轴传送
            if (Mathf.Abs(teleportDirection.x) > Mathf.Abs(teleportDirection.z))
            {
                teleportDirection = new Vector3(Mathf.Sign(teleportDirection.x), 0, 0);
            }
            else
            {
                teleportDirection = new Vector3(0, 0, Mathf.Sign(teleportDirection.z));
            }
            
            Gizmos.DrawRay(rayStart, teleportDirection * teleportDistance);
        }
        
        // 可视化记录的位置（调试用）
        if (hasRecordedPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(recalledPosition, 0.5f);
            Gizmos.DrawLine(transform.position, recalledPosition);
        }
        
        // 可视化脚下检测射线（调试用）
        if (canRecall)
        {
            Gizmos.color = Color.yellow;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawRay(rayStart, Vector3.down * 1.5f);
        }
    }

    void handleAnimation()
    {
        float targetVelocity = 0f;

        if (MovePressed)
        {
            targetVelocity = RunPressed ? max_Run_Velocity : max_Walk_Velocity;
        }

        if (velocity_A < targetVelocity)
        {
            velocity_A += Time.deltaTime * acceleration;
            if (velocity_A > targetVelocity) velocity_A = targetVelocity;
        }
        else if (velocity_A > targetVelocity)
        {
            velocity_A -= Time.deltaTime * deceleration;
            if (velocity_A < targetVelocity) velocity_A = targetVelocity;
        }

        animator.SetFloat(VelocityHash, velocity_A);
    }

    void handleRotation()
    {
        if (MovePressed)
        {
            // 获取摄像机方向
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            // 计算基于摄像机方向的移动向量
            Vector3 moveDirection = (camRight * currentMoveInput.x + camForward * currentMoveInput.y).normalized;
            
            // 只有当移动方向有有效值时才旋转
            if (moveDirection != Vector3.zero)
            {
                // 创建目标旋转
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                
                // 平滑旋转到目标方向
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationFactorPerFrame * Time.deltaTime * 2);
            }
        }
    }

    void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        VelocityHash = Animator.StringToHash("speed");
        GroundedHash = Animator.StringToHash("isGrounded");

        playerInput.player.move.performed += onMovementInput;
        playerInput.player.move.canceled += onMovementInput;

        playerInput.player.run.performed += onRunInput;
        playerInput.player.run.canceled += onRunInput;
        
        playerInput.player.run.performed += onTeleportInput;
        playerInput.player.run.canceled += onTeleportInput;
        
        // 新增召回输入绑定
        playerInput.player.run.performed += onRecallInput;
    }

    void OnDisable()
    {
        playerInput.Disable();
    }

    void Update()
    {
        handleMovement();
        handleRotation();
        handleAnimation();
        HandleTeleport();
        // 召回功能在输入回调中处理，不需要在Update中调用
    }
}