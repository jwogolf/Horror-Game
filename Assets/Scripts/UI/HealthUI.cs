using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;

    public void SetHealth(float current, float max)
    {
        healthFill.fillAmount = current / max;
    }
}
