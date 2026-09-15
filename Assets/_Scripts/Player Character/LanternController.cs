using UnityEngine;

public class LanternController : MonoBehaviour
{
    [Header("Light Components")]
    [SerializeField] private Light _pointLight;

    [Header("Oil Settings")]
    [SerializeField] private float _maxOil = 100f;
    [SerializeField] private float _currentOil;
    [SerializeField] private float _depletionRate = 5f;
    [SerializeField] private float _maxRadius = 15f;

    private bool _isLanternOn = true;

    // ---------------------------------------------------

    private void Awake()
    {
        _currentOil = _maxOil;
        if (_pointLight == null)
        {
            _pointLight = GetComponentInChildren<Light>();
        }
    }

    private void Update()
    {
        if (_isLanternOn && _currentOil > 0)
        {
            // Drain over time
            _currentOil -= _depletionRate * Time.deltaTime;

            // Shrink the vision radius based on oil percentage
            float oilPercentage = _currentOil / _maxOil;
            _pointLight.range = _maxRadius * oilPercentage;

            if (_currentOil <= 0)
            {
                _currentOil = 0;
                TurnOffLantern();
                // TODO: Trigger loss condition here
                Debug.Log("Out of oil!");
            }
        }
    }

    public void ToggleLantern()
    {
        if (_currentOil > 0)
        {
            _isLanternOn = !_isLanternOn;
            _pointLight.enabled = _isLanternOn;
        }
    }

    private void TurnOffLantern()
    {
        _isLanternOn = false;
        _pointLight.enabled = false;
    }

    // Callced when the player gets an oil flask
    public void AddOil(float amount)
    {
        _currentOil = Mathf.Clamp(_currentOil + amount, 0, _maxOil);
        if (_currentOil > 0 && !_isLanternOn)
        {
            ToggleLantern();
        }
    }
}
