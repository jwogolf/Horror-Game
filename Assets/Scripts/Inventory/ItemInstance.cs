using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

[System.Serializable]
public class ItemInstance
{
    public ItemData data;
    public float currentDurability;
    public int quantity;
    public string[] loadedAmmoTypes = { null, null };

    public ItemInstance(ItemData data)
    {
        this.data = data;
        this.quantity = data.pickUpQuantity;

        currentDurability = data.maxDurability >= 0f ? data.maxDurability : -1f;
    }

    // Overriden constructor that takes a string
    public ItemInstance(string itemName)
    {
        ItemData foundData = Resources.LoadAll<ItemData>("").FirstOrDefault(item => item.itemName == itemName);
        if (foundData == null)
        {
            Debug.LogError($"ItemData with name '{itemName}' not found in Resources!");
            return;
        }

        this.data = foundData;
        this.quantity = data.pickUpQuantity;
        this.currentDurability = data.maxDurability >= 0f ? data.maxDurability : -1f;
    }

    public void Use(float amount)
    {
        if (data.maxDurability < 0f) return;
        currentDurability -= amount;
        currentDurability = Mathf.Clamp(currentDurability, 0f, data.maxDurability);
    }

    public void Repair(float amount)
    {
        if (data.maxDurability < 0f) return;
        currentDurability += amount;
        currentDurability = Mathf.Clamp(currentDurability, 0f, data.maxDurability);
    }

    public bool IsBroken()
    {
        return data.maxDurability >= 0f && currentDurability <= 0f;
    }

    public float GetCurrentDurability()
    {
        return currentDurability;
    }

    public float GetMaxDurability()
    {
        return data.maxDurability;
    }

    public int GetQuantity()
    {
        return quantity;
    }

    public void UseQuantity(int used)
    {
        quantity -= used;
    }
} 