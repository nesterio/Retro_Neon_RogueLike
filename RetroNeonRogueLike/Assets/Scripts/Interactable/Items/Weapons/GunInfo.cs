using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
    [Header("Gun stats")]
    public int damage;
    public float shotSpeed;
    public int magCapacity;
    public float aimingSpeed;
    [Space(10)]

    [Header("Camera recoil")]
    public float recoilX = -3f;
    public float recoilY = 2f;
    public float recoilZ = 0.35f;
    [Space(5)]
    public float recoilAimedX = -1f;
    public float recoilAimedY = 0.5f;
    public float recoilAimedZ = 0.1f;
    [Space(10)]

    [Header("Gun recoil")]
    public float positionRecoilSpeed = 8f;
    public float rotationRecoilSpeed = 8f;
    [Space(5)]
    public Vector3 recoilRotation = new Vector3(10f, 5f, 7f);
    public Vector3 recoilForce = new Vector3(0.015f, 0f, -0.2f);
    [Space(5)]
    public Vector3 recoilRotationAimed = new Vector3(10f, 4f, 6f);
    public Vector3 recoilForceAimed = new Vector3(0.015f, 0f, -0.2f);
    
}
