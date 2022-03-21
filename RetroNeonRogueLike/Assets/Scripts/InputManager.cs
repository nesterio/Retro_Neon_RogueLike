using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InputManager : MonoBehaviour
{
    public float x;
    public float y;

    [Space(10)]

    public bool jumping;
    public bool sprinting;
    public bool crouching;

    [Space(10)]

    public bool shooting;
    public bool aiming;
    public bool reloading;

    [Space(10)]

    public bool interacting;
    public bool droppingItem;

    [Space(10)]

    public float mouseX;
    public float mouseY;

    PhotonView PV;

    void Awake() 
    {
        PV = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!PV.IsMine)
            return;

        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");

        jumping = Input.GetButton("Jump");

        crouching = Input.GetKey(KeyCode.LeftControl);
        sprinting = Input.GetKey(KeyCode.LeftShift);

        shooting = Input.GetKey(KeyCode.Mouse0);
        aiming = Input.GetKey(KeyCode.Mouse1);

        reloading = Input.GetKey(KeyCode.R);

        droppingItem = Input.GetKey(KeyCode.G);

        interacting = Input.GetKey(KeyCode.E);

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}
