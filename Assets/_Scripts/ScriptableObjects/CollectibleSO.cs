using UnityEngine;

[CreateAssetMenu(fileName = " New Collectible", menuName = "Inventory/Collectible")]
public class CollectibleSO : ScriptableObject
{
    public string ItemName;
    public ItemCategory Category;
    public Sprite DetailedImage;
    [TextArea(3, 10)] public string LoreText;
}

public enum ItemCategory { KeyItem, Collectible, Resource }
