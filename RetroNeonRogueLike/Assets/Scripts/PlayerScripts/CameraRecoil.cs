using UnityEngine;

namespace PlayerScripts
{
    public class CameraRecoil : MonoBehaviour
    {
        //Rotations
        Vector3 _currentRotation;
        Vector3 _targetRotation;

        [Header("Settings")]
        [SerializeField] float snappiness;
        [SerializeField] float returnSpeed;


        void FixedUpdate()
        {
            _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, snappiness * Time.fixedDeltaTime);

            transform.localRotation = Quaternion.Euler(_currentRotation);
        }

        public void RecoilFire(float recoilX, float recoilY, float recoilZ)=>
            _targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}
