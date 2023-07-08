using System.Collections;
using System.Collections.Generic;
using PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

public class IngameUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    

    void OnEnable() 
    {
        playerStats.MaxStamChangeEvent += ChangeMaxStamina;
        playerStats.MaxHpChangeEvent += ChangeMaxHealth;
    }

    void OnDisable()
    {
        playerStats.MaxStamChangeEvent -= ChangeMaxStamina;
        playerStats.MaxHpChangeEvent -= ChangeMaxHealth;
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
