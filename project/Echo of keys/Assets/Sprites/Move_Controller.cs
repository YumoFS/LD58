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

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;
    public float jumpTimeout = 0.1f; // 防止连续跳跃的时间间隔
    public float fallTimeout = 0.15f; // 落地检测的时间间隔

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
    public bool canJump = true; // 可以控制是否允许跳跃
    public bool canTeleport = false;

    [Header("Teleport Settings")]
    public float teleportDistance = 10f;
    public float teleportCooldown = 1f;
    public LayerMask teleportLayerMask = 1; // 可传送到的层级
    public GameObject teleportEffect; // 传送特效（可选）

    Vector2 currentMoveInput;
    Vector3 currentMove;
    float velocity_A = 0.0f;
    bool MovePressed;
    bool RunPressed;
    bool JumpPressed;
    bool TeleportPressed;
    private Vector3 velocity;

    // 跳跃相关变量
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;
    private bool isJumping = false;

    private float lastTeleportTime;
    private bool isTeleporting = false;

    // Animator 参数哈希
    int VelocityHash;
    int JumpHash;
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

    void onJumpInput(InputAction.CallbackContext context)
    {
        if (canJump) // 只有允许跳跃时才响应
        {
            JumpPressed = context.ReadValue<float>() > 0.5f;
        }
    }

    void onTeleportInput(InputAction.CallbackContext context)
    {
        if (canTeleport) // 只有允许传送时才响应
        {
            TeleportPressed = context.ReadValue<float>() > 0.5f;
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
                // Debug.Log("解锁向前移动能力!");
                break;
            case "backward":
                canMoveBackward = true;
                // Debug.Log("解锁向后移动能力!");
                break;
            case "left":
                canMoveLeft = true;
                // Debug.Log("解锁向左移动能力!");
                break;
            case "right":
                canMoveRight = true;
                // Debug.Log("解锁向右移动能力!");
                break;
            case "run":
                canRun = true;
                // Debug.Log("解锁奔跑能力!");
                break;
            case "jump":
                canJump = true;
                // Debug.Log("解锁跳跃能力!");
                break;
            case "teleport":
                canTeleport = true;
                // Debug.Log("解锁奔跑能力!");
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
            case "jump":
                canJump = !canJump;
                break;
            case "teleport":
                canTeleport = !canTeleport;
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
            case "jump": return canJump;
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
            
            // 可选：播放传送特效
            // if (teleportEffect != null)
            // {
            //     Instantiate(teleportEffect, transform.position, Quaternion.identity);
            // }

            yield return new WaitForSeconds(0.1f);
            
            characterController.enabled = false;
            transform.position = targetPosition;
            characterController.enabled = true;
            
            // 可选：在目标位置播放传送特效
            // if (teleportEffect != null)
            // {
            //     Instantiate(teleportEffect, transform.position, Quaternion.identity);
            // }
            
            Debug.Log("传送到位置: " + targetPosition + "\n传送物体名称: " + hit.collider.tag);
        }
        else
        {
            Debug.Log("前方没有可传送的方块");
        }
        
        lastTeleportTime = Time.time;
        isTeleporting = false;
    }
    
    // 可视化传送检测射线（调试用）
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
        JumpHash = Animator.StringToHash("isJumping");
        GroundedHash = Animator.StringToHash("isGrounded");

        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;

        playerInput.player.move.performed += onMovementInput;
        playerInput.player.move.canceled += onMovementInput;

        playerInput.player.run.performed += onRunInput;
        playerInput.player.run.canceled += onRunInput;

        playerInput.player.jump.performed += onJumpInput;
        playerInput.player.jump.canceled += onJumpInput;
        
        playerInput.player.run.performed += onTeleportInput;
        playerInput.player.run.canceled += onTeleportInput;
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
    }

}