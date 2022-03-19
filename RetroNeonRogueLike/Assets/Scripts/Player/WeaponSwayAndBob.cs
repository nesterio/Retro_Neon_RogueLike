using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwayAndBob : MonoBehaviour
{
    [SerializeField] InputManager IM;

    public float swayIntensityX;
    public float swayIntensityY;
    [Space]
    public float maxSway;
    public float minSway;
    [Space]
    [SerializeField] float aimedSwayMultiplier;
    [SerializeField] float aimedBobMultiplier;

    [Space(10)]

    public BobOverride[] bobOverrides;


    [HideInInspector]
    public float currentSpeed;

    [HideInInspector]
    public bool isAiming;

    [HideInInspector]
    public bool isCrouching;

    float currentTimeX;
    float currentTimeY;

    float xPos;
    float yPos;

    private Vector3 smoothV;

    void Update() 
    {
        foreach (BobOverride bob in bobOverrides) 
        {
            if (currentSpeed >= bob.minSpeed && currentSpeed <= bob.maxSpeed) 
            {
                float bobMultiplier;
                bobMultiplier = (currentSpeed == 0) ? 1 : currentSpeed;

                if (isAiming)
                    bobMultiplier = aimedBobMultiplier;

                if (isCrouching)
                    bobMultiplier = 1;

                currentTimeX += bob.speedX / 10 * Time.deltaTime * bobMultiplier;
                currentTimeY += bob.speedY / 10 * Time.deltaTime * bobMultiplier;

                xPos = bob.bobX.Evaluate(currentTimeX) * bob.intensityX;
                yPos = bob.bobY.Evaluate(currentTimeY) * bob.intensityY;
            }
        }

        float xSway = -IM.mouseX * swayIntensityX;
        float ySway = -IM.mouseY * swayIntensityY;

        if (isAiming) 
        {
            xSway *= aimedSwayMultiplier;
            ySway *= aimedSwayMultiplier;
        }

        xSway = Mathf.Clamp(xSway, minSway, maxSway);
        ySway = Mathf.Clamp(ySway, minSway, maxSway);

        xPos += xSway;
        yPos += ySway;
    }

    void FixedUpdate()
    {

            Vector3 target = new Vector3(xPos, yPos, 0);
            Vector3 desiredPos = Vector3.SmoothDamp(transform.localPosition, target, ref smoothV, 0.1f);
            transform.localPosition = desiredPos;


        if (transform.localPosition.x > maxSway)
            transform.localPosition = new Vector3(maxSway, transform.localPosition.y, transform.localPosition.z);
        if (transform.localPosition.x < minSway)
            transform.localPosition = new Vector3(minSway, transform.localPosition.y, transform.localPosition.z);

        if (transform.localPosition.y > maxSway)
            transform.localPosition = new Vector3(transform.localPosition.x, maxSway, transform.localPosition.z);
        if (transform.localPosition.y < minSway)
            transform.localPosition = new Vector3(transform.localPosition.x, minSway, transform.localPosition.z);

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
