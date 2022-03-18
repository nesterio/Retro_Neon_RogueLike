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
        PlayerStats.maxStamChangeEvent += ChangeMaxStamina;
        PlayerStats.maxHpChangeEvent += ChangeMaxHealth;
    }

    void OnDisable()
    {
        PlayerStats.maxStamChangeEvent -= ChangeMaxStamina;
        PlayerStats.maxHpChangeEvent -= ChangeMaxHealth;
    }

    void LateUpdate() 
    {
        staminaSlider.value = PlayerStats.currentStamina;
        healthSlider.value = PlayerStats.currentHealth;
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
