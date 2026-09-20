using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Maps the specific item data to the total quantity owned
    [SerializeField] private Dictionary<CollectableSO, int> _inventory = new Dictionary<CollectableSO, int>();

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

    public void AddItem(CollectableSO item, int amount = 1)
    {
        if (_inventory.ContainsKey(item))
        {
            _inventory[item] += amount;
        }
        else
        {
            _inventory.Add(item, amount);
        }
    }

    public void RemoveItem(CollectableSO item, int amount = 1)
    {
        if (_inventory.ContainsKey(item))
        {
            _inventory[item] -= amount;
            if (_inventory[item] <= 0)
            {
                _inventory.Remove(item); // Clears the slot if completely empty
            }
        }
    }

    public Dictionary<CollectableSO, int> GetInventory()
    {
        return _inventory;
    }
}
