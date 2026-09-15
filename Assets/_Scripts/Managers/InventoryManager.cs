using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Master List of everything the player has collected
    [SerializeField] private List<CollectibleSO> _collectedItems = new List<CollectibleSO>();

    // ----------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddItem(CollectibleSO item)
    {
        if (!_collectedItems.Contains(item))
        {
            _collectedItems.Add(item);
            Debug.Log($"Added {item.ItemName} to inventory!");
            // TODO: UIManager refresh active tab
        }
    }
}
