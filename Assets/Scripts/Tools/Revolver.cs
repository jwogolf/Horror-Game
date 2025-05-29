using UnityEngine;

public class Revolver : MonoBehaviour, ToolBehavior
{
    // Damage
    [SerializeField] private LayerMask hitMask; // can probably remove unless only want to hit certain layers
    private float fireRange = 500f;
    private float damage = 50f;

    // Reloading
    private float reloadTimePerRound = 1f;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    private ItemInstance instance;
    private InventorySystem inventory;

    // Aiming
    private Camera playerCamera;
    private float defaultFOV;
    private float zoomFOV = 40f;
    private bool isAiming = false;

    public void SetInstance(ItemInstance itemInstance)
    {
        instance = itemInstance;
    }

    public ItemInstance GetInstance()
    {
        return instance;
    }

    public void SetInventory(InventorySystem inv)
    {
        inventory = inv;
    }

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
            defaultFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        Reload();

        // Maintain FOV when aiming
        if (playerCamera != null)
        {
            float targetFOV = isAiming ? zoomFOV : defaultFOV;
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * 8f
            );
        }
    }

    public void mainAction()
    {
        if (instance == null)
            return;

        if (!instance.IsBroken())
        {
            Fire();
            instance.Use(1f);
            Debug.Log("Ammo in Gun: " + instance.GetCurrentDurability());
        }
        else
        {
            Debug.Log("Click! No ammo.");
        }
    }

    public void secondAction()
    {
        // Crosshair handled in ToolManager.cs
        isAiming = true;
        if (isReloading)
            isReloading = false; // Cancel reload if aiming
            Debug.Log("Reload Cancelled");
    }

    public void thirdAction()
    {
        if (!isReloading && instance.GetCurrentDurability() < instance.GetMaxDurability())
        {
            isReloading = true;
            reloadTimer = 0f;
        }
    }

    public void fourthAction() { }

    public void StopAiming()
    {
        isAiming = false;
    }

    private void Reload()
    {
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTimePerRound)
            {
                reloadTimer = 0f;

                string ammoName = "Revolver Ammo";
                if (inventory.GetQuantityByName(ammoName) > 0)
                {
                    ReloadOneBullet();
                    inventory.ReduceQuantityByName(ammoName, 1);
                    Debug.Log("Ammo left in Inventory: " + inventory.GetQuantityByName(ammoName));
                }
                else
                {
                    isReloading = false;
                    Debug.Log("Out of " + ammoName);
                }
            }
        }
    }

    private void ReloadOneBullet()
    {
        instance.Repair(1f);

        Debug.Log("Ammo in Gun: " + instance.GetCurrentDurability());

        if (instance.GetCurrentDurability() == instance.GetMaxDurability())
        {
            isReloading = false;
        }
    }

    private void Fire()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("No main camera assigned for revolver fire.");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, fireRange, hitMask))
        {
            Debug.Log($"Revolver hit {hit.collider.name}");

            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }

        // TODO: Trigger animation, muzzle flash, and sound
    }
}
