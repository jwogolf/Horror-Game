using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;         // The prefab we created for slots
    public Transform slotParent;          // Parent with GridLayoutGroup
    public GameObject uiPanel;            // UI panel to toggle on/off

    private List<GameObject> slotInstances = new List<GameObject>();
    [SerializeField] private InventorySystem inventory;    // Reference to the inventory system
    private bool isVisible = false;
    private int selectedIndex = 0;

    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false); // Start hidden
    }

    public void setIndex(int value)
    {
        selectedIndex += value;

        if (selectedIndex < 0) {
            selectedIndex = inventory.items.Count - 1;
        }
        if (selectedIndex > inventory.items.Count - 1) {
            selectedIndex = 0;
        }
        // selectedIndex = Mathf.Clamp(selectedIndex, 0, inventory.items.Count - 1);

        UpdateUI();
    }

    public void ToggleUI()
    {
        isVisible = !isVisible;
        if (uiPanel != null)
        {
            uiPanel.SetActive(isVisible);
            if (isVisible)
                selectedIndex = 0;
                UpdateUI();
        }
    }

    public bool IsOpen() => isVisible;

    public void UpdateUI()
    {
        // Reuse slots instead of destroying/instantiating every time
        while (slotInstances.Count < inventory.items.Count)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotParent);
            slotInstances.Add(newSlot);
        }

        for (int i = 0; i < slotInstances.Count; i++)
        {
            if (i < inventory.items.Count)
            {
                var slot = inventory.items[i];
                var slotData = slot.data;
                GameObject slotObj = slotInstances[i];

                slotObj.SetActive(true);

                Image icon = slotObj.transform.Find("Icon").GetComponent<Image>();
                TMP_Text quantityText = slotObj.transform.Find("Quantity").GetComponent<TMP_Text>();
                TMP_Text nameText = slotObj.transform.Find("Name").GetComponent<TMP_Text>();

                icon.sprite = slotData.icon;
                nameText.text = slotData.itemName;
                quantityText.text = (slotData.isStackable && slot.quantity > 1) ? "x " + slot.quantity.ToString() : "";

                if (i == selectedIndex)
                {
                    nameText.fontStyle = FontStyles.Underline; // Or use color, outline, etc.
                }
                else
                {
                    nameText.fontStyle = FontStyles.Normal;
                }
            }
            else
            {
                slotInstances[i].SetActive(false);
            }
        }
    }
}
