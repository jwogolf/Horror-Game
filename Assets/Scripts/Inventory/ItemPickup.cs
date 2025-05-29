using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData; // Assign in the inspector

    public void Pickup(InventorySystem inventory)
    {
        if (itemData != null)
        {
            inventory.AddItem(itemData, itemData.pickUpQuantity);
            Destroy(gameObject); // Remove from world after pickup
        }
    }
}
