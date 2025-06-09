using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Config")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float verticalClampAngle = 80f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform groundCheck; // small empty GameObject at player feet
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Scrollwheel Cooldown")]
    [SerializeField] private float scrollCooldown = 0.2f;
    private float nextScrollTime = 0f;

    [Header("PlayerLoadout (Important)")]
    [SerializeField] private PlayerLoadout playerLoadout;

    private PlayerInput playerInput;
    private CharacterController characterController;

    //Input Actions
    private Vector2 moveInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lmbAction;
    private InputAction mouseWheelAction;

    //Camera
    private InputAction lookAction;
    private Vector2 mouseDelta;
    private float verticalRotation = 0f;

    //Gravity
    private Vector3 velocity;
    private bool isGrounded;

    private bool controlsEnabled = false;


    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Movement"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        lmbAction = playerInput.actions["LeftMouse"];
        mouseWheelAction = playerInput.actions["MouseWheel"];
    }

    // Update is called once per frame
    void Update()
    {
        if (!controlsEnabled)
            return;

        Gravity();
        HandleLook();
        HandleMove();
        HandleJump();
        HandleShoot();
        HandleScrollSwitch();

        moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }
    
    private void HandleLook()
    {
        mouseDelta = lookAction.ReadValue<Vector2>();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        // Rotate player body (horizontal look)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera root (vertical look)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);

        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleMove()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void Gravity()
    {
        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (isGrounded && jumpAction.WasPressedThisFrame())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void HandleShoot()
    {
        if (playerLoadout.heldWeapon == null) return;

        switch (playerLoadout.heldWeapon.weaponData.fireMode)
        {
            case FireMode.FullAuto:
                if (lmbAction.IsPressed())
                    playerLoadout.heldWeapon.Shoot();
                break;

            case FireMode.SemiAuto:
                if (lmbAction.WasPressedThisFrame())
                    playerLoadout.heldWeapon.Shoot();
                break;
        }
    }

    private void HandleScrollSwitch()
    {
        if (Time.time < nextScrollTime) return;

        float scrollY = mouseWheelAction.ReadValue<Vector2>().y;

        if (scrollY > 0f)
        {
            playerLoadout.SwitchToNextWeapon();
            nextScrollTime = Time.time + scrollCooldown;
        }
        else if (scrollY < 0f)
        {
            playerLoadout.SwitchToPreviousWeapon();
            nextScrollTime = Time.time + scrollCooldown;
        }
    }

    public void EnableControls(bool toggle)
    {
        controlsEnabled = toggle;
        GameManager.Instance.allowSpawning = toggle;
    }
}
