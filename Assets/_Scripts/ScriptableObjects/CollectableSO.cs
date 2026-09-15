using UnityEngine;

[CreateAssetMenu(fileName = " New Collectable", menuName = "Inventory/Collectable")]
public class CollectableSO : ScriptableObject
{
    public string ItemName;
    public ItemCategory Category;
    public Sprite DetailedImage;
    [TextArea(3, 10)] public string LoreText;
}

public enum ItemCategory { KeyItem, Collectable, Resource }
