using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Exploration, Sprinting, Hiding }
    [SerializeField] private PlayerState _currentState = PlayerState.Exploration;

    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _sprintSpeed = 6f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Stamina System")]
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _currentStamina;
    [SerializeField] private float _staminaDrainRate = 25f;
    [SerializeField] private float _staminaRegenRate = 10f;
    [SerializeField] private float _hidingRegenMultiplier = 2.5f; 

    [Header("References")]
    [SerializeField] private LanternController lantern;

    private CharacterController _controller;
    private InputSystem_Actions _inputActions; 
    private float _currentMoveInput;
    private Vector3 _velocity;

    // ---------------------------------------------------------------

    private void Awake()
    {
        _controller = GetComponentInChildren<CharacterController>();
        _inputActions = new InputSystem_Actions();
        _currentStamina = _maxStamina;

        // Map the Input System callbacks
        _inputActions.Player.Sprint.started += ctx => OnSprintStart();
        _inputActions.Player.Sprint.canceled += ctx => OnSprintCancel();
        _inputActions.Player.Interact.performed += ctx => OnInteract();

        _inputActions.Player.LightToggle.performed += ctx => lantern.ToggleLantern();
    }

    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

    private void Update()
    {
        _currentMoveInput = _inputActions.Player.Move.ReadValue<float>();
        
        HandleStamina();
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_currentState == PlayerState.Hiding)
        {
            return; // Lock movement if hiding
        }

        float speed = (_currentState == PlayerState.Sprinting) ? _sprintSpeed : _walkSpeed;
        
        // 2.5D Horizontal Movement (X axis)
        Vector3 move = new Vector3(_currentMoveInput, 0f, 0f);
        _controller.Move(move * speed * Time.deltaTime);

        // Flip Logic
        // Rotate the controller's transform based on input direction
        if (_currentMoveInput > 0.1f)
        {
            _controller.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Face Right
        }
        else if (_currentMoveInput < -0.1f)
        {
            _controller.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Face Left
        }

        // Apply simple gravity
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleStamina()
    {
        if (_currentState == PlayerState.Sprinting)
        {
            // Only drain if actively pressing movement keys
            if (Mathf.Abs(_currentMoveInput) > 0.1f) 
            {
                _currentStamina -= _staminaDrainRate * Time.deltaTime;
                if (_currentStamina <= 0)
                {
                    _currentStamina = 0;
                    _currentState = PlayerState.Exploration; // Exhausted, force walk
                }
            }
        }
        else
        {
            // Regenerate based on current state
            float regenRate = (_currentState == PlayerState.Hiding) ? _staminaRegenRate * _hidingRegenMultiplier : _staminaRegenRate;
            _currentStamina += regenRate * Time.deltaTime;
        }
        
        _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
    }

    private void OnSprintStart()
    {
        if (_currentState == PlayerState.Exploration && _currentStamina > 0)
        {
            _currentState = PlayerState.Sprinting;
        }
    }

    private void OnSprintCancel()
    {
        if (_currentState == PlayerState.Sprinting)
        {
            _currentState = PlayerState.Exploration;
        }
    }

    private void OnInteract()
    {
        Debug.Log("Interact Pressed - Firing door/hiding logic!");
    }
}