using UnityEngine;
using System;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    // sky color
    [SerializeField] Color dawnSkybox;
    [SerializeField] Color daySkybox;
    [SerializeField] Color duskSkybox;
    [SerializeField] Color nightSkybox;

    // horizon color
    [SerializeField] Color dawnHorizon;
    [SerializeField] Color dayHorizon;
    [SerializeField] Color duskHorizon;
    [SerializeField] Color nightHorizon;

    // sun reference
    [SerializeField] Light mainLight;
    [SerializeField] GameObject mainLightObj;

    // sunlight color
    [SerializeField] Color dawnColor;
    [SerializeField] Color dayColor;
    [SerializeField] Color duskColor;
    [SerializeField] Color nightColor;

    // built-in long distance "blue ridge" fog color
    [SerializeField] private Color nightFog;
    [SerializeField] private Color dawnFog;
    [SerializeField] private Color dayFog;
    [SerializeField] private Color duskFog;

    // horizon blend (lower is bigger horizon)
    private float dawnHorizonBlend = 3.0f;
    private float dayHorizonBlend = 10.0f;
    private float duskHorizonBlend = 2.0f;
    private float nightHorizonBlend = 6.0f;

    // sun light intensity
    private float nightLight = 0.05f;
    private float dawnLight = 1.5f;
    private float dayLight = 1.5f;
    private float duskLight = 1.0f;

    // sun ambient light
    private float nightAmbient = 0.5f;
    private float dawnAmbient = 1.5f;
    private float dayAmbient = 1.5f;
    private float duskAmbient = 1.0f;

    // skybox exposure
    private float nightExposure = 0.5f;
    private float dawnExposure = 1.5f;
    private float dayExposure = 2.0f;
    private float duskExposure = 1.25f;

    // star brightness
    private float dayStars = 0.0f;
    private float nightStars = 0.4f;

    // transition time between phases
    private float transitionPeriod = 60f;

    // hour each phase begins
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
    private int stdSunPosHour = 10;
    private int stdSunPosMinute = 37;

    // for loading time
    public bool suppressHourTransition = false;

    private float tempSeconds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RenderSettings.sun = mainLight;

        defaultSunRotation = mainLightObj.transform.rotation;

        minute = 50;
        hour = 6;
        day = 0;

        updateSky(hour, minute);
    }

    // Update is called once per frame
    void Update()
    {
        tempSeconds += Time.deltaTime;

        mainLightObj.transform.Rotate(Vector3.forward, (1f / 1440f) * 360f * Time.deltaTime, Space.World);

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
        if (hour == dawnStart)
        {
            StartCoroutine(LerpSkybox(nightSkybox, dawnSkybox, transitionPeriod));
            StartCoroutine(LerpLight(nightColor, dawnColor, transitionPeriod));
            StartCoroutine(LerpBrightness(nightLight, dawnLight, nightAmbient, dawnAmbient, transitionPeriod));
            StartCoroutine(LerpFog(nightFog, dawnFog, transitionPeriod));
            StartCoroutine(LerpExposure(nightExposure, dawnExposure, transitionPeriod));
            StartCoroutine(LerpHorizon(nightHorizon, dawnHorizon, nightHorizonBlend, dawnHorizonBlend, transitionPeriod));
            StartCoroutine(LerpStars(nightStars, dayStars, transitionPeriod / 2f));
        }
        else if (hour == dayStart)
        {
            StartCoroutine(LerpSkybox(dawnSkybox, daySkybox, transitionPeriod / 3f));
            StartCoroutine(LerpLight(dawnColor, dayColor, transitionPeriod / 3f));
            StartCoroutine(LerpBrightness(dawnLight, dayLight, dawnAmbient, dayAmbient, transitionPeriod / 3f));
            StartCoroutine(LerpFog(dawnFog, dayFog, transitionPeriod / 3f));
            StartCoroutine(LerpExposure(dawnExposure, dayExposure, transitionPeriod / 3f));
            StartCoroutine(LerpHorizon(dawnHorizon, dayHorizon, dawnHorizonBlend, dayHorizonBlend, transitionPeriod / 3f));
        }
        else if (hour == duskStart)
        {
            StartCoroutine(LerpSkybox(daySkybox, duskSkybox, transitionPeriod));
            StartCoroutine(LerpLight(dayColor, duskColor, transitionPeriod));
            StartCoroutine(LerpBrightness(dayLight, duskLight, dayAmbient, duskAmbient, transitionPeriod));
            StartCoroutine(LerpFog(dayFog, duskFog, transitionPeriod));
            StartCoroutine(LerpExposure(dayExposure, duskExposure, transitionPeriod));
            StartCoroutine(LerpHorizon(dayHorizon, duskHorizon, dayHorizonBlend, duskHorizonBlend, transitionPeriod));
        }
        else if (hour == nightStart)
        {
            StartCoroutine(LerpSkybox(duskSkybox, nightSkybox, transitionPeriod / 3f));
            StartCoroutine(LerpLight(duskColor, nightColor, transitionPeriod / 3f));
            StartCoroutine(LerpBrightness(duskLight, nightLight, duskAmbient, nightAmbient, transitionPeriod / 3f));
            StartCoroutine(LerpFog(duskFog, nightFog, transitionPeriod / 3f));
            StartCoroutine(LerpExposure(duskExposure, nightExposure, transitionPeriod / 3f));
            StartCoroutine(LerpHorizon(duskHorizon, nightHorizon, duskHorizonBlend, nightHorizonBlend, transitionPeriod / 3f));
            StartCoroutine(LerpStars(dayStars, nightStars, transitionPeriod / 3f));
        }
    }

    private void OnDayChange(int value) {
        // maybe dont need maybe do
    }
    
    private IEnumerator LerpSkybox(Color a, Color b, float time)
    {
        var grad = new Gradient();

        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(a, 0.0f);
        colors[1] = new GradientColorKey(b, 1.0f);

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 1.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        grad.SetKeys(colors, alphas);

        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetColor("_SkyColor", grad.Evaluate(i / time));
            yield return null;
        }
    }

    private IEnumerator LerpLight(Color a, Color b, float time)
    {
        var grad = new Gradient();

        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(a, 0.0f);
        colors[1] = new GradientColorKey(b, 1.0f);

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 1.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        for (float i = 0; i < time; i += Time.deltaTime)
        {
            mainLight.color = grad.Evaluate(i / time);
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
    
    private IEnumerator LerpFog(Color startColor, Color endColor, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.fogColor = Color.Lerp(startColor, endColor, i / time);
            yield return null;
        }

        RenderSettings.fogColor = endColor;
    }

    private IEnumerator LerpExposure(float start, float end, float time)
    {
        float expDiff = end - start;
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_SkyExposure", start + (expDiff * (i / time)));
            yield return null;
        }
    }

    private IEnumerator LerpHorizon(Color a, Color b, float start, float end, float time)
    {
        var grad = new Gradient();

        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(a, 0.0f);
        colors[1] = new GradientColorKey(b, 1.0f);

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 1.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        grad.SetKeys(colors, alphas);

        float diff = end - start;

        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetColor("_HorizonColor", grad.Evaluate(i / time));
            RenderSettings.skybox.SetFloat("_HorizonBlend", start + (diff * (i / time)));
            yield return null;
        }
    }

    private IEnumerator LerpStars(float start, float end, float time)
    {
        float diff = end - start;
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_StarVisibility", start + (diff * (i / time)));
            yield return null;
        }
    }

    public void updateSky(int hour, int minute)
    {
        int minuteDiff = ((hour - stdSunPosHour) * 60) + (minute - stdSunPosMinute);

        mainLightObj.transform.rotation = defaultSunRotation;

        float minutesPerDay = 1440f;
        float anglePerMinute = 360f / minutesPerDay;
        float rotationAmount = minuteDiff * anglePerMinute;

        mainLightObj.transform.Rotate(Vector3.forward, rotationAmount, Space.World);

        // night
        if (hour < dawnStart || hour > nightStart)
        {
            mainLight.intensity = nightLight;
            mainLight.color = nightColor;
            RenderSettings.ambientIntensity = nightAmbient;
            RenderSettings.fogColor = nightFog;
            RenderSettings.skybox.SetFloat("_SkyExposure", nightExposure);
            RenderSettings.skybox.SetColor("_SkyColor", nightSkybox);
            RenderSettings.skybox.SetColor("_HorizonColor", nightHorizon);
            RenderSettings.skybox.SetFloat("_HorizonBlend", nightHorizonBlend);
            RenderSettings.skybox.SetFloat("_StarVisibility", nightStars);
        }
        // dawn
        else if (hour < dayStart)
        {
            mainLight.intensity = dawnLight;
            mainLight.color = dawnColor;
            RenderSettings.ambientIntensity = dawnAmbient;
            RenderSettings.fogColor = dawnFog;
            RenderSettings.skybox.SetFloat("_SkyExposure", dawnExposure);
            RenderSettings.skybox.SetColor("_SkyColor", dawnSkybox);
            RenderSettings.skybox.SetColor("_HorizonColor", dawnHorizon);
            RenderSettings.skybox.SetFloat("_HorizonBlend", dawnHorizonBlend);
            RenderSettings.skybox.SetFloat("_StarVisibility", dayStars);
        }
        // day
        else if (hour < duskStart)
        {
            mainLight.intensity = dayLight;
            mainLight.color = dayColor;
            RenderSettings.ambientIntensity = dayAmbient;
            RenderSettings.fogColor = dayFog;
            RenderSettings.skybox.SetFloat("_SkyExposure", dayExposure);
            RenderSettings.skybox.SetColor("_SkyColor", daySkybox);
            RenderSettings.skybox.SetColor("_HorizonColor", dayHorizon);
            RenderSettings.skybox.SetFloat("_HorizonBlend", dayHorizonBlend);
            RenderSettings.skybox.SetFloat("_StarVisibility", dayStars);
        }
        // dusk
        else if (hour < nightStart)
        {
            mainLight.intensity = duskLight;
            mainLight.color = duskColor;
            RenderSettings.ambientIntensity = duskAmbient;
            RenderSettings.fogColor = duskFog;
            RenderSettings.skybox.SetFloat("_SkyExposure", duskExposure);
            RenderSettings.skybox.SetColor("_SkyColor", duskSkybox);
            RenderSettings.skybox.SetColor("_HorizonColor", duskHorizon);
            RenderSettings.skybox.SetFloat("_HorizonBlend", duskHorizonBlend);
            RenderSettings.skybox.SetFloat("_StarVisibility", dayStars);
        }
    }
}
