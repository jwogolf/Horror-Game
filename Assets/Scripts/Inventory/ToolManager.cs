using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ToolManager : MonoBehaviour
{
    [SerializeField] GameSaveManager save;

    public Transform toolHoldPoint; // Where tools are held in hand

    [SerializeField] private InventorySystem inventory;
    [SerializeField] private GameObject crosshairDot;
    private InventoryUI inventoryUI;
    private List<ItemInstance> tools;
    private ItemInstance currentToolInstance;
    private GameObject currentToolObject;
    private int toolIndex;

    // Controls
    [SerializeField] private InputActionAsset inputActionsAsset;
    private InputAction toggleAction;
    private InputAction mainUseAction;
    private InputAction secondUseAction;
    private InputAction thirdUseAction;
    private InputAction fourthUseAction;
    private ToolBehavior currentToolBehavior;

    private System.Action<InputAction.CallbackContext> onMainUse;
    private System.Action<InputAction.CallbackContext> onSecondUseStart;
    private System.Action<InputAction.CallbackContext> onSecondUseCancel;
    private System.Action<InputAction.CallbackContext> onThirdUse;
    private System.Action<InputAction.CallbackContext> onFourthUse;
    private System.Action<InputAction.CallbackContext> onToggle;

    private List<string> partiallyUseableTools = new List<string> {
        "Flashlight Shotgun",
    };

    void OnEnable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player");

        toggleAction = playerMap.FindAction("Toggle");
        mainUseAction = playerMap.FindAction("Attack");
        secondUseAction = playerMap.FindAction("Aim");
        thirdUseAction = playerMap.FindAction("Reload");
        fourthUseAction = playerMap.FindAction("Right");

        onMainUse = ctx =>
        {
            //UseTool(currentToolInstance.data.usageRate);
            currentToolBehavior?.mainAction();
        };

        onSecondUseStart = ctx => currentToolBehavior?.secondAction();
        onSecondUseCancel = ctx =>
        {
            if (currentToolBehavior is Revolver revolver || currentToolBehavior is Shotgun shotgun)
                currentToolBehavior.StopAiming();
        };

        onThirdUse = ctx => currentToolBehavior?.thirdAction();

        onFourthUse = ctx =>
        {
            if (!inventoryUI.IsOpen())
                currentToolBehavior?.fourthAction();
        };

        onToggle = ctx => cycleTools(toggleAction.ReadValue<float>());

        mainUseAction.performed += onMainUse;
        secondUseAction.performed += onSecondUseStart;
        secondUseAction.canceled += onSecondUseCancel;
        thirdUseAction.started += onThirdUse;
        fourthUseAction.started += onFourthUse;
        toggleAction.started += onToggle;

        playerMap.Enable();
    }

    void OnDisable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player");

        mainUseAction.started -= onMainUse;
        secondUseAction.started -= onSecondUseStart;
        secondUseAction.canceled -= onSecondUseCancel;
        thirdUseAction.started -= onThirdUse;
        fourthUseAction.started -= onFourthUse;
        toggleAction.started -= onToggle;

        playerMap.Disable();
    }

    void Start()
    {
        inventoryUI = FindFirstObjectByType<InventoryUI>();
        tools = inventory.GetHandheldItems();
        toolIndex = 0;
        currentToolInstance = tools[toolIndex];
        Equip(currentToolInstance);
    }

    void Update()
    {
        // Smooth zoom effects or other continuous input effects could go here if needed.
    }

    public void cycleTools(float value)
    {
        tools = inventory.GetHandheldItems();
        bool next = value > 0.01f;

        Unequip();

        if (next)
        {
            toolIndex = (toolIndex + 1) % tools.Count;
        }
        else
        {
            toolIndex = toolIndex - 1;
            if (toolIndex < 0)
            {
                toolIndex = tools.Count - 1;
            }
        }

        currentToolInstance = tools[toolIndex];
        Equip(currentToolInstance);
        Debug.Log(currentToolInstance.data.itemName);
        save.LoadGame(); // CHANGE THIS TO TEST SAVE/LOAD
    }

    public void Equip(ItemInstance instance)
    {
        Unequip();

        currentToolInstance = instance;

        if (instance.data.prefab != null)
        {
            currentToolObject = Instantiate(instance.data.prefab, toolHoldPoint);
            currentToolBehavior = currentToolObject.GetComponent<ToolBehavior>();

            // Show crosshair for specific tools
            string name = instance.data.itemName;
            if (name == "Revolver" || name == "Shotgun" || name == "Knife")
                crosshairDot.SetActive(true);

            else
                crosshairDot.SetActive(false);
        }

        // Set instances when first opening a tool
        if (currentToolBehavior is Flashlight flashlightTool && flashlightTool.GetInstance() == null)
        {
            flashlightTool.SetInstance(currentToolInstance);
        }
        if (currentToolBehavior is Revolver revolverTool && revolverTool.GetInstance() == null)
        {
            revolverTool.SetInstance(currentToolInstance);
            revolverTool.SetInventory(inventory);
        }
        if (currentToolBehavior is Shotgun shotgunTool && shotgunTool.GetInstance() == null)
        {
            shotgunTool.SetInstance(currentToolInstance);
            shotgunTool.SetInventory(inventory);
        }
    }

    public void Unequip()
    {
        if (currentToolObject != null)
        {
            Destroy(currentToolObject);
            currentToolObject = null;
        }

        if (crosshairDot != null)
            crosshairDot.SetActive(false);

        currentToolInstance = null;
    }

    public void UseTool(float usage)
    {
        if (currentToolInstance == null || currentToolInstance.IsBroken()) return;
        currentToolInstance.Use(usage);
    }

    public ItemInstance GetCurrentTool()
    {
        return currentToolInstance;
    }
}
