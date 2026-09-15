using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Room Transition")]
    [SerializeField] private Transform _targetSpawnPoint;

    // TODO: Add Cinemachine bounding box reference here for LevelManager

    // --------------------------------------------------------

    public void Interact()
    {
        if (_targetSpawnPoint != null)
        {
            Debug.Log($"Transitioning player to {_targetSpawnPoint.gameObject.name}");
            // TODO: Call LevelManager fade-to-black and teleport logic here
        }
        else
        {
            Debug.LogWarning("Door has not target spawn point set!");
        }
    }
}
