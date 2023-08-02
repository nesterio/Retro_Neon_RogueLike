using System;
using System.Linq;
using UnityEngine;
using IM = InputManagerData;

namespace PlayerScripts
{
    public class WeaponSwayAndBob : MonoBehaviour
    {
        [SerializeField] float swayIntensityX;
        [SerializeField] float swayIntensityY;
        [Space]
        [SerializeField] float maxSway;
        [SerializeField] float minSway;
        [Space]
        [SerializeField] private float swayMoveSpeed = 0.1f;
        [Space]
        [SerializeField] float aimedSwayMultiplier;
        [SerializeField] float aimedBobMultiplier;

        [Space(10)]

        [SerializeField] BobOverride[] bobOverrides;

        [HideInInspector]
        public bool isAiming;

        [HideInInspector]
        public bool isCrouching;

        float _currentTimeX;
        float _currentTimeY;

        float _xPos;
        float _yPos;

        private Vector3 _smoothV;

        private BobOverride? _lastBob;

        private void Start() => _lastBob = bobOverrides.FirstOrDefault();

        void Update() 
        {
            ProcessSway();

            ProcessBob();

            ChangePosition();
        }

        void ChangePosition()
        {
            Vector3 target = new Vector3(_xPos, _yPos, 0);
            var localPosition = transform.localPosition;
            
            transform.localPosition = Vector3.SmoothDamp(localPosition, target, ref _smoothV, swayMoveSpeed);
        }

        void ProcessSway()
        {
            float xSway = (PlayerManager.CanLook ? -IM.MouseX : 0) * swayIntensityX;
            float ySway = (PlayerManager.CanLook ? -IM.MouseY : 0) * swayIntensityY;

            if (isAiming) 
            {
                xSway *= aimedSwayMultiplier;
                ySway *= aimedSwayMultiplier;
            }

            xSway = Mathf.Clamp(xSway, minSway, maxSway);
            ySway = Mathf.Clamp(ySway, minSway, maxSway);

            _xPos = xSway;
            _yPos = ySway;
        }

        void ProcessBob()
        {
            if(!PlayerMovement.Grounded)
                return;

            BobOverride? currentBob = null;
            foreach (BobOverride bob in bobOverrides)
                if (PlayerMovement.CurrentSpeed >= bob.minSpeed && PlayerMovement.CurrentSpeed < bob.maxSpeed)
                {
                    currentBob = bob;
                    break;
                }
            if (!currentBob.HasValue)
            {
                _lastBob = null;
                return;
            }

            var bobMultiplier = (PlayerMovement.CurrentSpeed == 0) ? 1f : PlayerMovement.CurrentSpeed;

            if (isAiming)
                bobMultiplier = aimedBobMultiplier;

            if (isCrouching)
                bobMultiplier = 1f;

            if (_lastBob.HasValue && !_lastBob.Value.Equals(currentBob.Value))
                _currentTimeX = _currentTimeY = 0.05f;

            _currentTimeX += currentBob.Value.speedX * Time.deltaTime;
            _currentTimeY += currentBob.Value.speedY * Time.deltaTime;

            _xPos += currentBob.Value.bobX.Evaluate(_currentTimeX) * currentBob.Value.intensityX * bobMultiplier;
            _yPos += currentBob.Value.bobY.Evaluate(_currentTimeY) * currentBob.Value.intensityY * bobMultiplier;

            _lastBob = currentBob.Value;
        }

        [System.Serializable]
        public struct BobOverride 
        {
            public float minSpeed;
            public float maxSpeed;
            [Space(5)]

            [Header("X Settings")]
            public float speedX;
            public float intensityX;
            public AnimationCurve bobX;
            [Space(5)]

            [Header("Y Settings")]
            public float speedY;
            public float intensityY;
            public AnimationCurve bobY;
        }
    }
}
