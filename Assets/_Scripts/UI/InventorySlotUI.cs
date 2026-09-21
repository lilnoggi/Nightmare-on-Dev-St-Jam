using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemCountText;

    private CollectableSO _itemData;

    public void Initialise(CollectableSO item, int count)
    {
        _itemData = item;
        _itemNameText.text = item.ItemName;

        // Show a multiplier if they have more than 1
        if (_itemCountText != null)
        {
            _itemCountText.text = count > 1 ? $"x{count}" : "";
        }

        GetComponent<Button>().onClick.AddListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        InventoryUIManager.Instance.UpdateDetailedView(_itemData);
    }
}