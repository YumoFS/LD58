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
    public float WalkSpeed = 2.0f;
    public float RunSpeed = 4.0f;
    public float gravity = -100f;
    public float rotationFactorPerFrame = 5.0f;

    [Header("Animation Settings")]
    public float acceleration = 0.8f;
    public float deceleration = 0.8f;
    private float max_Run_Velocity = 1.0f;
    private float max_Walk_Velocity = 0.5f;

    [Header("Movement Abilities")]
    public bool canMoveForward = true;    // 初始可以向前移动
    public bool canMoveBackward = false;  // 初始不能向后移动
    public bool canMoveLeft = false;      // 初始不能向左移动
    public bool canMoveRight = false;     // 初始不能向右移动
    public bool canRun = false;     // 初始不能奔跑

    Vector2 currentMoveInput;
    Vector3 currentMove;
    float velocity_A = 0.0f;
    bool MovePressed;
    bool RunPressed;
    private Vector3 velocity;

    int VelocityHash;

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
        
        // 根据解锁的能力过滤输入
        Vector2 filteredInput = FilterMovementInput(currentMoveInput);
        
        currentMove.x = filteredInput.x;
        currentMove.z = filteredInput.y;
        MovePressed = filteredInput.x != 0 || filteredInput.y != 0;
    }

    // 根据解锁的方向过滤输入
    private Vector2 FilterMovementInput(Vector2 input)
    {
        Vector2 filtered = Vector2.zero;
        
        // 水平移动 (A/D 键)
        if (input.x < 0 && canMoveLeft)    // 向左
            filtered.x = input.x;
        else if (input.x > 0 && canMoveRight) // 向右
            filtered.x = input.x;
        
        // 垂直移动 (W/S 键)
        if (input.y > 0 && canMoveForward)  // 向前
            filtered.y = input.y;
        else if (input.y < 0 && canMoveBackward) // 向后
            filtered.y = input.y;
        
        return filtered;
    }

    void onRunInput(InputAction.CallbackContext context)
    {
        RunPressed = context.ReadValue<float>() > 0.5f;
    }

    // 解锁移动方向的方法
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
            default:
                Debug.LogWarning("未知的移动方向: " + direction);
                break;
        }
    }

    // 检查当前是否解锁了某个方向
    public bool HasMovementAbility(string direction)
    {
        switch (direction.ToLower())
        {
            case "forward": return canMoveForward;
            case "backward": return canMoveBackward;
            case "left": return canMoveLeft;
            case "right": return canMoveRight;
            case "run": return canRun;
            default: return false;
        }
    }

    void hanleMovement()
    {
        Vector3 move = Vector3.zero;
        if (MovePressed)
        {
            float speed = 0f;
            if (canRun) speed = RunPressed ? RunSpeed : WalkSpeed;
            else speed = WalkSpeed;

            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            move = (camRight * currentMove.x + camForward * currentMove.z) * speed;
        }

        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime * 10000;
        move.y = velocity.y;
        characterController.Move(move * Time.deltaTime);
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

        if (MovePressed && currentMove.z >= 0) {
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

        playerInput.player.move.performed += onMovementInput;
        playerInput.player.move.canceled += onMovementInput;
        playerInput.player.run.performed += onRunInput;
        playerInput.player.run.canceled += onRunInput;
    }

    void OnDisable() {
        playerInput.Disable();
    }

    void Update()
    {
        hanleMovement();
        handleRotation();
        handleAnimation();
    }
}