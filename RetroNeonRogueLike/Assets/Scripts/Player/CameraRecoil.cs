using UnityEngine;

public class CameraRecoil : MonoBehaviour
{

    //Rotations
    Vector3 currentRotation;
    Vector3 targetRotation;

    [Header("Settings")]
    [SerializeField] float snappiness;
    [SerializeField] float returnSpeed;


    void FixedUpdate()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ) 
    {
            targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}
