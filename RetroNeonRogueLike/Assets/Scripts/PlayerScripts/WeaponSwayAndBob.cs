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

        void Update() 
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
            
            foreach (BobOverride bob in bobOverrides) 
            {
                if (PlayerMovement.CurrentSpeed >= bob.minSpeed && PlayerMovement.CurrentSpeed < bob.maxSpeed) 
                {
                    var bobMultiplier = (PlayerMovement.CurrentSpeed == 0) ? 1f : PlayerMovement.CurrentSpeed;

                    if (isAiming)
                        bobMultiplier = aimedBobMultiplier;

                    if (isCrouching)
                        bobMultiplier = 1f;

                    _currentTimeX += bob.speedX * Time.deltaTime;
                    _currentTimeY += bob.speedY * Time.deltaTime;

                    _xPos += bob.bobX.Evaluate(_currentTimeX) * bob.intensityX * bobMultiplier;
                    _yPos += bob.bobY.Evaluate(_currentTimeY) * bob.intensityY * bobMultiplier;
                    
                    break;
                }
            }
        }

        void FixedUpdate()
        {
            Vector3 target = new Vector3(_xPos, _yPos, 0);
            var localPosition = transform.localPosition;
            
            transform.localPosition = Vector3.SmoothDamp(localPosition, target, ref _smoothV, swayMoveSpeed);
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