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

    Vector2 currentMoveInput;
    Vector3 currentMove;
    float velocity_A = 0.0f;
    bool MovePressed;
    bool RunPressed;
    bool JumpPressed;
    private Vector3 velocity;

    // 跳跃相关变量
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;
    private bool isJumping = false;

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
                Debug.Log("解锁向前移动能力!");
                break;
            case "backward":
                canMoveBackward = true;
                Debug.Log("解锁向后移动能力!");
                break;
            case "left":
                canMoveLeft = true;
                Debug.Log("解锁向左移动能力!");
                break;
            case "right":
                canMoveRight = true;
                Debug.Log("解锁向右移动能力!");
                break;
            case "run":
                canRun = true;
                Debug.Log("解锁奔跑能力!");
                break;
            case "jump":
                canJump = true;
                Debug.Log("解锁跳跃能力!");
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

        // 地面检测和跳跃处理
        HandleGravityAndJump(ref move);

        characterController.Move(move * Time.deltaTime);
    }

    void HandleGravityAndJump(ref Vector3 move)
    {
        bool isGrounded = characterController.isGrounded;

        // 更新动画状态
        animator.SetBool(GroundedHash, isGrounded);

        // 落地处理
        if (isGrounded)
        {
            fallTimeoutDelta = fallTimeout;

            animator.SetBool(JumpHash, false);

            // 停止y轴速度，但保持一个小的向下力让角色紧贴地面
            if (velocity.y < 0.0f)
            {
                velocity.y = -2f;
                isJumping = false;
            }

            // 跳跃
            if (JumpPressed && jumpTimeoutDelta <= 0.0f && canJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                animator.SetBool(JumpHash, true);
                isJumping = true;
            }

            // 跳跃超时
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            jumpTimeoutDelta = jumpTimeout;

            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }

            // 松开跳跃键时减少跳跃高度（实现可变高度跳跃）
            if (!JumpPressed && velocity.y > 0.0f && isJumping)
            {
                velocity.y += gravity * Time.deltaTime * 0.5f;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        // 应用垂直速度
        move.y = velocity.y;
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
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMove.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMove.z;

        Quaternion currentRotation = transform.rotation;

        if (MovePressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
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
    }

}