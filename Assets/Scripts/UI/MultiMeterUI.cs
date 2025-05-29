using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiMeterUI : MonoBehaviour
{
    public MultiMeter meter;
    public TMP_Text readoutText;

    void Update()
    {
        float radReading = meter.RadiationLevel;
        float emfReading = meter.EmfLevel;
        float expReading = meter.ExperimentalLevel;
        string locationReading = meter.Coordinates;

        string reading = "";
        if (meter.GetScanMode() == MultiMeter.ScanMode.Location) {
            reading = locationReading;
        }
        if (meter.GetScanMode() == MultiMeter.ScanMode.Radiation) {
            reading = radReading.ToString("F2") + " μSv";
        }
        if (meter.GetScanMode() == MultiMeter.ScanMode.EMF) {
            reading = emfReading.ToString("F2") + " mV";
        }
        if (meter.GetScanMode() == MultiMeter.ScanMode.Experimental) {
            reading = expReading.ToString("F2") + " EXP";
        }

        readoutText.text = reading;
    }
}
