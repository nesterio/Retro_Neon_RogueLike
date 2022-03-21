using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerStats : MonoBehaviour
{
    PhotonView PV;

    [Header("Health")]
    private float maxHealth;
    public int defaultHealth;
    public float currentHealth;

    public delegate void maxHealthChange(int newMax);
    public event maxHealthChange maxHpChangeEvent;

    [SerializeField] private float healthRegen;
    [Space(10)]

    [Header("Stamina")]
    private float maxStamina;
    public float defaultStamina;
    public float currentStamina;

    public delegate void maxStaminaChange(float newMax);
    public event maxStaminaChange maxStamChangeEvent;

    [SerializeField] private float staminaRegen;

    bool sprinting;
    [Space]

    [SerializeField] private StaminaDrain[] staminaDrains;
    private Dictionary<string, StaminaDrain> drainsDictionary = new Dictionary<string, StaminaDrain>();
    [Space(10)]

    [Header("Movement")]
    public float defaultMoveSpeed;
    public float moveSpeed;

    public float maxSpeed
    {
        get 
        { 
            if(sprinting)
                return moveSpeed * sprintSpeedMultiplier / 100; 
            else
                return moveSpeed / 100;
        }
    }

    [Space]
    public float sprintSpeedMultiplier;
    [Space(10)]

    [Header("Jumping")]
    public float defaultJumpForce;
    public float jumpForce;
    public float crouchJumpForceMultiplier;
    [Space]
    public int numberOfJumps = 1;
    [Space(10)]

    [Header("Weaponry")]
    public int maxItems = 2;


    public delegate void death();
    public event death deathEvent;


    void Start() 
    {
        PV = GetComponent<PhotonView>();

        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;

        currentHealth = defaultHealth;

        ChangeMaxHealth(defaultHealth);


        currentStamina = defaultStamina;

        ChangeMaxStamina(defaultStamina);

        foreach (StaminaDrain action in staminaDrains) 
        {
           drainsDictionary.Add(action.name, action);
        }

    }

    void FixedUpdate() 
    {
        if (!PV.IsMine)
            return;

        if(currentStamina < maxStamina && sprinting == false)
            currentStamina += staminaRegen;

        if (currentHealth < maxHealth)
            currentHealth += healthRegen;
    }

    ///////////// Health /////////////
    public void DrainHealth(int amount)
    {
        PV.RPC("RPC_DrainHealth", RpcTarget.All, amount);
    }

    [PunRPC]
    void RPC_DrainHealth(int amount) 
    {
        if (!PV.IsMine)
            return;

        currentHealth -= amount;
        //Debug.Log(gameObject + " just took " + amount + " damage");

        if (currentHealth <= 0)
            deathEvent();
    }

    public void ChangeMaxHealth(int newMax)
    {
        maxHealth = newMax;

        if (maxHpChangeEvent != null)
            maxHpChangeEvent(newMax);
    }


    ///////////// Stamina /////////////
    public void DrainStamina(string _name)
    {
        currentStamina -= drainsDictionary[_name].staminaCost;
    }

    public float GetStaminaPrice(string _name) 
    {
        return drainsDictionary[_name].staminaCost;
    }

    public void ChangeMaxStamina(float newMax) 
    {
        maxStamina = newMax;

        if(maxStamChangeEvent != null)
            maxStamChangeEvent(newMax);
    }

    public void Sprint(bool state) 
    {
        sprinting = state;

        if(sprinting)
            DrainStamina("Sprint");
    }

}

[System.Serializable]
public struct StaminaDrain
{
    public string name;
    public float staminaCost;
}

