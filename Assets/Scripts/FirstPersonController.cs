using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraTransform; // camera movement
    [SerializeField] InventorySystem inventory;
    [SerializeField] InventoryUI inventoryUI;

    HealthUI healthUI;
    StaminaUI staminaUI;
    CharacterController controller;

    // controls
    [SerializeField] private InputActionAsset inputActionsAsset;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction pickUpAction;

    // Gravity
    private float mass = 1f;

    // Look
    Vector2 look;
    private float xSens = 3f;
    private float ySens = 2f;

    // Movement
    Vector3 velo;
    private Vector3 velocity;
    private float jumpSpeed = 5f;
    private float walkSpeed = 3f;
    private float runSpeed = 7f;
    private Vector3 lastPosition;

    // Stamina
    private float maxStamina = 100f;
    private float staminaRegenRate = 5f;
    private float staminaRegenDelay = 3f;
    private float jumpStaminaCost = 20f;
    private float runStaminaCostPerSecond = 5f;
    private float currentStamina;
    private float staminaRegenCooldown;
    private float noStaminaPenalty = 5f;
    private bool stunStamina = false;

    // Health
    private float maxHealth = 100f;
    private float healthRegenRate = 1f;
    private float healthRegenDelay = 15f;
    private float currentHealth;
    private float healthRegenCooldown;
    private bool stunHealth = false;

    // Hidden metrics
    private float delirium = 0f;
    private float confidence = 0f;

    // Inventory
    private float pickUpReach = 3f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        staminaUI = GetComponent<StaminaUI>();
        healthUI = GetComponent<HealthUI>();
    }

    void OnEnable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player");

        lookAction = playerMap.FindAction("Look"); // right stick
        moveAction = playerMap.FindAction("Move"); // left stick
        jumpAction = playerMap.FindAction("Jump"); // triangle button
        runAction = playerMap.FindAction("Run"); // left stick down / L3
        pickUpAction = playerMap.FindAction("Pick Up"); // X button

        playerMap.Enable();
    }


    void OnDisable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player");

        playerMap.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentStamina = maxStamina;
        currentHealth = maxHealth;

        staminaUI.SetStamina(currentStamina, maxStamina);
        healthUI.SetHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        UpdateLook();
        UpdateMove();
        UpdateGravity();
        UpdateStamina();
        UpdateHealth();
        PickUp();
    }

    void UpdateGravity()
    {
        var gravity = Physics.gravity * mass * Time.deltaTime;
        velo.y = controller.isGrounded ? -1f : velo.y + gravity.y;
    }

    void UpdateLook()
    {
        Vector2 delta = lookAction.ReadValue<Vector2>() * Time.deltaTime * 100f;

        look.x += delta.x * xSens;
        look.y += delta.y * ySens;

        look.y = Mathf.Clamp(look.y, -89f, 89f);

        transform.localRotation = Quaternion.Euler(0, look.x, 0);
        cameraTransform.localRotation = Quaternion.Euler(-look.y, 0, 0);
    }

    void UpdateMove()
    {
        Vector2 input2D = moveAction.ReadValue<Vector2>();
        Vector3 input = transform.forward * input2D.y + transform.right * input2D.x;
        input = Vector3.ClampMagnitude(input, 1f);

        float curSpeed = walkSpeed;

        bool wantsToRun = runAction.IsPressed() && input.magnitude > 0.1f;
        if (wantsToRun && currentStamina > 0f)
        {
            curSpeed = runSpeed;
            float staminaCost = runStaminaCostPerSecond * Time.deltaTime;
            UseStamina(staminaCost);
        }

        if (jumpAction.WasPressedThisFrame() && controller.isGrounded && currentStamina >= jumpStaminaCost)
        {
            velo.y += jumpSpeed;
            UseStamina(jumpStaminaCost);
        }

        // Calculate velocity as change in position over time
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        controller.Move((input * curSpeed + velo) * Time.deltaTime);
    }

    void UpdateStamina()
    {
        if (staminaRegenCooldown > 0f)
        {
            staminaRegenCooldown -= Time.deltaTime;
        }
        else if (stunStamina)
        {
            staminaRegenCooldown = staminaRegenDelay;
        }
        else
        {
            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
            staminaUI.SetStamina(currentStamina, maxStamina);
        }
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        staminaRegenCooldown = staminaRegenDelay;
        if (currentStamina < 0)
        {
            currentStamina = 0;
            staminaRegenCooldown += noStaminaPenalty;
        }
        staminaUI.SetStamina(currentStamina, maxStamina);
    }

    void UpdateHealth()
    {
        if (healthRegenCooldown > 0f)
        {
            healthRegenCooldown -= Time.deltaTime;
        }
        else if (stunHealth)
        {
            healthRegenCooldown = healthRegenDelay;
            stunHealth = false;
        }
        else if (currentStamina == maxStamina)
        {
            currentHealth = Mathf.Min(currentHealth + healthRegenRate * Time.deltaTime, maxHealth);
            healthUI.SetHealth(currentHealth, maxHealth);
        }
    }

    public float getHealth()
    {
        return currentHealth;
    }

    public float getStamina()
    {
        return currentStamina;
    }

    public void TakeDamage(float amount)
    {
        // ADD VISUAL CUE
        stunHealth = true;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // YOU ARE DEAD
            // ADD CODE TO HANDLE DEATH
        }
        healthRegenCooldown = healthRegenDelay;
        healthUI.SetHealth(currentHealth, maxHealth);
    }

    public void addDelirium(float value)
    {
        delirium += value;
    }

    public void addConfidence(float value)
    {
        confidence += value;
    }

    public float getDelirium()
    {
        return delirium;
    }

    public float getConfidence()
    {
        return confidence;
    }


    // HANDLE THE CONSEQUENCES OF DIFFERENT CONFIDENCE AND DELIRIUM LEVELS HERE


    void PickUp()
    {
        // ADD NAME OF THE ITEM HOVERING OVER THE ITEM
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickUpReach))
        {
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                if (pickUpAction.WasPressedThisFrame() && !inventoryUI.IsOpen())
                {
                    pickup.Pickup(inventory);
                }
            }
        }
    }

    public void SetStunHealth(bool val) => stunHealth = val;
    public void SetStunStamina(bool val) => stunStamina = val;
    public Vector3 GetVelocity(){ return velocity; }
}
