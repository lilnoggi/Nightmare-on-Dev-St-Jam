using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("Spawning")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _contentContainer;

    [Header("Detailed View")]
    [SerializeField] private Image _detailedImage;
    [SerializeField] private TMP_Text _detailedLoreText;
    [SerializeField] private Button _useButton;

    private CollectableSO _currentlyViewedItem;

    // ----------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_useButton != null)
        {
            _useButton.onClick.AddListener(OnUseButtonClicked);
        }
    }

    // Called by cliking the tab buttons
    public void RefreshTab(int categoryIndex)
    {
        // Cast the int from the Unity Button back into our Enum
        ItemCategory selectedCategory = (ItemCategory)categoryIndex;

        // Destroy old items in the list
        foreach (Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Clear the detailed view
        _detailedImage.gameObject.SetActive(false);
        _detailedLoreText.text = "";
        if (_useButton != null)
        {
            _useButton.gameObject.SetActive(false);
        }

        // Spawn new items that match the category
        foreach (var kvp in InventoryManager.Instance.GetInventory())
        {
            CollectableSO item = kvp.Key;
            int count = kvp.Value;

            if (item.Category == selectedCategory)
            {
                GameObject newSlot = Instantiate(_slotPrefab, _contentContainer);
                newSlot.GetComponent<InventorySlotUI>().Initialise(item, count);
            }
        }
    }

    public void UpdateDetailedView(CollectableSO item)
    {
        _currentlyViewedItem = item;
        _detailedImage.sprite = item.DetailedImage;
        _detailedImage.gameObject.SetActive(true);
        _detailedLoreText.text = item.LoreText;

        // Only show the Use button if it is a consumable resource
        if (_useButton != null)
        {
            _useButton.gameObject.SetActive(item.Category == ItemCategory.Resource);
        }
    }

    private void OnUseButtonClicked()
    {
        if (_currentlyViewedItem != null && _currentlyViewedItem.ItemName == "Oil Flask")
        {
            // Send 50 oil to the lantern controller
            LanternController lantern = FindAnyObjectByType<LanternController>();
            if (lantern != null)
            {
                lantern.AddOil(100f); 
            }

            // Consume the item from the dictionary
            InventoryManager.Instance.RemoveItem(_currentlyViewedItem, 1);
            
            // Hard refresh the UI to update the number or clear the empty slot
            RefreshTab((int)_currentlyViewedItem.Category);
        }
    }
}
