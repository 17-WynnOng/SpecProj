using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float verticalClampAngle = 80f;

    private PlayerInput playerInput;
    private CharacterController characterController;

    //Movement
    private Vector2 moveInput;
    private InputAction moveAction;

    //Camera
    private InputAction lookAction;
    private Vector2 mouseDelta;
    private float verticalRotation = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Movement"];
        lookAction = playerInput.actions["Look"];
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMove();

        moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    public void HandleLook()
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

    public void HandleMove()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }
}
