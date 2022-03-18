using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Object references")]
    [SerializeField] Transform recoilPosition;
    [SerializeField] Transform rotationPoint;

    GunInfo gunInfo;
    [Space(10)]

    [Header("Recoil speed")]
   
    [SerializeField] float positionalReturnSpeed = 18f;
    [SerializeField] float rotationalReturnSpeed = 38f;
    [Space(10)]

    Vector3 rotationalRecoil;
    Vector3 positionalRecoil;
    Vector3 Rot;

    void Awake() 
    {
        gunInfo = ((GunInfo)GetComponent<Gun>().itemInfo);
    }

    void FixedUpdate() 
    {
        rotationalRecoil = Vector3.Lerp(rotationalRecoil, Vector3.zero, rotationalReturnSpeed * Time.deltaTime);
        positionalRecoil = Vector3.Lerp(positionalRecoil, Vector3.zero, positionalReturnSpeed * Time.deltaTime);

        recoilPosition.localPosition = Vector3.Slerp(recoilPosition.localPosition, positionalRecoil, gunInfo.positionRecoilSpeed * Time.fixedDeltaTime);
        Rot = Vector3.Slerp(Rot, rotationalRecoil, gunInfo.rotationRecoilSpeed * Time.fixedDeltaTime);
        rotationPoint.localRotation = Quaternion.Euler(Rot);
    }

    public void RecoilFire(bool aiming) 
    {
        if (aiming)
        {
            rotationalRecoil += new Vector3(-gunInfo.recoilRotationAimed.x, Random.Range(-gunInfo.recoilRotationAimed.y, gunInfo.recoilRotationAimed.y), Random.Range(-gunInfo.recoilRotationAimed.z, gunInfo.recoilRotationAimed.z));
            positionalRecoil += new Vector3(Random.Range(-gunInfo.recoilForceAimed.x, gunInfo.recoilForceAimed.x), Random.Range(-gunInfo.recoilForceAimed.y, gunInfo.recoilForceAimed.y), gunInfo.recoilForceAimed.z);
        }
        else 
        {
            rotationalRecoil += new Vector3(-gunInfo.recoilRotation.x, Random.Range(-gunInfo.recoilRotation.y, gunInfo.recoilRotation.y), Random.Range(-gunInfo.recoilRotation.z, gunInfo.recoilRotation.z));
            positionalRecoil += new Vector3(Random.Range(-gunInfo.recoilForce.x, gunInfo.recoilForce.x), Random.Range(-gunInfo.recoilForce.y, gunInfo.recoilForce.y), gunInfo.recoilForce.z);
        }
    }
}
