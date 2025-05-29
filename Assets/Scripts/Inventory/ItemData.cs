using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject prefab;
    public Sprite icon;
    public string description;
    public int pickUpQuantity;
    public bool isStackable;
    public bool isHandheld;
    public bool isPermanent;
    public float maxDurability; // -1 = not degradable
    public float usageRate;

    // Clone method
    public ItemData Clone()
    {
        ItemData clone = ScriptableObject.CreateInstance<ItemData>();
        clone.itemName = this.itemName;
        clone.prefab = this.prefab;
        clone.icon = this.icon;
        clone.isStackable = this.isStackable;
        clone.isHandheld = this.isHandheld;
        clone.pickUpQuantity = this.pickUpQuantity;
        clone.isPermanent = this.isPermanent;
        clone.usageRate = this.usageRate;
        return clone;
    }
}
