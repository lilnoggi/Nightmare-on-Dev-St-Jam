using UnityEngine;

public class TeddyBearPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite _promptIcon;
    [SerializeField] private CollectableSO _teddyBearItem;
    [SerializeField] private GameObject _handMonster; 
    
    private bool _hasBeenCollected = false;

    public void Interact()
    {
        if (_hasBeenCollected) return;
        _hasBeenCollected = true;

        // Add the bear to the inventory dictionary
        InventoryManager.Instance.AddItem(_teddyBearItem);

        // Display an eerie text prompt on the HUD
        UIManager.Instance.ShowFeedback("The air turns freezing cold... something is awake.");

        // Awaken the hand monster to start the chase sequence
        if (_handMonster != null)
        {
            _handMonster.SetActive(true);
        }

        // Remove the bear from the room so it can't be picked up twice
        Destroy(gameObject);
    }

    public Sprite GetPromptIcon()
    {
        return _promptIcon;
    }
}