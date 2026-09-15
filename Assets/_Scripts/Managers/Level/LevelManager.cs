using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Player & Camera")]
    [SerializeField] private CharacterController _playerController;
    [SerializeField] private CinemachineConfiner3D _cameraConfiner;

    [Header("Initial Spawn")]
    [SerializeField] private Transform _initialSpawnPoint;
    [SerializeField] private Collider _initialRoomCollider;

    [Header("UI Transition")]
    [SerializeField] private Image _fadeOverlay;
    [SerializeField] private float _fadeDuration = 1.5f;

    // -----------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Snap player to start spawn point when game boots up
        if (_initialSpawnPoint != null && _initialRoomCollider != null)
        {
            TeleportPlayer(_initialSpawnPoint, _initialRoomCollider);
            StartCoroutine(FadeFromBlack());
        }
    }

    public void TransitionToRoom(Transform targetSpawn, Collider targetRoomBounds)
    {
        StartCoroutine(TransitionRoutine(targetSpawn, targetRoomBounds));
    }

    private IEnumerator TransitionRoutine(Transform targetSpawn, Collider targetRoomBounds)
    {
        // Get the player controlle rscript
        PlayerController playerScript = _playerController.GetComponent<PlayerController>();
        
        // Disable player input and fade to black
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }
        
        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Teleport the player and update camera boundaries
        TeleportPlayer(targetSpawn, targetRoomBounds);

        // Fade back into the game
        yield return StartCoroutine(FadeFromBlack());

        // Restore input
        if (playerScript != null)
        {
            playerScript.enabled = true;
        }
    }

    private void TeleportPlayer(Transform targetSpawn, Collider targetRoomBounds)
    {
        // Disable controller to move it safely
        _playerController.enabled = false;

        _playerController.transform.position = targetSpawn.position;

        // Update the camera to stay inside the new room
        _cameraConfiner.BoundingVolume = targetRoomBounds;

        _playerController.enabled = true;
    }

    private IEnumerator FadeToBlack()
    {
        float timer = 0f;
        Color colour = _fadeOverlay.color;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            colour.a = Mathf.Lerp(0f, 1f, timer / _fadeDuration);
            _fadeOverlay.color = colour;
            yield return null;
        }
    }

    private IEnumerator FadeFromBlack()
    {
        float timer = 0f;
        Color colour = _fadeOverlay.color;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            colour.a = Mathf.Lerp(1f, 0f, timer / _fadeDuration);
            _fadeOverlay.color = colour;
            yield return null;
        }
    }
}
