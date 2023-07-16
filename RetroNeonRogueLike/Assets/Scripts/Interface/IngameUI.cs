using PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Interface
{
    public class IngameUI : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider staminaSlider;
    

        void OnEnable() 
        {
            PlayerManager.PlayerStats.MaxStamChangeEvent += ChangeMaxStamina;
            PlayerManager.PlayerStats.MaxHpChangeEvent += ChangeMaxHealth;
        }

        void OnDisable()
        {
            PlayerManager.PlayerStats.MaxStamChangeEvent -= ChangeMaxStamina;
            PlayerManager.PlayerStats.MaxHpChangeEvent -= ChangeMaxHealth;
        }

        void LateUpdate() 
        {
            staminaSlider.value = PlayerManager.PlayerStats.CurrentStamina;
            healthSlider.value = PlayerManager.PlayerStats.CurrentHealth;
        }

        void ChangeMaxStamina(float newMax)
        {
            staminaSlider.maxValue = newMax;
        }

        void ChangeMaxHealth(float newMax) 
        {
            healthSlider.maxValue = newMax;
        }
    }
}
