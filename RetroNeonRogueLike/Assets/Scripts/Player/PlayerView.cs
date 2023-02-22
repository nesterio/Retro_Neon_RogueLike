using Photon.Pun;
using UnityEngine;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private float MouseSensitivity;
        private float xRotation;
        private float desiredX;

        [SerializeField] private Transform PlayerOrientation;
        [SerializeField] private Transform CameraViewPoint;

        PhotonView PV;

        void Awake()
        {
            PV = GetComponent<PhotonView>();
        }

        void Update()
        {
            if (!PV.IsMine)
                return;

            Look();
        }

        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.fixedDeltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.fixedDeltaTime;

            //Find current look rotation
            Vector3 rot = CameraViewPoint.localRotation.eulerAngles;
            desiredX = rot.y + mouseX;

            //Rotate, and also make sure we dont over- or under-rotate.
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //Perform the rotations
            CameraViewPoint.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
            PlayerOrientation.localRotation = Quaternion.Euler(0, desiredX, 0);
        }
    }
}
