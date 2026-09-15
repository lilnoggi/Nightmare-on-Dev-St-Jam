using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Exploration, Sprinting, Hiding }
    public PlayerState currentState = PlayerState.Exploration;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float gravity = -9.81f;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 25f;
    public float staminaRegenRate = 10f;
    public float hidingRegenMultiplier = 2.5f; 

    private CharacterController controller;
    private InputSystem_Actions inputActions; 
    private float currentMoveInput;
    private Vector3 velocity;

    // ---------------------------------------------------------------

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();
        currentStamina = maxStamina;

        // Map the Input System callbacks
        inputActions.Player.Sprint.started += ctx => OnSprintStart();
        inputActions.Player.Sprint.canceled += ctx => OnSprintCancel();
        inputActions.Player.Interact.performed += ctx => OnInteract();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        currentMoveInput = inputActions.Player.Move.ReadValue<float>();
        
        HandleStamina();
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (currentState == PlayerState.Hiding)
        {
            return; // Lock movement if hiding
        }

        float speed = (currentState == PlayerState.Sprinting) ? sprintSpeed : walkSpeed;
        
        // 2.5D Horizontal Movement (X axis)
        Vector3 move = new Vector3(currentMoveInput, 0f, 0f);
        controller.Move(move * speed * Time.deltaTime);

        // Apply simple gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleStamina()
    {
        if (currentState == PlayerState.Sprinting)
        {
            // Only drain if actively pressing movement keys
            if (Mathf.Abs(currentMoveInput) > 0.1f) 
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    currentState = PlayerState.Exploration; // Exhausted, force walk
                }
            }
        }
        else
        {
            // Regenerate based on current state
            float regenRate = (currentState == PlayerState.Hiding) ? staminaRegenRate * hidingRegenMultiplier : staminaRegenRate;
            currentStamina += regenRate * Time.deltaTime;
        }
        
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    private void OnSprintStart()
    {
        if (currentState == PlayerState.Exploration && currentStamina > 0)
        {
            currentState = PlayerState.Sprinting;
        }
    }

    private void OnSprintCancel()
    {
        if (currentState == PlayerState.Sprinting)
        {
            currentState = PlayerState.Exploration;
        }
    }

    private void OnInteract()
    {
        Debug.Log("Interact Pressed - Firing door/hiding logic!");
    }
}