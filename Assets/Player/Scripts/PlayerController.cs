using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Config (Important)")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField, Range(0f, 10f)] private float horizontalSensitivity, verticalSensitivity;

    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float verticalClampAngle = 80f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private Camera playerCamera;


    [Header("PlayerLoadout (Important)")]
    public PlayerLoadout playerLoadout;

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
    [SerializeField] private float recoilSnapSpeed = 15f;     // how fast recoil applies while firing
    [SerializeField] private float recoilRecoverySpeed = 8f;
    [SerializeField] private float maxRecoilPitch = 30f;      // how far up/down recoil can push
    [SerializeField] private float maxRecoilYaw = 10f;        // how far side-to-side recoil can push

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
    private PlayerHealth playerHealth;

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
    private InputAction escAction;
    private InputAction skipAction;

    //Camera
    private InputAction lookAction;
    private Vector2 mouseDelta;
    private float verticalRotation = 0f;

    //Walking
    private bool isWalking;

    private bool isFiring;

    //Gravity
    private Vector3 velocity;
    private bool isGrounded;

    private bool controlsEnabled = false;

    //Recoil
    private float targetRecoilX = 0f;
    private float targetRecoilY = 0f;
    private float currentRecoilX = 0f;
    private float currentRecoilY = 0f;

    //Yaw
    private float yawAngle = 0f;
    
    //Pause
    private bool isPaused;
    private GameObject pauseMenu;

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();

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
        escAction = playerInput.actions["Esc"];
        skipAction = playerInput.actions["Skip"];

    }

    void Start()
    {
        initialWeaponPos = weaponHolder.localPosition;
        pauseMenu = UIManager.Instance.pauseMenu;
    }

    // Update is called once per frame
    void Update()
    {
        HandlePause();

        if (GameManager.Instance.isExtracted)
            return;

        if (!controlsEnabled || isPaused)
            return;

        HandleBuildModeToggle();
        HandleSecondaryAction();
        HandleSkip();

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

    private void HandleSkip()
    {
        if (skipAction.WasPressedThisFrame())
        {
            GameManager.Instance.isSectorClear = true;
        }
    }

    private void HandleLook()
    {
        // 1) Mouse input (use a single clamp, remove extra normalize-to-5 cap)
        Vector2 raw = lookAction.ReadValue<Vector2>();

        float mouseX = raw.x * horizontalSensitivity;
        float mouseY = raw.y * verticalSensitivity;

        // 2) Base aim from mouse (explicit yaw angle instead of Rotate)
        yawAngle += mouseX;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);

        // 3) Recoil smoothing
        float snapLerp = 1f - Mathf.Exp(-recoilSnapSpeed * Time.deltaTime);
        float recoveryLerp = 1f - Mathf.Exp(-recoilRecoverySpeed * Time.deltaTime);

        float prevRecoilX = currentRecoilX;
        float prevRecoilY = currentRecoilY;

        // Snap toward target while firing
        currentRecoilX = Mathf.Lerp(currentRecoilX, targetRecoilX, snapLerp);
        currentRecoilY = Mathf.Lerp(currentRecoilY, targetRecoilY, snapLerp);

        // Not firing → targets go to 0 and current follows
        if (!isFiring)
        {
            targetRecoilX = Mathf.Lerp(targetRecoilX, 0f, recoveryLerp);
            targetRecoilY = Mathf.Lerp(targetRecoilY, 0f, recoveryLerp);

            currentRecoilX = Mathf.Lerp(currentRecoilX, targetRecoilX, recoveryLerp);
            currentRecoilY = Mathf.Lerp(currentRecoilY, targetRecoilY, recoveryLerp);

            // Compensation so crosshair stays put during recovery
            float dPitch = currentRecoilX - prevRecoilX;
            float dYaw = currentRecoilY - prevRecoilY;

            verticalRotation -= dPitch;  // pitch compensation
            yawAngle -= dYaw;    // yaw compensation (no extra Rotate calls)
        }

        // 4) Apply final rotations ONCE (no transform.Rotate)
        float finalPitch = Mathf.Clamp(verticalRotation + currentRecoilX,
                                       -verticalClampAngle, verticalClampAngle);

        // Yaw on the body, pitch on the camera
        transform.localRotation = Quaternion.Euler(0f, yawAngle + currentRecoilY, 0f);

        float currentRoll = cameraHolder.localRotation.eulerAngles.z; // keep strafe tilt roll
        cameraHolder.localRotation = Quaternion.Euler(finalPitch, 0f, currentRoll);
    }
    
    public void ApplyRecoil(float amount)
    {
        // Vertical (pitch) recoil
        targetRecoilX = Mathf.Max(targetRecoilX - amount, -maxRecoilPitch);

        // Horizontal (yaw) recoil
        // Randomly choose left or right kick for variation
        float yawKick = Random.Range(-amount * 0.1f, amount * 0.1f);
        targetRecoilY = Mathf.Clamp(targetRecoilY + yawKick, -maxRecoilYaw, maxRecoilYaw);
    }

    private void HandleMove()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        isWalking = moveInput.magnitude > 0.1f && isGrounded;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        if (playerLoadout.heldWeapon != null && playerLoadout.heldWeapon.animator != null && isGrounded)
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

        isFiring = false;

        switch (playerLoadout.heldWeapon.weaponData.fireMode)
        {
            case FireMode.FullAuto:
                if (lmbAction.IsPressed())
                {
                    isFiring = playerLoadout.heldWeapon.Shoot();
                }
                break;

            case FireMode.SemiAuto:
                if (lmbAction.WasPressedThisFrame())
                {
                    isFiring = playerLoadout.heldWeapon.Shoot();
                }
                break;
        }

        if (isFiring)
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

    private void HandlePause()
    {
        if (escAction.WasPressedThisFrame())
        {
            if (isPaused) 
                ResumeGame();
            else 
                PauseGame();
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

    public void PauseGame()
    {
        isPaused = true;

        // Stop game time & audio
        Time.timeScale = 0f;

        // Show cursor + menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (pauseMenu) pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Restore time & audio
        Time.timeScale = 1f;

        // Hide cursor + menu
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (pauseMenu) pauseMenu.SetActive(false);
    }

    // Safety: if this object gets disabled while paused, unpause time.
    private void OnDisable()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
