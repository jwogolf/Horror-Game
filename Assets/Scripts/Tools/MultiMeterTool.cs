using UnityEngine;

public class MultiMeterTool : MonoBehaviour, ToolBehavior
{
    private MultiMeter sensor;

    void Start()
    {
        // Assumes the sensor is always on the player
        sensor = FindFirstObjectByType<MultiMeter>();
        if (sensor == null)
            Debug.LogWarning("No active MultiMeterSensor found.");
    }

    public void mainAction()
    {
        sensor?.setCurrentMode(1f);
    }

    public void secondAction()
    {
        sensor?.setCurrentMode(-1f);
    }

    public void thirdAction() { }
    public void fourthAction() { }
    public void StopAiming() { }
}
