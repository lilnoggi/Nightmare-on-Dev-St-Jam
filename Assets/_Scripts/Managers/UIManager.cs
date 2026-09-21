using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Interaction HUD")]
    [SerializeField] private Image _promptImage;

    [Header("Feedback Panel")]
    [SerializeField] private GameObject _feedbackPanel;
    [SerializeField] private TextMeshProUGUI _feedbackDialogue;

    [Header("Inventory HUD")]
    [SerializeField] private GameObject _inventoryCanvas;

    private bool _isInventoryOpen = false;
    private Coroutine _feedbackCoroutine;

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

        if (_feedbackPanel != null && _feedbackDialogue != null)
        {
            _feedbackDialogue.text = "";
            _feedbackPanel.SetActive(false);
        }
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

    // --- FEEDBACK HELPERS ---

    public void ShowFeedback(string message)
    {
        // Stop the previous timer if the player searches multiple things quickly
        if (_feedbackCoroutine != null)
        {
            StopCoroutine(_feedbackCoroutine);
        }

        _feedbackCoroutine = StartCoroutine(FeedbackRoutine(message));
    }

    private IEnumerator FeedbackRoutine(string message)
    {
        _feedbackPanel.SetActive(true);
        _feedbackDialogue.text = message;
        yield return new WaitForSeconds(2.5f);
        _feedbackDialogue.text = "";
        _feedbackPanel.SetActive(false);
    }

    // --- INVENTORY HELPERS ---
    
    public void ToggleInventory()
    {
        // Prevent opening if game is already paused
        if (!_isInventoryOpen && Time.timeScale == 0f)
        {
            return;
        }
        
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
