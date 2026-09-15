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

    // ----------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

        // Spawn new items that match the category
        foreach (CollectableSO item in InventoryManager.Instance.GetCollectedItems())
        {
            if (item.Category == selectedCategory)
            {
                GameObject newSlot = Instantiate(_slotPrefab, _contentContainer);
                newSlot.GetComponent<InventorySlotUI>().Initialise(item);
            }
        }
    }

    public void UpdateDetailedView(CollectableSO item)
    {
        _detailedImage.sprite = item.DetailedImage;
        _detailedImage.gameObject.SetActive(true);
        _detailedLoreText.text = item.LoreText;
    }
}
