using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MultiMeter : MonoBehaviour
{
    public enum ScanMode { Location, Radiation, EMF, Experimental }
    private ScanMode currentMode = ScanMode.Location;

    // main detection cone
    public LayerMask scanMask;
    private Transform origin;
    private float maxDistance = 100f;
    private float angle = 20f;

    // radiation damage
    private float damageThreshold = 2.0f;
    private float damagePower = 0.5f;
    private float damageDelay = 10f;
    private float staminaDrainRate = 5f;
    private float stunAfterLeaving = 6f;

    // experimental damage settings
    private float echoDamageThreshold = 2.0f; // Threshold after which echo damage starts
    private float echoDamageIntervalMin = 15f; // Minimum echo damage interval
    private float echoDamageIntervalMax = 45f; // Maximum echo damage interval
    private float echoDamageAmountMin = 1f; // Minimum echo damage amount
    private float echoDamageAmountMax = 10f; // Maximum echo damage amount
    private float echoDamageDurationMultiplier = 15f; // Echo damage duration multiplier
    private float deliriumAddRate = 1.0f;

    // Location
    // coordinated of (0,0,0) in game
    // Centered around central Appalachia
    private readonly float baseLatitude = 37.5f;
    private readonly float baseLongitude = -81.5f;

    // Increase granularity by simulating more movement per coordinate step
    private float gpsGranularityFactor = 10.0f;  // multiplier to exaggerate coordinate change
    private Vector2 trueCoords;
    private Vector2 targetFuzzyCoords;
    private Vector2 currentFuzzyCoords;
    private float coordFuzz = 0.0005f;
    private string displayCoords;

    // Levels
    private float radiationLevel;
    private float emfLevel;
    private float experimentalLevel;

    public float RadiationLevel => radiationLevel;
    public float EmfLevel => emfLevel;
    public float ExperimentalLevel => experimentalLevel;
    public string Coordinates => displayCoords;

    private FirstPersonController player;

    // Consequences of getting close to radiation
    private float timeInRadiation = 0f;
    private float timeOutOfRadiation = 0f;
    private float timeInExperimental = 0f;
    private float timeOutOfExperimental = 0f;
    private float lastEchoDamageTime = 0f;
    private float echoDamageInterval = 30f; // To be randomized
    private float echoDamageAmount = 0f; // To be randomized
    private float echoDamageDuration = 0f; // To be calculated based on time in the affected area

    // meter background noise
    private float noiseStrength = 0.05f; // +/-  zero to this for each reading
    private float minNoiseRange = 1.0f; // How often the target noise values change (in seconds)
    private float maxNoiseRange = 5.0f; // How often the target noise values change (in seconds)
    // Noise targets for smooth transitions
    private float targetRadiationNoise;
    private float targetEmfNoise;
    private float targetExperimentalNoise;
    private float currentRadiationNoise;
    private float currentEmfNoise;
    private float currentExperimentalNoise;
    private float noiseTimer = 0f;

    public ScanMode GetScanMode()
    {
        return currentMode;
    }

    void Start()
    {
        origin = Camera.main.transform;
        player = GetComponent<FirstPersonController>();

        currentFuzzyCoords = GetRealCoordinates();
        targetFuzzyCoords = currentFuzzyCoords;

        // Initialize with random starting noise values
        targetRadiationNoise = UnityEngine.Random.Range(-noiseStrength, noiseStrength);
        targetEmfNoise = UnityEngine.Random.Range(-noiseStrength, noiseStrength);
        targetExperimentalNoise = UnityEngine.Random.Range(-noiseStrength, noiseStrength);
    }

    public void setCurrentMode(float value) {
        if (value > 0.1) {
            currentMode = (ScanMode)(((int)currentMode + 1) % Enum.GetValues(typeof(ScanMode)).Length);
        }
        if (value < -0.1) {
            int next = (int)currentMode - 1;
            if (next < 0) {
                currentMode = (ScanMode)(Enum.GetValues(typeof(ScanMode)).Length - 1);
            }
            else {
                currentMode = (ScanMode)(next);
            }
        }
    }

    private Vector2 GetRealCoordinates()
    {
        Vector3 pos = transform.position;

        // Approx conversion: 1 lat degree ≈ 111 km, 1 lon degree ≈ 111km * cos(latitude)
        float latitude = baseLatitude + (pos.z / 111000f) * gpsGranularityFactor;
        float longitude = baseLongitude + (pos.x / (111000f * Mathf.Cos(baseLatitude * Mathf.Deg2Rad))) * gpsGranularityFactor;

        return new Vector2(latitude, longitude);
    }

    void Update()
    {
        // Location
        Vector2 realCoords = GetRealCoordinates();

        // Still a bug with the display where both readouts are shown silmulteneously
        displayCoords = "---.---- / ---.----";

        if (player.GetVelocity().magnitude < 2f)
        {
            // Update target with new jitter
            targetFuzzyCoords = realCoords + new Vector2(
                UnityEngine.Random.Range(-coordFuzz, coordFuzz),
                UnityEngine.Random.Range(-coordFuzz, coordFuzz)
            );

            // Lerp display value toward target
            currentFuzzyCoords = Vector2.Lerp(currentFuzzyCoords, targetFuzzyCoords, Time.deltaTime);

            displayCoords = currentFuzzyCoords.x.ToString("F4") + " / " + currentFuzzyCoords.y.ToString("F4");
        }


        radiationLevel = 0f;
        emfLevel = 0f;
        experimentalLevel = 0f;

        Collider[] hits = Physics.OverlapSphere(origin.position, maxDistance, scanMask);
        bool isInRadiation = false;

        // EVENTUALLY ADD FUNCTIONALITY TO INCREASE DELIRIUM WHEN CLOSE TO EXPERIMENTAL SOURCE
        //bool isInExperimental = false;

        foreach (var hit in hits)
        {
            RadiationSource source = hit.GetComponent<RadiationSource>();
            if (source == null) continue;

            Vector3 toTarget = hit.transform.position - origin.position;
            float distance = toTarget.magnitude;
            float coneFalloff = Vector3.Angle(origin.forward, toTarget) > angle ?
                Mathf.Clamp01(1f - ((Vector3.Angle(origin.forward, toTarget) - angle) / 180f)) : 1f;

            float falloff = 1f / Mathf.Pow(distance + 1f, 1.5f);

            // Add based on mode
            radiationLevel += Mathf.Max(0f, (source.radiationStrength * falloff * coneFalloff) - 1f);
            emfLevel += Mathf.Max(0f, (source.emfStrength * falloff * coneFalloff) - 1f);
            experimentalLevel += Mathf.Max(0f, (source.experimentalStrength * falloff * coneFalloff) - 1f);
        }

        // Update the noise parameters over time
        noiseTimer += Time.deltaTime;

        float noiseInterval = UnityEngine.Random.Range(minNoiseRange, maxNoiseRange);

        if (noiseTimer >= noiseInterval)
        {
            // Generate new target noise values
            targetRadiationNoise = UnityEngine.Random.Range(-noiseStrength, noiseStrength);
            targetEmfNoise = UnityEngine.Random.Range(-noiseStrength, noiseStrength);
            targetExperimentalNoise = UnityEngine.Random.Range(-noiseStrength, noiseStrength);

            // Reset the timer
            noiseTimer = 0f;
        }

        // Calculate interpolation speed (this will make the interpolation take the entire interval)
        float interpolationSpeed = Time.deltaTime / noiseInterval;

        // Smoothly interpolate towards the new target noise values over the interval
        currentRadiationNoise = Mathf.Lerp(currentRadiationNoise, targetRadiationNoise, interpolationSpeed);
        currentEmfNoise = Mathf.Lerp(currentEmfNoise, targetEmfNoise, interpolationSpeed);
        currentExperimentalNoise = Mathf.Lerp(currentExperimentalNoise, targetExperimentalNoise, interpolationSpeed);

        // Apply the noise to the readings
        radiationLevel += currentRadiationNoise;
        emfLevel += currentEmfNoise;
        experimentalLevel += currentExperimentalNoise;

        // Ensure the readings stay within a reasonable range (prevent negative values or overly large fluctuations)
        radiationLevel = Mathf.Clamp(radiationLevel, 0f, 99.99f);
        emfLevel = Mathf.Clamp(emfLevel, 0f, 99.99f);
        experimentalLevel = Mathf.Clamp(experimentalLevel, 0f, 99.99f);

        // Radiation-specific effects
        if (radiationLevel > damageThreshold)
        {
            player.TakeDamage((Mathf.Pow(radiationLevel, damagePower) - 0.75f) * Time.deltaTime);
            isInRadiation = true;
        }

        if (isInRadiation)
        {
            player.SetStunStamina(true);
            timeInRadiation += Time.deltaTime;
            timeOutOfRadiation = 0f;
        }
        else
        {
            timeOutOfRadiation += Time.deltaTime;
            if (timeOutOfRadiation > stunAfterLeaving)
                player.SetStunStamina(false);
        }

        if (timeOutOfRadiation >= damageDelay && timeInRadiation > 0f)
        {
            player.UseStamina(staminaDrainRate * Time.deltaTime);
            timeInRadiation -= Time.deltaTime;
        }

        // Experimental field effects (sound suppression and echo damage)
        // Experimental field specific effects
        if (experimentalLevel > echoDamageThreshold)
        {
            player.addDelirium(deliriumAddRate * Time.deltaTime);

            // Disrupt sound (e.g., lowpass filter on audio)
            ApplySoundSuppression(experimentalLevel);

            timeInExperimental += Time.deltaTime;
            timeOutOfExperimental = 0f;
        }
        else
        {
            timeOutOfExperimental += Time.deltaTime;

            // Handle Echo Damage (delayed, small damage)
            echoDamageDuration = timeInExperimental * echoDamageDurationMultiplier;
            if (timeOutOfExperimental <= echoDamageDuration)
            {
                if (Time.time - lastEchoDamageTime >= echoDamageInterval)
                {
                    player.TakeDamage(echoDamageAmount);
                    lastEchoDamageTime = Time.time;

                    // Randomize the echo damage values
                    echoDamageInterval = UnityEngine.Random.Range(echoDamageIntervalMin, echoDamageIntervalMax);
                    echoDamageAmount = UnityEngine.Random.Range(echoDamageAmountMin, echoDamageAmountMax);
                }
            }
            else
            {
                timeInExperimental = 0f;
            }
        }
    }

    // Function to simulate sound suppression effect
    private void ApplySoundSuppression(float level)
    {
        float suppressionFactor = Mathf.Clamp01(level / 5f);  // Scaling based on level
        // Here you can apply a filter to the audio listener or use Unity’s AudioLowPassFilter
        AudioListener.volume = Mathf.Clamp01(1f - suppressionFactor); // Simple example, scale volume down
    }

}
