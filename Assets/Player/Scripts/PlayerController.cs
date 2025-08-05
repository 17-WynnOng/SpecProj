using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Config (Important)")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField, Range(0f, 0.1f)] private float mouseSensitivity = 0.1f;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float verticalClampAngle = 80f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private Camera playerCamera;


    [Header("PlayerLoadout (Important)")]
    [SerializeField] public PlayerLoadout playerLoadout;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform groundCheck; // small empty GameObject at player feet
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Collectible Attract")]
    [SerializeField] private float attractRadius = 3f;
    [SerializeField] private LayerMask collectibleLayer;

    [Header("Interact Range")]
    [SerializeField] private float interactRange = 1f;

    [Header("Scrollwheel Cooldown")]
    [SerializeField] private float scrollCooldown = 0.2f;
    private float nextScrollTime = 0f;

    [Header("Recoil Recovery")]
    [SerializeField] private float recoilSnapSpeed = 15f;

    [Header("Weapon Sway")]
    [SerializeField] private Transform weaponHolder; // Assign in inspector (usually the weapon parent)
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float swaySmooth = 6f;

    [Header("Camera Strafe Tilt")]
    [SerializeField] private float strafeTiltAmount = 5f;
    [SerializeField] private float strafeTiltSpeed = 5f;

    private Vector3 initialWeaponPos;

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
    private InputAction interactAction;
    private InputAction rmbAction;

    //Camera
    private InputAction lookAction;
    private Vector2 mouseDelta;
    private float verticalRotation = 0f;

    //Walking
    private bool isWalking;

    //Gravity
    private Vector3 velocity;
    private bool isGrounded;

    private bool controlsEnabled = false;

    //Recoil
    private float targetRecoilX = 0f;
    private float targetRecoilY = 0f;
    private float currentRecoilX = 0f;
    private float currentRecoilY = 0f;


    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Movement"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        lmbAction = playerInput.actions["LeftMouse"];
        rmbAction = playerInput.actions["RightMouse"];
        mouseWheelAction = playerInput.actions["MouseWheel"];
        buildAction = playerInput.actions["Build"];
        reloadAction = playerInput.actions["Reload"];
        enterAction = playerInput.actions["Enter"];
        interactAction = playerInput.actions["EKey"];

    }

    void Start()
    {
        initialWeaponPos = weaponHolder.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!controlsEnabled)
            return;
        HandleBuildModeToggle();
        HandleSecondaryAction();

        // Always allow movement/look/jump
        Gravity();
        HandleLook();
        HandleCameraStrafeTilt();
        HandleWeaponSway();
        HandleMove();
        HandleJump();
        HandleEnter();
        HandleInteract();


        // Weapon switching should probably still work, even in build mode
        HandleScrollSwitch();

        // Only fire when *not* in build mode
        HandleShoot();
        HandleReload();


        HandleBuildMode();
    }

    private void FixedUpdate()
    {
        if (!controlsEnabled)
            return;

        AttractNearbyCollectibles();
    }

    private void HandleWeaponSway()
    {
        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        Vector2 moveDelta = moveAction.ReadValue<Vector2>();

        //Mouse
        float swayX = -mouseDelta.x * swayAmount;
        float swayY = -mouseDelta.y * swayAmount;

        // Movement
        float moveSwayX = moveDelta.x * swayAmount * 5f; // lateral sway
        float moveSwayY = Mathf.Abs(moveDelta.y) * swayAmount * 5f; // forward/back

        Vector3 targetPosition = initialWeaponPos + new Vector3(swayX + moveSwayX, swayY + moveSwayY, 0f);
        weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition, targetPosition, Time.deltaTime * swaySmooth);
    }

    private void HandleCameraStrafeTilt()
    {
        float strafeInput = moveAction.ReadValue<Vector2>().x; // A/D input
        float targetZRotation = -strafeInput * strafeTiltAmount;

        Vector3 currentEuler = cameraHolder.localRotation.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(verticalRotation, 0f, targetZRotation);
        cameraHolder.localRotation = Quaternion.Slerp(cameraHolder.localRotation, targetRotation, Time.deltaTime * strafeTiltSpeed);
    }

    private void HandleLook()
    {
        
        mouseDelta = lookAction.ReadValue<Vector2>();

        // Scaled sensitivity
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // Horizontal Look
        transform.Rotate(Vector3.up * mouseX); // yaw

        // Recoil
        currentRecoilX = Mathf.Lerp(currentRecoilX, targetRecoilX, recoilSnapSpeed * Time.deltaTime);
        currentRecoilY = Mathf.Lerp(currentRecoilY, targetRecoilY, recoilSnapSpeed * Time.deltaTime);

        // Vertical Look
        verticalRotation -= mouseY;            // normal mouse input
        verticalRotation += currentRecoilX;    // pitch recoil
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);

        // Camera Rotation
        Quaternion currentRotation = cameraHolder.localRotation;
        float currentRoll = currentRotation.eulerAngles.z;

        // Apply pitch and roll (roll is preserved)
        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, currentRoll);
    }

    private void HandleMove()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        isWalking = moveInput.magnitude > 0.1f && isGrounded;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        if (playerLoadout.heldWeapon != null && playerLoadout.heldWeapon.animator != null)
        {
            playerLoadout.heldWeapon.animator.SetBool("Walking", isWalking);
        }
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
        if (PlayerBuild.Instance.buildMode || playerLoadout.heldWeapon.isReloading)
            return;

        if (playerLoadout.heldWeapon == null)
            return;

        bool shot = false;

        switch (playerLoadout.heldWeapon.weaponData.fireMode)
        {
            case FireMode.FullAuto:
                if (lmbAction.IsPressed())
                {
                    shot = playerLoadout.heldWeapon.Shoot();
                }
                break;

            case FireMode.SemiAuto:
                if (lmbAction.WasPressedThisFrame())
                {
                    shot = playerLoadout.heldWeapon.Shoot();
                }
                break;
        }

        if (shot)
        {
            ApplyRecoil(playerLoadout.heldWeapon.weaponData.recoil);

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

        if (!PlayerBuild.Instance.buildMode)
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
                playerLoadout.SwitchToNextDeployable();
            else
                playerLoadout.SwitchToPreviousDeployable();
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
            if (PlayerBuild.Instance.buildMode)
            {
                PlayerBuild.Instance.sellMode = false;
            }

            PlayerBuild.Instance.ToggleBuildMode(playerLoadout);
        }
    }

    private void HandleBuildMode()
    {
        if (playerCamera == null) return;

        if (PlayerBuild.Instance.sellMode)
        {
            PlayerBuild.Instance.UpdateSellTarget(playerCamera);

            if (lmbAction.WasPressedThisFrame())
            {
                PlayerBuild.Instance.TrySellDeployable(playerCamera, playerLoadout);
            }
        }
        else
        {
            PlayerBuild.Instance.UpdateGhostPosition(playerCamera, playerLoadout);

            if (lmbAction.WasPressedThisFrame())
            {
                PlayerBuild.Instance.TryPlaceDeployable(playerCamera, playerLoadout);
            }
        }
    }

    private void HandleSecondaryAction()
    {
        if (PlayerBuild.Instance.buildMode)
        {
            if (rmbAction.WasPressedThisFrame())
            {
                PlayerBuild.Instance.ToggleSellMode(playerLoadout);
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

    private void HandleInteract()
    {
        if (interactAction.WasPressedThisFrame())
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange)) // 3f = interaction distance
            {
                if (hit.collider.TryGetComponent<Deployable>(out var deployable))
                {
                    deployable.InteractWithDeployable();
                }
            }
        }
    }

    private void AttractNearbyCollectibles()
    {
        Collider[] collision = Physics.OverlapSphere(transform.position, attractRadius, collectibleLayer);
        foreach (var hit in collision)
        {
            if (hit.TryGetComponent<Collectible>(out var collectible))
            {
                collectible.StartFlyingToPlayer(transform); // Make this method public in Collectible
            }
        }
    }

    public void ApplyRecoil(float recoilAmount)
    {
        currentRecoilX -= recoilAmount; // Pitch (upward)
        currentRecoilY += Random.Range(-recoilAmount * 0.5f, recoilAmount * 0.5f); // Optional yaw sway
    }
}
