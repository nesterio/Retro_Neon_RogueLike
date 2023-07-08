using System.Collections.Generic;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health")]
        private float _maxHealth;
        public int defaultHealth;
        public float currentHealth;

        public delegate void MaxHealthChange(int newMax);
        public event MaxHealthChange MaxHpChangeEvent;

        [SerializeField] private float healthRegen;
        [Space(10)]

        [Header("Stamina")]
        private float _maxStamina;
        public float defaultStamina;
        public float currentStamina;

        public delegate void MaxStaminaChange(float newMax);
        public event MaxStaminaChange MaxStamChangeEvent;

        [SerializeField] private float staminaRegen;

        bool _sprinting;
        [Space]

        [SerializeField] private StaminaDrain[] staminaDrains;
        private readonly Dictionary<string, StaminaDrain> _drainsDictionary = new Dictionary<string, StaminaDrain>();
        [Space(10)]

        [Header("Movement")]
        public float defaultMoveSpeed;
        public float moveSpeed;

        public float MaxSpeed
        {
            get 
            { 
                if(_sprinting)
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


        public delegate void DeathDelegate();
        public event DeathDelegate DeathEvent;


        void Start() 
        {
            moveSpeed = defaultMoveSpeed;
            jumpForce = defaultJumpForce;

            currentHealth = defaultHealth;

            ChangeMaxHealth(defaultHealth);


            currentStamina = defaultStamina;

            ChangeMaxStamina(defaultStamina);

            foreach (StaminaDrain action in staminaDrains) 
            {
                _drainsDictionary.Add(action.name, action);
            }

        }

        void FixedUpdate() 
        {
            if(currentStamina < _maxStamina && _sprinting == false)
                currentStamina += staminaRegen;

            if (currentHealth < _maxHealth)
                currentHealth += healthRegen;
        }

        ///////////// Health /////////////
        public void DrainHealth(int amount)
        {
            currentHealth -= amount;
            //Debug.Log(gameObject + " just took " + amount + " damage");

            if (currentHealth <= 0)
                DeathEvent();
        }

        public void ChangeMaxHealth(int newMax)
        {
            _maxHealth = newMax;

            if (MaxHpChangeEvent != null)
                MaxHpChangeEvent(newMax);
        }


        ///////////// Stamina /////////////
        public void DrainStamina(string _name)
        {
            currentStamina -= _drainsDictionary[_name].staminaCost;
        }

        public float GetStaminaPrice(string _name) 
        {
            return _drainsDictionary[_name].staminaCost;
        }

        private void ChangeMaxStamina(float newMax) 
        {
            _maxStamina = newMax;

            if(MaxStamChangeEvent != null)
                MaxStamChangeEvent(newMax);
        }

        public void Sprint(bool state) 
        {
            _sprinting = state;

            if(_sprinting)
                DrainStamina("Sprint");
        }

    }

    [System.Serializable]
    public struct StaminaDrain
    {
        public string name;
        public float staminaCost;
    }
}