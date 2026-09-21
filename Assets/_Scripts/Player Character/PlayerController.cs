using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Exploration, Sprinting, Hiding, Tripping }
    [SerializeField] private PlayerState _currentState = PlayerState.Exploration;

    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _sprintSpeed = 6f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Stamina System")]
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _currentStamina;
    [SerializeField] private float _staminaDrainRate = 12.5f;
    [SerializeField] private float _staminaRegenRate = 15f;
    [SerializeField] private float _hidingRegenMultiplier = 2.5f; 

    [Header("References")]
    [SerializeField] private LanternController lantern;

    [Header("Post Processing")]
    [SerializeField] private Volume _globalVolume;
    private Vignette _vignette;

    private bool _isFacingRight = false;
    private bool _isTurning = false;
    private float _idleTimer = 0f;
    
    private Animator _animator;
    private CharacterController _controller;
    private InputSystem_Actions _inputActions; 
    private IInteractable _currentInteractable;
    private float _currentMoveInput;
    private Vector3 _velocity;
    private Vector3 _positionBeforeHiding;

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

        if (_globalVolume != null)
        {
            _globalVolume.profile.TryGet(out _vignette);
        }
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
        if (_currentState == PlayerState.Hiding || _currentState == PlayerState.Tripping || _isTurning)
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
                    StartCoroutine(TripSequenceRoutine());
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

        // Dynamically adjust vignette intensity based on stamina
        if (_vignette != null)
        {
            float staminaPercent = _currentStamina / _maxStamina;
            
            // Full stamina = 0.489 (baseline), Empty stamina = 0.8 (heavy tunnel vision)
            _vignette.intensity.value = Mathf.Lerp(0.8f, 0.489f, staminaPercent);
        }
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

    private IEnumerator TripSequenceRoutine()
    {
        _currentState = PlayerState.Tripping;

        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
            _animator.SetTrigger("Trip");
        }

        // Determine slide direction based on facing direction (Right = +1, Left = -1)
        float slideDirection = _isFacingRight ? 1f : -1f;
        float slideDistance = 3.5f; // Adjust this to slide further or shorter
        float tripDuration = 0.5f;  // How long the forward lunge lasts
        float elapsedTime = 0f;

        // Drive the parent player object forward via the CharacterController
        while (elapsedTime < tripDuration)
        {
            float moveStep = (slideDistance / tripDuration) * Time.deltaTime;
            _controller.Move(new Vector3(slideDirection * moveStep, 0f, 0f));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Keep controls locked until the animation plays through standing up and returns to Movement
        while (_animator != null && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
        {
            yield return null;
        }

        // Give control back to the player
        _currentState = PlayerState.Exploration;
    }

    private void OnInteract()
    {
        if (_currentState == PlayerState.Hiding)
        {
            // Exit hiding sequence
            if (_animator != null) 
            {
                _animator.SetBool("IsHiding", false);
                
                // Restore animation layer weights to 1 so the masks take over again
                int lanternLayer = _animator.GetLayerIndex("Lantern Layer");
                int postureLayer = _animator.GetLayerIndex("Posture Layer");
                if (lanternLayer != -1) _animator.SetLayerWeight(lanternLayer, 1f);
                if (postureLayer != -1) _animator.SetLayerWeight(postureLayer, 1f);
            }
            
            // Turn the physical lantern back on
            if (lantern != null) lantern.gameObject.SetActive(true);
            
            // Snap back to the 2.5D walking plane
            transform.position = _positionBeforeHiding;
            
            // Face the original left/right direction
            float resetAngle = _isFacingRight ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0f, resetAngle, 0f);
            
            _controller.enabled = true; // Re-enable physics collisions
            _currentState = PlayerState.Exploration;
            return;
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

    public void StartHiding(HideableSpot spot)
    {
        if (_currentState != PlayerState.Hiding)
        {
            StartCoroutine(HideSequenceRoutine(spot));
        }
    }

    private IEnumerator HideSequenceRoutine(HideableSpot spot)
    {
        _currentState = PlayerState.Hiding;
        _isTurning = true; // Lock standard movement

        // Turn off the physical lantern
        if (lantern != null) lantern.gameObject.SetActive(false);

        // Disable override layers so the Terrified animation can control the whole body
        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
            
            int lanternLayer = _animator.GetLayerIndex("Lantern Layer");
            int postureLayer = _animator.GetLayerIndex("Posture Layer");
            if (lanternLayer != -1) _animator.SetLayerWeight(lanternLayer, 0f);
            if (postureLayer != -1) _animator.SetLayerWeight(postureLayer, 0f);
        }

        _positionBeforeHiding = transform.position;
        _controller.enabled = false; // MUST disable to manually move the player on the Z-axis

        // Snap rotation to face the background (0 degrees)
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        // Play the opening animation
        if (_animator != null)
        {
            _animator.SetTrigger("OpenWardrobe");
        }

        // Wait exactly 1 frame for the Animator to start
        yield return null;

        // Wait for the open animation to finish
        while (_animator != null && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            yield return null;
        }

        // Snap into the wardrobe and turn around to face the camera (180 degrees)
        transform.position = spot.GetHidePoint().position;
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // Trigger the continuous terrified loop
        if (_animator != null) _animator.SetBool("IsHiding", true);
        
        _isTurning = false;
    }
}