using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    [Header("Object references")]
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject PlayerModel;
    [SerializeField] Transform orientation;
    [SerializeField] PlayerStats PStats;
    [SerializeField] InputManager IM;
    [SerializeField] WeaponSwayAndBob WSAB;

    [Space]

    [Header("Movement")]

    [SerializeField] private float counterMovement = 0.175f;
    private float threshold = 0.01f;

    [SerializeField] private float jumpCooldown;
    private bool readyToJump = true;

    [Space]

    [Header("Slopes")]
    public float maxSlopeAngle = 35f;
    private float slopeThreshold = 0.1f;
    [SerializeField] private float distanceToSlope;
    [SerializeField] private float slopeRayHeight = 0f;

    [SerializeField] private bool onSlope;

    [Space]

    [Header("Crouch & Slide")]
    private Vector3 crouchScale;
    private Vector3 playerScale;
    [SerializeField] private float slideForce = 400;
    [SerializeField] private float slideCounterMovement = 0.2f;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    private bool grounded;

    [Space]

    [SerializeField] private LayerMask whatIsGround;


    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        playerScale = PlayerModel.transform.localScale;
        crouchScale = new Vector3(playerScale.x, playerScale.y / 2f, playerScale.z);
    }

    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;
        
        Move();
    }

    void Update() 
    {
        if (!PV.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();

        WSAB.currentSpeed = AllowWeaponSway() ? rb.velocity.magnitude : 0;
    }

    void Move()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(IM.x, IM.y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && IM.jumping) Jump();

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (IM.crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (IM.x > 0 && xMag > PStats.maxSpeed) IM.x = 0;
        if (IM.x < 0 && xMag < -PStats.maxSpeed) IM.x = 0;
        if (IM.y > 0 && yMag > PStats.maxSpeed) IM.y = 0;
        if (IM.y < 0 && yMag < -PStats.maxSpeed) IM.y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f, diagonalMultiplier = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && IM.crouching) multiplierV = 0f;

        if (IM.x != 0 && IM.y != 0)
            diagonalMultiplier = 0.707f;


        //Apply forces to move player
        if(IM.sprinting == true && PlayerStats.currentStamina > PStats.GetStaminaPrice("Sprint")) 
        {
            rb.AddForce(orientation.forward * IM.y * PStats.moveSpeed * PStats.sprintSpeedMultiplier * Time.deltaTime * multiplier * multiplierV * diagonalMultiplier);
            rb.AddForce(orientation.right * IM.x * PStats.moveSpeed * PStats.sprintSpeedMultiplier * Time.deltaTime * multiplier * diagonalMultiplier);
            PStats.Sprint(true);
        }
        else
        {
            rb.AddForce(orientation.forward * IM.y * PStats.moveSpeed * Time.deltaTime * multiplier * multiplierV * diagonalMultiplier);
            rb.AddForce(orientation.right * IM.x * PStats.moveSpeed * Time.deltaTime * multiplier * diagonalMultiplier);
            PStats.Sprint(false);
        }

    }

    private void Jump()
    {
        if (grounded && readyToJump && PlayerStats.currentStamina > PStats.GetStaminaPrice("Jump"))
        {
            readyToJump = false;

            //Add jump forces
            if (IM.crouching) 
            {
                rb.AddForce(Vector2.up * PStats.jumpForce * PStats.crouchJumpForceMultiplier * 1.5f);
                rb.AddForce(normalVector * PStats.jumpForce * PStats.crouchJumpForceMultiplier * 0.5f);
            }
            else 
            {
                rb.AddForce(Vector2.up * PStats.jumpForce * 1.5f);
                rb.AddForce(normalVector * PStats.jumpForce * 0.5f);
            }

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);

            PStats.DrainStamina("Jump");
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    
    private void StartCrouch()
    {
        Debug.Log("start crouch");
        GetComponent<CapsuleCollider>().height = 1f;
        PlayerModel.transform.localScale = crouchScale;
        
        if (rb.velocity.magnitude > 0.5f&& grounded)
            rb.AddForce(orientation.forward * slideForce);
        
        WSAB.isCrouching = true;
    }

    private void StopCrouch()
    {
        Debug.Log("stop crouch");
        GetComponent<CapsuleCollider>().height = 2f;
        PlayerModel.transform.localScale = playerScale;

        WSAB.isCrouching = false;

    }
    
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || IM.jumping) return;

        //Slow down sliding
        if (IM.crouching)
        {
            rb.AddForce(PStats.moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(IM.x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(PStats.moveSpeed * orientation.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(IM.y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(PStats.moveSpeed * orientation.forward * Time.deltaTime * -mag.y * counterMovement);
        }


        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > PStats.maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * PStats.maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }

        if (onSlope) 
        {
            if (!grounded)
                onSlope = false;

        }

    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }

            if (Vector3.Angle(normal, Vector3.up) > 0 && Vector3.Angle(normal, Vector3.up) < maxSlopeAngle && grounded)
                onSlope = true;
            else
                onSlope = false;

            //Debug.Log(Vector3.Angle(normal, Vector3.up));
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    private bool AllowWeaponSway() 
    {
        if (IM.x != 0 || IM.y != 0)
        {
            if (grounded)
                return true;
        }
        
        return false;
    }

}
