using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Interaction HUD")]
    [SerializeField] private Image _promptImage;

    [Header("Inventory HUD")]
    [SerializeField] private GameObject _inventoryCanvas;

    private bool _isInventoryOpen = false;

    // ---------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Hide prompt when game starts
        HidePrompt();

        _inventoryCanvas.gameObject.SetActive(false);
    }

    // --- PROMPT HELPERS ---

    public void ShowPrompt(Sprite promptSprite)
    {
        if (promptSprite != null)
        {
            _promptImage.sprite = promptSprite;
            _promptImage.gameObject.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        _promptImage.gameObject.SetActive(false);
    }

    // --- INVENTORY HELPERS ---
    
    public void ToggleInventory()
    {
        _isInventoryOpen = !_isInventoryOpen;
        _inventoryCanvas.SetActive(_isInventoryOpen);

        if (_isInventoryOpen)
        {
            // Pause the game and load the default tab
            Time.timeScale = 0f;
            InventoryUIManager.Instance.RefreshTab((int)ItemCategory.KeyItem);
        }    
        else
        {
            // Resume the game
            Time.timeScale = 1f;
        }
    }
}
