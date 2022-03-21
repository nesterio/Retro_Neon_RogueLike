using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    

    void OnEnable() 
    {
        playerStats.maxStamChangeEvent += ChangeMaxStamina;
        playerStats.maxHpChangeEvent += ChangeMaxHealth;
    }

    void OnDisable()
    {
        playerStats.maxStamChangeEvent -= ChangeMaxStamina;
        playerStats.maxHpChangeEvent -= ChangeMaxHealth;
    }

    void LateUpdate() 
    {
        staminaSlider.value = playerStats.currentStamina;
        healthSlider.value = playerStats.currentHealth;
    }

    void ChangeMaxStamina(float newMax)
    {
        staminaSlider.maxValue = newMax;
    }

    void ChangeMaxHealth(int newMax) 
    {
        healthSlider.maxValue = newMax;
    }
}
