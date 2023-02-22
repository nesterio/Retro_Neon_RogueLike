using UnityEngine;
using Photon.Pun;

public static class InputManager
{
    public static float x;
    public static float y;

    [Space(10)]

    public static bool Jumping;
    public static bool Sprinting;
    public static bool Crouching;

    [Space(10)]

    public static bool Shooting;
    public static bool Aiming;
    public static bool Reloading;

    [Space(10)]

    public static bool Interacting;
    public static bool DroppingItem;

    [Space(10)]

    public static float MouseX;
    public static float MouseY;
}

class InputManagerObject : MonoBehaviour
{
    PhotonView PV;
    void Awake() =>
        PV = GetComponent<PhotonView>();

    void Update()
    {
        if (!PV.IsMine)
            return;

        InputManager.x = Input.GetAxisRaw("Horizontal");
        InputManager.y = Input.GetAxisRaw("Vertical");

        InputManager.Jumping = Input.GetButton("Jump");

        InputManager.Crouching = Input.GetKey(KeyCode.LeftControl);
        InputManager.Sprinting = Input.GetKey(KeyCode.LeftShift);

        InputManager.Shooting = Input.GetKey(KeyCode.Mouse0);
        InputManager.Aiming = Input.GetKey(KeyCode.Mouse1);

        InputManager.Reloading = Input.GetKey(KeyCode.R);

        InputManager.DroppingItem = Input.GetKey(KeyCode.G);

        InputManager.Interacting = Input.GetKey(KeyCode.E);

        InputManager.MouseX = Input.GetAxis("Mouse X");
        InputManager.MouseY = Input.GetAxis("Mouse Y");
    }
}


