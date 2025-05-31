using UnityEngine;
using System;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    [SerializeField] Texture2D dawnSkybox;
    [SerializeField] Texture2D daySkybox;
    [SerializeField] Texture2D duskSkybox;
    [SerializeField] Texture2D nightSkybox;

    [SerializeField] Light mainLight;

    [SerializeField] Gradient nightDawn;
    [SerializeField] Gradient dawnDay;
    [SerializeField] Gradient dayDusk;
    [SerializeField] Gradient duskNight;

    private float nightLight = 0.1f;
    private float dawnLight = 1.0f;
    private float dayLight = 1.5f;
    private float duskLight = 0.7f;

    private float nightAmbient = 0.05f;
    private float dawnAmbient = 1.5f;
    private float dayAmbient = 1.0f;
    private float duskAmbient = 1.5f;

    private float transitionPeriod = 60f;

    private int dawnStart = 7;
    private int dayStart = 8;
    private int duskStart = 18;
    private int nightStart = 19;


    // Time of Day
    public int minute;
    public int Minutes 
    { get { return minute; } set { minute = value; OnMinuteChange(value); } }

    public int hour;
    public int Hours
    {
        get { return hour; }
        set
        {
            hour = value;
            if (!suppressHourTransition)
                OnHourChange(value);
        }
    }

    public int day;
    public int Days 
    { get { return day; } set { day = value; OnDayChange(value); } }

    private Quaternion defaultSunRotation;
    // Time when sun is in default position
    private int stdSunPosHour = 11;
    private int stdSunPosMinute = 0;

    // for loading time
    public bool suppressHourTransition = false;

    private float tempSeconds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RenderSettings.sun = mainLight;

        defaultSunRotation = mainLight.transform.rotation;

        minute = 50;
        hour = 7;
        day = 0;

        updateSky(hour, minute);
    }

    // Update is called once per frame
    void Update()
    {
        tempSeconds += Time.deltaTime;

        mainLight.transform.Rotate(Vector3.forward, (1f / 1440f) * 360f * Time.deltaTime, Space.World);

        if (tempSeconds >= 1) {
            Minutes += 1;
            tempSeconds = 0;
        }
    }

    private void OnMinuteChange(int value) {
        if (value >= 60) {
            Hours++;
            minute = 0;

            if (Hours >= 24) {
                Days++;
                Hours = 0;
            }
        }
    }

    private void OnHourChange(int value) {
        if (hour == dawnStart){
            StartCoroutine(LerpSkybox(nightSkybox, dawnSkybox, transitionPeriod));
            StartCoroutine(LerpLight(nightDawn, transitionPeriod));
            StartCoroutine(LerpBrightness(nightLight, dawnLight, nightAmbient, dawnAmbient, transitionPeriod));
        }
        else if (hour == dayStart) {
            StartCoroutine(LerpSkybox(dawnSkybox, daySkybox, transitionPeriod / 3f));
            StartCoroutine(LerpLight(dawnDay, transitionPeriod / 3f));
            StartCoroutine(LerpBrightness(dawnLight, dayLight, dawnAmbient, dayAmbient, transitionPeriod / 3f));
        }
        else if (hour == duskStart) {
            StartCoroutine(LerpSkybox(daySkybox, duskSkybox, transitionPeriod));
            StartCoroutine(LerpLight(dayDusk, transitionPeriod));
            StartCoroutine(LerpBrightness(dayLight, duskLight, dayAmbient, duskAmbient, transitionPeriod));
        }
        else if (hour == nightStart) {
            StartCoroutine(LerpSkybox(duskSkybox, nightSkybox, transitionPeriod / 3f));
            StartCoroutine(LerpLight(duskNight, transitionPeriod / 3f));
            StartCoroutine(LerpBrightness(duskLight, nightLight, duskAmbient, nightAmbient, transitionPeriod / 3f));
        }
    }

    private void OnDayChange(int value) {
        // maybe dont need maybe do
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time) {
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);

        for (float i = 0; i < time; i += Time.deltaTime) {
            RenderSettings.skybox.SetFloat("_Blend", i / time);
            yield return null;
        }

        RenderSettings.skybox.SetTexture("_Texture1", b);
    }

    private IEnumerator LerpLight(Gradient lightGradient, float time) {
        for (float i = 0; i < time; i += Time.deltaTime) {
            mainLight.color = lightGradient.Evaluate(i / time);
            yield return null;
        }
    }

    private IEnumerator LerpBrightness (float startLight, float endLight, float startAmb, float endAmb, float time) {
        float lightDiff = endLight - startLight;
        float ambDiff = endAmb - startAmb;
        for (float i = 0; i < time; i += Time.deltaTime) {
            mainLight.intensity = startLight + (lightDiff * (i / time));
            RenderSettings.ambientIntensity = startAmb + (ambDiff * (i / time));
            yield return null;
        }
    }

    public void updateSky(int hour, int minute) {
        int minuteDiff = ((hour - stdSunPosHour) * 60) + (minute - stdSunPosMinute);

        mainLight.transform.rotation = defaultSunRotation;

        float minutesPerDay = 1440f;
        float anglePerMinute = 360f / minutesPerDay;
        float rotationAmount = minuteDiff * anglePerMinute;

        mainLight.transform.Rotate(Vector3.forward, rotationAmount, Space.World);

        // night
        if (hour < dawnStart || hour > nightStart) {
            RenderSettings.skybox.SetTexture("_Texture1", nightSkybox);
            RenderSettings.skybox.SetTexture("_Texture2", dawnSkybox);
            RenderSettings.skybox.SetFloat("_Blend", 0);
            mainLight.intensity = nightLight;
            RenderSettings.skybox.SetColor("_TintColor1", nightDawn.Evaluate(0f));
            RenderSettings.skybox.SetColor("_TintColor2", nightDawn.Evaluate(1f));
            mainLight.color = nightDawn.Evaluate(0f);
            RenderSettings.ambientIntensity = nightAmbient;
        }
        // dawn
        else if (hour < dayStart)
        {
            RenderSettings.skybox.SetTexture("_Texture1", dawnSkybox);
            RenderSettings.skybox.SetTexture("_Texture2", daySkybox);
            RenderSettings.skybox.SetFloat("_Blend", 0);
            mainLight.intensity = dawnLight;
            RenderSettings.skybox.SetColor("_TintColor1", dawnDay.Evaluate(0f));
            RenderSettings.skybox.SetColor("_TintColor2", dawnDay.Evaluate(1f));
            mainLight.color = dawnDay.Evaluate(0f);
            RenderSettings.ambientIntensity = dawnAmbient;
        }
        // day
        else if (hour < duskStart)
        {
            RenderSettings.skybox.SetTexture("_Texture1", daySkybox);
            RenderSettings.skybox.SetTexture("_Texture2", duskSkybox);
            RenderSettings.skybox.SetFloat("_Blend", 0);
            mainLight.intensity = dayLight;
            RenderSettings.skybox.SetColor("_TintColor1", dayDusk.Evaluate(0f));
            RenderSettings.skybox.SetColor("_TintColor2", dayDusk.Evaluate(1f));
            mainLight.color = dayDusk.Evaluate(0f);
            RenderSettings.ambientIntensity = dayAmbient;
        }
        // dusk
        else if (hour < nightStart)
        {
            RenderSettings.skybox.SetTexture("_Texture1", duskSkybox);
            RenderSettings.skybox.SetTexture("_Texture2", nightSkybox);
            RenderSettings.skybox.SetFloat("_Blend", 0);
            mainLight.intensity = duskLight;
            RenderSettings.skybox.SetColor("_TintColor1", duskNight.Evaluate(0f));
            RenderSettings.skybox.SetColor("_TintColor2", duskNight.Evaluate(1f));
            mainLight.color = duskNight.Evaluate(0f);
            RenderSettings.ambientIntensity = duskAmbient;
        }
    }
}
