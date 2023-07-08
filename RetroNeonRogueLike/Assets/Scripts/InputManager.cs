using UnityEngine;

public struct InputManagerData
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

public class InputManager : MonoBehaviour
{
    void Update()
    {
        InputManagerData.x = Input.GetAxisRaw("Horizontal");
        InputManagerData.y = Input.GetAxisRaw("Vertical");

        InputManagerData.Jumping = Input.GetButton("Jump");

        InputManagerData.Crouching = Input.GetKey(KeyCode.LeftControl);
        InputManagerData.Sprinting = Input.GetKey(KeyCode.LeftShift);

        InputManagerData.Shooting = Input.GetKey(KeyCode.Mouse0);
        InputManagerData.Aiming = Input.GetKey(KeyCode.Mouse1);

        InputManagerData.Reloading = Input.GetKey(KeyCode.R);

        InputManagerData.DroppingItem = Input.GetKey(KeyCode.G);

        InputManagerData.Interacting = Input.GetKey(KeyCode.E);

        InputManagerData.MouseX = Input.GetAxis("Mouse X");
        InputManagerData.MouseY = Input.GetAxis("Mouse Y");
    }
}