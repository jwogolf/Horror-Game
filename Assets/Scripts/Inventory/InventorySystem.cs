using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;

public class InventorySystem : MonoBehaviour
{
    public InventoryUI ui;
    public CraftingManager craftingManager;

    public List<ItemInstance> items = new List<ItemInstance>();
    public int maxInventorySize = 10;

    public List<string> abilities = new List<string>();

    // Controls
    [SerializeField] private InputActionAsset inputActionsAsset;
    private InputAction upAction;
    private InputAction downAction;
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction selectAction;
    private InputAction exitAction;
    private InputAction openAction;
    //private bool holding = false; // holding button

    // permanent items
    private string[] permaItems = { "Knife", "Revolver", "Lighter", "Multi-Meter" };
    private int startingRevolverRounds = 24;

    void OnEnable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player");

        // May be able to remove left/right depending on final implementation, but maybe use them to flip between inventory and map
        upAction = playerMap.FindAction("Up"); // Up D-pad
        downAction = playerMap.FindAction("Down"); // Up D-pad
        leftAction = playerMap.FindAction("Left"); // Up D-pad
        rightAction = playerMap.FindAction("Right"); // Up D-pad
        selectAction = playerMap.FindAction("Pick Up"); // X button
        exitAction = playerMap.FindAction("Crouch"); // Circle button
        openAction = playerMap.FindAction("Inventory"); // options button

        playerMap.Enable();
    }

    void OnDisable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player");

        playerMap.Disable();
    }

    void Awake()
    {
        // Add permenant items
        foreach (string item in permaItems)
        {
            AddItemByString(item);
        }

        // add revolver ammo
        AddItemWithState("Revolver Ammo", startingRevolverRounds, 0f);
    }

    void Update()
    {
        // Handle opening inventory and navigation once inventory is open
            if (openAction.WasPressedThisFrame())
            {
                ui.ToggleUI();
            }

        if (ui.IsOpen())
        {
            if (upAction.WasPressedThisFrame())
            {
                ui.setIndex(-1);
            }
            if (downAction.WasPressedThisFrame())
            {
                ui.setIndex(1);
            }

            // ADD MORE CONTROLS TO INVENTORY AS NEEDED

        }
    }

    public void AddItem(ItemData itemData, int quantity = 1)
    {
        // Stackable check
        if (itemData.isStackable)
        {
            ItemInstance stack = items.FirstOrDefault(i => i.data == itemData);
            if (stack != null)
            {
                stack.quantity += quantity;
                ui.UpdateUI();
                Debug.Log($"Added {itemData.itemName} x{quantity} to inventory.");
                return;
            }
        }

        if (items.Count < maxInventorySize)
        {
            ItemInstance newItem = new ItemInstance(itemData);
            newItem.quantity = quantity;
            items.Add(newItem);
            Debug.Log($"Added {itemData.itemName} x{quantity} to inventory.");
        }
        else
        {
            Debug.Log("Inventory Full");
        }

        ui.UpdateUI();
    }

    public void AddItemWithState(string itemName, int quantity, float durability, string[] ammo = null)
    {
        ItemInstance newItem = new ItemInstance(itemName);
        newItem.quantity = quantity;
        newItem.currentDurability = durability;
        if (ammo != null) newItem.loadedAmmoTypes = ammo;
        items.Add(newItem);
        ui.UpdateUI();
    }

    public void AddItemByString(string itemName)
    {
        ItemInstance newItem = new ItemInstance(itemName);
        items.Add(newItem);
        ui.UpdateUI();
    }

    public void clearItems()
    {
        items.Clear();
        ui.UpdateUI();
    }


    // DETERMINE TRIGGER TO REMOVE ITEMS FROM INVENTORY
    // make sure the player cannot drop permanant items but they can be removed when crafting
    public void RemoveItem(ItemInstance item)
    {
        items.Remove(item);
        Debug.Log($"Removed {item.data.itemName} from inventory.");
        ui.UpdateUI();
    }

    // HAVENT REALLY TESTED ANYTHING TO DO WITH CRAFTING
    // Needs to be triggered somehow aswell
    public bool CombineItems(ItemInstance itemA, ItemInstance itemB)
    {
        ItemData resultData = craftingManager.TryCraft(itemA.data, itemB.data);
        if (resultData != null)
        {
            RemoveItem(itemA);
            RemoveItem(itemB);
            AddItem(resultData, resultData.pickUpQuantity);
            Debug.Log($"Crafted {resultData.itemName}!");
            return true;
        }

        ui.UpdateUI();
        Debug.Log("Combination failed.");
        return false;
    }

    public bool HasItem(ItemData itemData)
    {
        return items.Any(i => i.data == itemData);
    }

    public List<ItemInstance> GetHandheldItems()
    {
        return items.Where(i => i.data.isHandheld).ToList();
    }

    public List<ItemInstance> getAllItems()
    {
        return items;
    }

    public int GetQuantityByName(string name)
    {
        ItemInstance item = items.FirstOrDefault(i => i.data.itemName == name);
        return item != null ? item.quantity : 0;
    }

    public void ReduceQuantityByName(string name, int used)
    {
        ItemInstance item = items.FirstOrDefault(i => i.data.itemName == name);
        if (item != null)
        {
            item.quantity -= used;
            if (item.quantity <= 0)
            {
                items.Remove(item); // Optional: remove from inventory if depleted
            }
        }
    }

    public void addAbility(string ability)
    {
        abilities.Add(ability);
    }

    public bool hasAbility(string ability)
    {
        return abilities.Contains(ability);
    }

    public List<string> getAbilities()
    {
        return abilities;
    }
}
