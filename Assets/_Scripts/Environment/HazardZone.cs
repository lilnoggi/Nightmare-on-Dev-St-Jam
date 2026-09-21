using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HazardZone : MonoBehaviour
{
    [Header("Strike 1: Warning")]
    [SerializeField] private GameObject _dustParticles;

    [Header("Strike 2: Escalation")]
    [SerializeField] private GameObject _creepingShadow;

    [Header("Strike 3: Lethal")]
    [SerializeField] private GameObject _handMonster;
    [SerializeField] private Transform _handSpawnPoint;

    [Header("Timing")]
    [SerializeField] private float _timeBetweenStrikes = 2.0f;

    private bool _isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger once, and only if it's the player
        if (!_isActivated && other.CompareTag("Player"))
        {
            _isActivated = true;
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                StartCoroutine(HazardSequenceRoutine(player));
            }
        }
    }

    private IEnumerator HazardSequenceRoutine(PlayerController player)
    {
        // STRIKE 1: Dust and muffled thump
        if (_dustParticles != null) _dustParticles.SetActive(true);
        Debug.Log("Strike 1: Dust falls from the ceiling... (Play muffled thump audio)");
        yield return new WaitForSeconds(_timeBetweenStrikes);

        // STRIKE 2: Shadow and louder thump
        if (_creepingShadow != null) _creepingShadow.SetActive(true);
        Debug.Log("Strike 2: A shadow stretches across the wall... (Play heavy thump audio)");
        yield return new WaitForSeconds(_timeBetweenStrikes);

        // STRIKE 3: The Hand bursts through
        yield return new WaitForSeconds(_timeBetweenStrikes);

        // Check if the player successfully entered a hiding spot before the third strike
        if (player.IsHiding())
        {
            Debug.Log("Player survived! They hid in time.");
            // Spawn the hand to sweep the room safely, then despawn it
        }
        else
        {
            Debug.Log("Strike 3: Player was caught in the open!");
            if (_handMonster != null)
            {
                _handMonster.SetActive(true);
                if (_handSpawnPoint != null)
                {
                    _handMonster.transform.position = _handSpawnPoint.position;
                }
            }
            // TODO: Trigger Game Over sequence
        }
    }
}