using UnityEngine;
using System.Collections.Generic;

public class Shotgun : MonoBehaviour, ToolBehavior
{
    [SerializeField] private LayerMask hitMask; // can probably remove unless only want to hit certain layers
    public enum AmmoType { Birdshot, Buckshot, Slug }
    private AmmoType ammoType = AmmoType.Buckshot;

    // Damage & spread
    private int pelletsPerShot = 8;
    private float spreadAngle = 1f;
    private float damagePerPellet = 8f;
    private float range = 500f;
    private float birdShotAngleMult = 2f;

    private Dictionary<AmmoType, float> damageMult = new Dictionary<AmmoType, float>
    {
        { AmmoType.Birdshot, 1f },
        { AmmoType.Buckshot, 2.5f },
        { AmmoType.Slug, 25f }
    };

    // Aiming
    private float defaultFOV;
    private float zoomFOV = 40f;
    private bool isAiming = false;
    private int activeChamber = 0;

    // Reloading
    private bool isReloading = false;
    private float lastReload = 0f;
    private float reloadTimePerRound = 1f;
    private bool isDoubleReload = false;

    // Double reload
    private float doubleReloadTimeIncrease = 0.5f;
    private float doubleTapWindow = 0.25f; // seconds
    private float lastTapTime = -1f;
    private bool awaitingSecondTap = false;


    private ItemInstance instance;
    private Camera playerCamera;
    private InventorySystem inventory;


    void OnDisable()
    {
        awaitingSecondTap = false;
    }

    public void SetInstance(ItemInstance itemInstance)
    {
        instance = itemInstance;
        int ammo = 0;
        if (instance.loadedAmmoTypes[0] != null) ammo++;
        if (instance.loadedAmmoTypes[1] != null) ammo++;
        instance.currentDurability = ammo;
    }

    public ItemInstance GetInstance()
    {
        return instance;
    }

    public void SetInventory(InventorySystem inv)
    {
        inventory = inv;
    }

    public void SetAmmoType(int type)
    {
        ammoType = (AmmoType)Mathf.Clamp(type, 0, System.Enum.GetValues(typeof(AmmoType)).Length - 1);
    }

    private string GetAmmoName(AmmoType type)
    {
        return type switch
        {
            AmmoType.Birdshot => "Birdshot",
            AmmoType.Buckshot => "Buckshot",
            AmmoType.Slug => "Shotgun Slug",
            _ => null
        };
    }

    private AmmoType ParseAmmoType(string s)
    {
        return s switch
        {
            "Birdshot" => AmmoType.Birdshot,
            "Buckshot" => AmmoType.Buckshot,
            "Shotgun Slug" => AmmoType.Slug,
            _ => AmmoType.Buckshot
        };
    }

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
            defaultFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        HandleReloading();

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
            if (instance.loadedAmmoTypes[0] != null && instance.loadedAmmoTypes[0] != "") activeChamber = 0;
            else activeChamber = 1;

            Fire(ParseAmmoType(instance.loadedAmmoTypes[activeChamber]));
            instance.loadedAmmoTypes[activeChamber] = null;
            instance.Use(1f);
            Debug.Log("Ammo in gun " + instance.loadedAmmoTypes[0] + " ," + instance.loadedAmmoTypes[1]);

            if (instance.loadedAmmoTypes[1] == null) activeChamber = 0;
            else activeChamber = 1;
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
        if (isReloading) {
            isReloading = false;
            Debug.Log("Reload Cancelled");
        }
    }

    public void StopAiming()
    {
        isAiming = false;
    }

    public void thirdAction()
    {
        float timeSinceLastTap = Time.time - lastTapTime;

        if (inventory.hasAbility("Double Reload") && awaitingSecondTap && timeSinceLastTap <= doubleTapWindow)
        {
            // Double tap detected
            isDoubleReload = true;
            lastReload = Time.time;
            isReloading = true;
            awaitingSecondTap = false;
        }
        else
        {
            // First tap — start timer
            awaitingSecondTap = true;
            lastTapTime = Time.time;

            // Start coroutine to fallback to single reload if second tap doesn't come in time
            StartCoroutine(FinalizeSingleReloadAfterDelay());
        }
    }

    public void fourthAction()
    {
        ammoType = (AmmoType)(((int)ammoType + 1) % System.Enum.GetValues(typeof(AmmoType)).Length);
        Debug.Log("Switched to: " + ammoType);
    }

    private void Fire(AmmoType type)
    {
        int projectiles = type == AmmoType.Slug ? 1 : pelletsPerShot;
        float spread = type == AmmoType.Birdshot ? spreadAngle * birdShotAngleMult : spreadAngle;

        for (int i = 0; i < projectiles; i++)
        {
            Vector3 direction = playerCamera.transform.forward;

            if (type != AmmoType.Slug)
            {
                direction = Quaternion.Euler(
                    Random.Range(-spread, spread),
                    Random.Range(-spread, spread),
                    0
                ) * direction;
            }

            if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, range, hitMask))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damagePerPellet * damageMult[type]);
                }

                // TODO: impact effect, decal, sound, etc.
            }
        }

        // TODO: play muzzle flash, recoil, etc.
    }

    private void HandleReloading()
    {
        if (!isReloading) return;

        float waitTime = reloadTimePerRound + (isDoubleReload ? doubleReloadTimeIncrease : 0f);

        if (Time.time - lastReload >= waitTime)
        {
            string ammoName = GetAmmoName(ammoType);
            int available = inventory.GetQuantityByName(ammoName);
            int shellsToLoad = isDoubleReload ? 2 : 1;

            if (available >= shellsToLoad)
            {
                ReloadShells(shellsToLoad);
                inventory.ReduceQuantityByName(ammoName, shellsToLoad);
            }
            else if (available > 0)
            {
                ReloadShells(1);
                inventory.ReduceQuantityByName(ammoName, 1);
            }
            else
            {
                Debug.Log("Out of ammo");
            }

            isReloading = false;
            isDoubleReload = false;
        }
    }


    private void ReloadShells(float shells)
    {
        int barrelAdjust = 0;
        if (instance.GetCurrentDurability() > 0) barrelAdjust = 1; // if not out of ammo load ammo to empty barrel
        for (int i = 0; i < shells; i++){
            instance.loadedAmmoTypes[(activeChamber + i + barrelAdjust) % 2] = GetAmmoName(ammoType);
        }

        instance.Repair(shells);

        Debug.Log("Ammo in Gun: " + instance.GetCurrentDurability());
        Debug.Log("Ammo in Gun " + instance.loadedAmmoTypes[0] + " ," + instance.loadedAmmoTypes[1]);
    }

    private System.Collections.IEnumerator FinalizeSingleReloadAfterDelay()
    {
        yield return new WaitForSeconds(doubleTapWindow);

        if (awaitingSecondTap)
        {
            isDoubleReload = false;
            lastReload = Time.time;
            isReloading = true;
            awaitingSecondTap = false;
        }
    }


    // ADD FUNCTION TO UNLOAD BOTH ALL SHELLS IN CHAMBERS AND RETURN TO INVENTORY
    // HAVENT DONE YET BECAUSE LIKELY NEEDS AN ADDITIONAL BUTTON MAPPING


}
