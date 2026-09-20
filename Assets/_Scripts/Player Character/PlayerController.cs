using System.Collections;
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

    private bool _isFacingRight = false;
    private bool _isTurning = false;
    private float _idleTimer = 0f;
    
    private Animator _animator;
    private CharacterController _controller;
    private InputSystem_Actions _inputActions; 
    private IInteractable _currentInteractable;
    private float _currentMoveInput;
    private Vector3 _velocity;

    // ---------------------------------------------------------------

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _inputActions = new InputSystem_Actions();
        _currentStamina = _maxStamina;

        // Map the Input System callbacks
        _inputActions.Player.Sprint.started += ctx => OnSprintStart();
        _inputActions.Player.Sprint.canceled += ctx => OnSprintCancel();
        _inputActions.Player.Interact.performed += ctx => OnInteract();
        _inputActions.Player.ToggleInventory.performed += ctx => UIManager.Instance.ToggleInventory();
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
        // Block movement input if hiding or is turning
        if (_currentState == PlayerState.Hiding || _isTurning)
        {
            return; 
        }

        // Check for a direction change
        if (_currentMoveInput > 0.1f && !_isFacingRight)
        {
            StartCoroutine(TurnAroundRoutine(true));
            return;
        }
        else if (_currentMoveInput < -0.1f && _isFacingRight)
        {
            StartCoroutine(TurnAroundRoutine(false));
            return;
        }

        // Standard movement
        float speed = (_currentState == PlayerState.Sprinting) ? _sprintSpeed : _walkSpeed;

        // Update the Animator Blend Tree
        float currentAnimSpeed = Mathf.Abs(_currentMoveInput) * speed;
        if (_animator != null)
        {
            _animator.SetFloat("Speed", currentAnimSpeed);

            // Idle timer logic
            if (currentAnimSpeed < 0.1 && _animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= 10f)
                {
                    _animator.SetTrigger("PlayAltIdle");
                    _idleTimer = 0f;
                }
            }
            else if (currentAnimSpeed >= 0.1)
            {
                _idleTimer = 0f;
            }
        }
        
        // 2.5D Horizontal Movement (X axis)
        Vector3 move = new Vector3(_currentMoveInput, 0f, 0f);
        _controller.Move(move * speed * Time.deltaTime);

        // Flip Logic
        float targetAngle = _controller.transform.eulerAngles.y;
        
        if (_currentMoveInput > 0.1f)
        {
            targetAngle = 90f;
        }
        else if (_currentMoveInput < -0.1f)
        {
            targetAngle = 270f;
        }

        // Smoothly rotate the character at 800 degrees per second (takes ~0.2s to turn)
        float smoothAngle = Mathf.MoveTowardsAngle(_controller.transform.eulerAngles.y, targetAngle, 800f * Time.deltaTime);
        _controller.transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

        // Apply simple gravity
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private IEnumerator TurnAroundRoutine(bool turningRight)
    {
        _isTurning = true;
        _isFacingRight = turningRight;

        // Force speed to 0
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
        }

        // Determine which animation state we are targeting
        string targetStateName = (_currentState == PlayerState.Sprinting) ? "Running Turn 180" : "Walking Turn 180";

        // Trigger the correct animation based on state
        if (_currentState == PlayerState.Sprinting)
        {
            if (_animator != null)
            {
                _animator.SetTrigger("TurnSprint"); 
            }
        }
        else
        {
            if (_animator != null)
            {
                _animator.SetTrigger("TurnWalk");
            }
        }

        // Wait until the Animator actually enters the Turn state
        while (_animator != null && !_animator.GetCurrentAnimatorStateInfo(0).IsName(targetStateName))
        {
            yield return null;
        }

        // Wait until the Animator finishes the turn and begins transitioning back to Movement
        while (_animator != null && _animator.GetCurrentAnimatorStateInfo(0).IsName(targetStateName))
        {
            // The exact frame Unity starts exiting the Turn state, break the loop
            if (_animator.IsInTransition(0)) 
            {
                break;
            }
            yield return null;
        }

        // Snap the physical GameObject to face the new direction in profile
        float newAngle = _isFacingRight ? 90f : -90f;
        _controller.transform.rotation = Quaternion.Euler(0f, newAngle, 0f);

        // Unlock controls
        _isTurning = false;
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
        if (_currentState == PlayerState.Hiding)
        {
            return;
            // TODO: Exit hiding with E
        }

        if (_currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
        else
        {
            Debug.Log("Nothing to interact with.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            _currentInteractable = interactable;
            
            // Show the prompt
            UIManager.Instance.ShowPrompt(interactable.GetPromptIcon());
        }      
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<IInteractable>() != null)
        {
            _currentInteractable = null;
            
            // Hide the prompt
            UIManager.Instance.HidePrompt();
        }
    }
}