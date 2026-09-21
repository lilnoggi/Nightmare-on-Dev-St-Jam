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

        InventoryManager.Instance.AddItem(_teddyBearItem);
        UIManager.Instance.ShowFeedback("The air turns freezing cold... something is awake.");

        // Trigger the global sequence instead of a single hand
        if (ChaseManager.Instance != null)
        {
            ChaseManager.Instance.StartChaseSequence();
        }

        Destroy(gameObject);
    }

    public Sprite GetPromptIcon()
    {
        return _promptIcon;
    }
}