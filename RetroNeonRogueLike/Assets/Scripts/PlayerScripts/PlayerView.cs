using UnityEngine;

namespace PlayerScripts
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private float ViewSensitivity = 25;
        private float xRotation;
        private float desiredX;

        [SerializeField] private Transform PlayerOrientation;

        void Update()
        {
            if(!PlayerManager.CanLook)
                return;
            
            Look();
        }

        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * ViewSensitivity * Time.fixedDeltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * ViewSensitivity * Time.fixedDeltaTime;

            //Find current look rotation
            Vector3 rot = transform.localRotation.eulerAngles;
            desiredX = rot.y + mouseX;

            //Rotate, and also make sure we dont over- or under-rotate.
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //Perform the rotations
            transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
            PlayerOrientation.localRotation = Quaternion.Euler(0, desiredX, 0);
        }
    }
}
