using UnityEngine;

public class Flashlight : MonoBehaviour, ToolBehavior
{
    [SerializeField] Light flashlight; // Assign in inspector, ligth source
    private float crankAmount = 2f; // seconds of light per crank
    private float drainRate = 1f; // durability lost per second when on
    private float fadeDuration = 3f; // how long the fade out lasts when flashlight is dying
    private float maxIntensity = 500f; // light intensity

    private ItemInstance instance;
    private bool isOn = false;

    public void SetInstance(ItemInstance itemInstance)
    {
        instance = itemInstance;
        // Only drain if it hasn't been cranked yet
        if (instance.GetCurrentDurability() == instance.GetMaxDurability())
        {
            instance.Use(99999); // Flashlight starts dead
        }
        float maxDurability = instance.GetMaxDurability();
        flashlight.intensity = maxIntensity;
    }

    public ItemInstance GetInstance()
    {
        return instance;
    }

    void Start()
    {
        if (flashlight != null)
            flashlight.enabled = false;
    }

    void Update()
    {
        if (isOn && instance != null)
        {
            instance.Use(drainRate * Time.deltaTime);

            float currentDurability = instance.GetCurrentDurability();

            // Start fading when durability is in last few seconds
            if (currentDurability <= fadeDuration)
            {
                float fadePercent = Mathf.Clamp01(currentDurability / fadeDuration);
                flashlight.intensity = Mathf.Lerp(0, maxIntensity, fadePercent);
            }

            if (instance.IsBroken())
            {
                TurnOff();
            }
        }

        /* 
        if (!isOn && instance != null) {
            // make flashlight lose battery slowly while off
            instance.Use((drainRate / 10f) * Time.deltaTime);
        }
        */

        // Sync flashlight rotation with camera
        if (Camera.main != null && instance != null)
        {
            transform.rotation = Quaternion.Euler(Camera.main.transform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y, 0);
        }
    }

    public void mainAction() // Toggle light
    {
        if (instance == null || instance.IsBroken())
            return;

        if (isOn)
            TurnOff();
        else
            TurnOn();
    }

    public void thirdAction() // Crank flashlight
    {
        if (instance == null)
            return;

        instance.Repair(crankAmount);

        flashlight.intensity = maxIntensity;
    }

    public void secondAction() { }
    public void fourthAction() { }
    public void StopAiming() { }

    private void TurnOn()
    {
        isOn = true;
        if (flashlight != null)
            flashlight.enabled = true;
    }

    private void TurnOff()
    {
        isOn = false;
        if (flashlight != null)
            flashlight.enabled = false;
    }
}
