using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class SearchableObject : MonoBehaviour, IInteractable
{
    [Header("Search Settings")]
    [Tooltip("Leave this blank if the object contains nothing.")]
    [SerializeField] private CollectableSO _lootItem; 
    [SerializeField] private Sprite _searchPromptIcon; 
    
    private bool _hasBeenSearched = false;

    public void Interact()
    {
        // Plays FMOD Sound
        EventInstance ItemGet = RuntimeManager.CreateInstance("event:/ItemGet");
        RuntimeManager.AttachInstanceToGameObject(ItemGet, gameObject, GetComponent<Rigidbody>());
        ItemGet.start();
        ItemGet.release();

        if (_hasBeenSearched) return;

        if (_lootItem != null)
        {
            // Auto-loot the item
            InventoryManager.Instance.AddItem(_lootItem);
            
            // Send success message to HUD
            UIManager.Instance.ShowFeedback($"Found: {_lootItem.ItemName}.");
        }
        else
        {
            UIManager.Instance.ShowFeedback("Nothing but dust in here...");
        }


        // Lock the object from being searched again
        _hasBeenSearched = true;
        
        // Force the UI to hide the prompt immediately so it doesn't get stuck on screen
        UIManager.Instance.HidePrompt(); 
    }

    public Sprite GetPromptIcon()
    {
        // Only display the interact prompt if it hasn't been looted yet
        return _hasBeenSearched ? null : _searchPromptIcon;
    }
}