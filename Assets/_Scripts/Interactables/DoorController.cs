using System;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Room Transition")]
    [SerializeField] private Transform _targetSpawnPoint;
    [SerializeField] private Collider _targetRoomCollider;

    // --------------------------------------------------------

    public void Interact()
    {
        if (_targetSpawnPoint != null && _targetRoomCollider != null)
        {
            LevelManager.Instance.TransitionToRoom(_targetSpawnPoint, _targetRoomCollider);
        }
        else
        {
            Debug.LogWarning("Door is missing transition references!");
        }
    }
}
