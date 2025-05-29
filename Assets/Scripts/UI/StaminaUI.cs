using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private Image staminaFill;

    public void SetStamina(float current, float max)
    {
        staminaFill.fillAmount = current / max;
    }
}
