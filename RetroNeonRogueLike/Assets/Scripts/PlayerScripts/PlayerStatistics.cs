using System.Collections.Generic;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerStatistics : MonoBehaviour
    {
        [field: Header("Health")]
        [field: SerializeField]public float DefaultHealth { get; private set; } = 100;
        [field: SerializeField]public float CurrentHealth { get; private set; } = 100;
        [field: SerializeField]public float HealthRegen { get; private set; } = 0.05f;
        private float _maxHealth;

        public delegate void MaxHealthChange(float newMax);
        public event MaxHealthChange MaxHpChangeEvent;
        

        [field: Header("Stamina")]
        [field: SerializeField]public float DefaultStamina { get; private set; } = 100f;
        [field: SerializeField]public float CurrentStamina { get; private set; } = 100f;
        [field: SerializeField]public float StaminaRegen { get; private set; } = 1f;
        private float _maxStamina;

        public delegate void MaxStaminaChange(float newMax);
        public event MaxStaminaChange MaxStamChangeEvent;
        [Space]

        [SerializeField] private StaminaDrain[] staminaDrains;
        private readonly Dictionary<string, StaminaDrain> _drainsDictionary = new Dictionary<string, StaminaDrain>();


        [field: Header("Movement")]
        bool _sprinting;
        [field: SerializeField]public float DefaultMoveSpeed { get; private set; } = 400;
        [field: SerializeField]public float MoveSpeed { get; private set; } = 400;
        [field: SerializeField]public float SprintSpeedMultiplier { get; private set; } = 1.7f;
        [field: SerializeField]public float SlideForce { get; private set; } = 350;
        public float MaxSpeed
        {
            get 
            { 
                if(_sprinting)
                    return MoveSpeed * SprintSpeedMultiplier / 100; /// ???WTF???
                
                return MoveSpeed / 100;
            }
        }
        
        
        [field: Header("Jumping")]
        [field: SerializeField]public float DefaultJumpForce { get; private set; } = 700f;
        public float JumpForce { get; private set; }
        
        [field: SerializeField]public float DefaultJumpForceForwardMultiplier { get; private set; } = 0.25f;
        public float JumpForceForwardMultiplier { get; private set; }
        
        [field: SerializeField]public float CrouchJumpForceMultiplier { get; private set; } = 0.75f;
        
        [field: SerializeField]public int NumberOfJumps { get; private set; } = 1;

        
        [field: Header("Weaponry")]
        [field: SerializeField]public int MaxItems { get; private set; } = 2;
        
        
        public delegate void DeathDelegate();
        public event DeathDelegate DeathEvent;


        void Start() 
        {
            // Locomotion
            MoveSpeed = DefaultMoveSpeed;
            JumpForce = DefaultJumpForce;
            JumpForceForwardMultiplier = DefaultJumpForceForwardMultiplier;

            // Health
            CurrentHealth = DefaultHealth;
            ChangeMaxHealth(DefaultHealth);

            // Stamina
            CurrentStamina = DefaultStamina;
            ChangeMaxStamina(DefaultStamina);
            foreach (StaminaDrain action in staminaDrains)
                _drainsDictionary.Add(action.name, action);
        }

        void FixedUpdate() 
        {
            if(CurrentStamina < _maxStamina && _sprinting == false)
                CurrentStamina += StaminaRegen;

            ChangeHealth(HealthRegen);
        }

        private void Die() => DeathEvent?.Invoke();

            ///////////// Health /////////////
        public void ChangeHealth(float amount)
        {
            if (amount > 0)
            {
                if(CurrentHealth < _maxHealth)
                    CurrentHealth += HealthRegen;
            }
            else 
                CurrentHealth += amount;

            if (CurrentHealth <= 0)
                Die();
        }

        public void ChangeMaxHealth(float newMax)
        {
            _maxHealth = newMax;

            if (MaxHpChangeEvent != null)
                MaxHpChangeEvent(newMax);
        }

        ///////////// Stamina /////////////
        public void DrainStamina(string _name)
        {
            CurrentStamina -= _drainsDictionary[_name].staminaCost;
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