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
        public float currentSpeed;

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
            foreach (BobOverride bob in bobOverrides) 
            {
                if (currentSpeed >= bob.minSpeed && currentSpeed <= bob.maxSpeed) 
                {
                    var bobMultiplier = (currentSpeed == 0) ? 1 : currentSpeed;

                    if (isAiming)
                        bobMultiplier = aimedBobMultiplier;

                    if (isCrouching)
                        bobMultiplier = 1;

                    _currentTimeX += bob.speedX / 10 * Time.deltaTime * bobMultiplier;
                    _currentTimeY += bob.speedY / 10 * Time.deltaTime * bobMultiplier;

                    _xPos = bob.bobX.Evaluate(_currentTimeX) * bob.intensityX;
                    _yPos = bob.bobY.Evaluate(_currentTimeY) * bob.intensityY;
                }
            }

            float xSway = -IM.MouseX * swayIntensityX;
            float ySway = -IM.MouseY * swayIntensityY;

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
