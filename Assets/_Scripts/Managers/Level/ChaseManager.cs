using System.Collections.Generic;
using UnityEngine;

public class ChaseManager : MonoBehaviour
{
    public static ChaseManager Instance { get; private set; }

    [Header("The Gauntlet")]
    [Tooltip("Drag every HazardZone object in the scene into this list.")]
    [SerializeField] private List<GameObject> _hazardZones = new List<GameObject>();

    [Header("Environment Shifts")]
    [Tooltip("Doors to lock or debris to spawn during the chase.")]
    [SerializeField] private List<GameObject> _blockedPaths = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure all hazards are off during the stealth descent phase
        foreach (GameObject hazard in _hazardZones)
        {
            if (hazard != null) hazard.SetActive(false);
        }
    }

    public void StartChaseSequence()
    {
        Debug.Log("THE CHASE HAS BEGUN!");

        // Activate every Hazard Zone tripwire
        foreach (GameObject hazard in _hazardZones)
        {
            if (hazard != null) hazard.SetActive(true);
        }

        // Block the easy paths so the player must navigate the long way
        foreach (GameObject blocker in _blockedPaths)
        {
            if (blocker != null) blocker.SetActive(true);
        }
    }
}