using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Config (Important)")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float verticalClampAngle = 80f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private Camera playerCamera;

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

    [Header("PlayerBuild(Important)")]
    [SerializeField] private PlayerBuild playerBuildMode;


    private PlayerInput playerInput;
    private CharacterController characterController;

    //Input Actions
    private Vector2 moveInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lmbAction;
    private InputAction mouseWheelAction;
    private InputAction buildAction;
    private InputAction reloadAction;
    private InputAction enterAction;

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
        buildAction = playerInput.actions["Build"];
        reloadAction = playerInput.actions["Reload"];
        enterAction = playerInput.actions["Enter"];

    }

    // Update is called once per frame
    void Update()
    {
        if (!controlsEnabled)
            return;
        HandleBuildModeToggle();

        // Always allow movement/look/jump
        Gravity();
        HandleLook();
        HandleMove();
        HandleJump();
        HandleEnter();


        // Weapon switching should probably still work, even in build mode
        HandleScrollSwitch();

        // Only fire when *not* in build mode
        HandleShoot();
        HandleReload();

        // Still let the player place turrets when in build mode
        if (playerBuildMode.buildMode)
            HandleBuildMode();

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
        // don’t ever shoot if we’re in build mode
        if (playerBuildMode.buildMode || playerLoadout.heldWeapon.isReloading)
            return;

        if (playerLoadout.heldWeapon == null)
            return;

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

    private void HandleReload()
    {
        if (reloadAction.WasPressedThisFrame())
        {
            playerLoadout.heldWeapon.Reload();
        }
    }

    private void HandleScrollSwitch()
    {
        // don’t scroll if we’re still on cooldown
        if (Time.time < nextScrollTime)
            return;

        // read the wheel once
        float scrollY = mouseWheelAction.ReadValue<Vector2>().y;
        if (scrollY == 0f)
            return;

        // advance the cooldown
        nextScrollTime = Time.time + scrollCooldown;

        if (!playerBuildMode.buildMode)
        {
            // normal weapon switching
            if (scrollY > 0f)
                playerLoadout.SwitchToNextWeapon();
            else
                playerLoadout.SwitchToPreviousWeapon();
        }
        else
        {
            // in build mode, switch which sentry you’ll place
            if (scrollY > 0f)
                playerLoadout.SwitchToNextSentry();
            else
                playerLoadout.SwitchToPreviousSentry();
        }
    }

    public void EnableControls(bool toggle)
    {
        controlsEnabled = toggle;
    }

    private void HandleBuildModeToggle()
    {
        if (buildAction.WasPressedThisFrame())
        {
            playerBuildMode.TryBuildToggle(playerLoadout);
        }
    }

    private void HandleBuildMode()
    {
        if (playerCamera != null)
        {
            // On left click, place the turret
            if (lmbAction.WasPressedThisFrame())
            {
                playerBuildMode.TryPlaceSentry(playerCamera, playerLoadout);
            }
        }
    }

    private void HandleEnter()
    {
        if (enterAction.WasPressedThisFrame())
        {
            GameManager.Instance.EndCountdown();
        }
    }
}
