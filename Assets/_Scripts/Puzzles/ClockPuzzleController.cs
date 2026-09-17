using System.Collections;
using UnityEngine;

public class ClockPuzzleController : MonoBehaviour, IInteractable
{
    [Header("Puzzle Setup")]
    [SerializeField] private GameObject _puzzleCanvas;
    [SerializeField] private GameObject _rewardScreenImage;
    [SerializeField] private Transform _minuteHand;
    [SerializeField] private Transform _hourHand;
    [SerializeField] private CollectableSO _requiredClockHandItem;
    [SerializeField] private CollectableSO _hiddenCompartmentItem;

    [Header("Mechanics")]
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _requiredHoldTime = 2f;

    // The target time is 03:15.
    // Minute hand: 15 mins = 90 degrees left
    // Hour hand: 3.25 hours * 30 degrees = 97.5 degrees left
    private float _targetMinuteAngle = 270f;
    private float _targetHourAngle = 262.5f;
    private float _marginOfError = 5f;

    // Track the angles manually
    private float _currentMinuteAngle = 0f;
    private float _currentHourAngle = 0f;

    private float _currentHoldTime = 0f;

    private bool _isPuzzleActive = false;
    private bool _isSolved = false;
    private bool _hasAttatchedHand = false;
    private InputSystem_Actions _inputActions;

    // ---------------------------------------------------------------

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();

        _inputActions.Player.Interact.performed += ctx => TryExitPuzzle();
        _minuteHand.gameObject.SetActive(false);
        _puzzleCanvas.SetActive(false);
        _rewardScreenImage.SetActive(false);
    }

    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

    public void Interact()
    {
        if (_isSolved)
        {
            return;
        }

        // Check if the player has the required item
        if (!_hasAttatchedHand)
        {
            if (InventoryManager.Instance.GetCollectedItems().Contains(_requiredClockHandItem))
            {
                _hasAttatchedHand = true;
                _minuteHand.gameObject.SetActive(true);

                // Remove the item from the mast inventory list
                InventoryManager.Instance.GetCollectedItems().Remove(_requiredClockHandItem);
            }
            else
            {
                return;
            }
        }

        // Lock the player into the puzzle state
        _isPuzzleActive = true;
        Time.timeScale = 0f;
        _puzzleCanvas.SetActive(true);
    }

    private void Update()
    {
        if (!_isPuzzleActive || _isSolved)
        {
            return;
        }

        // Map the player's horizontal input to the local Z-axis rotation
        float input = _inputActions.Player.Move.ReadValue<float>();
        if (Mathf.Abs(input) > 0.1f)
        {
            float rotationStep = -input * _rotationSpeed * Time.unscaledDeltaTime;

            // Update the manual floats
            _currentMinuteAngle += rotationStep;
            _currentHourAngle += (rotationStep / 12f);

            // Force the UI transforms to perfectly match the floats
            _minuteHand.localRotation = Quaternion.Euler(0, 0, _currentMinuteAngle);
            _hourHand.localRotation = Quaternion.Euler(0, 0, _currentHourAngle);
        }

        // Always check solution
        CheckSolution();
    }

    private void CheckSolution()
    {
        // Normalise angles to 0-360 for comparison
        float currentMinAngle = NormaliseAngle(_currentMinuteAngle);
        float currentHourAngle = NormaliseAngle(_currentHourAngle);

        // Check if both transforms fall within the margin of error
        if (Mathf.Abs(Mathf.DeltaAngle(currentMinAngle, _targetMinuteAngle)) <= _marginOfError &&
            Mathf.Abs(Mathf.DeltaAngle(currentHourAngle, _targetHourAngle)) <= _marginOfError)
        {
            // Increment the timer while the hands are in the correct spot
            _currentHoldTime += Time.unscaledDeltaTime;

            // Only solve if they have held it long enough
            if (_currentHoldTime >= _requiredHoldTime)
            {
                _isSolved = true;
                StartCoroutine(ShowRewardSequence());
            }
        }
        else
        {
            // Reset the timer if the player moves the hands away from the correct spot
            _currentHoldTime = 0f;
        }
    }

    private IEnumerator ShowRewardSequence()
    {
        // Hide the clock show drawer
        _puzzleCanvas.SetActive(false);
        _rewardScreenImage.SetActive(true);

        // Wait 3 seconds
        yield return new WaitForSecondsRealtime(3f);

        // Close everything and resume game
        _rewardScreenImage.SetActive(false);
        _isPuzzleActive = false;
        Time.timeScale = 1f;

        // Get the key
        InventoryManager.Instance.AddItem(_hiddenCompartmentItem);
    }

    private void TryExitPuzzle()
    {
        // Only allow exiting if the puzzle is active AND hasn't been solved yet
        if (_isPuzzleActive && !_isSolved)
        {
            _isPuzzleActive = false;
            Time.timeScale = 1f;
            _puzzleCanvas.SetActive(false);
        }
    }

    private float NormaliseAngle(float angle)
    {
        angle %= 360;
        if (angle < 0)
        {
            angle += 360;
        }
        return angle;
    }

    public Sprite GetPromptIcon()
    {
        return null; // TODO: Assign speicifc clock icon here
    }
}
