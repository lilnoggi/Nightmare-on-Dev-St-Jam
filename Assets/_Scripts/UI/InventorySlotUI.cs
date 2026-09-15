using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _itemNameText;

    private CollectableSO _itemData;

    // ----------------------------------------------

    public void Initialise(CollectableSO item)
    {
        _itemData = item;
        _itemNameText.text = item.ItemName;

        // Listen for player clicking this specific button
        GetComponent<Button>().onClick.AddListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        // Send this item's data on the detailed view
        InventoryUIManager.Instance.UpdateDetailedView(_itemData);
    }
}
