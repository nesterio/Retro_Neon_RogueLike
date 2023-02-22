using System;
using Photon.Pun;
using UnityEngine;
using IM = InputManager;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Object references")]
        [SerializeField] Rigidbody rb;
        [SerializeField] GameObject PlayerModel;
        [SerializeField] Transform orientation;
        [SerializeField] PlayerStats PS;
        [SerializeField] WeaponSwayAndBob WSAB;

        [Space]

        [Header("Movement")]

        [SerializeField] private float counterMovement = 0.175f;
        private float threshold = 0.01f;

        [SerializeField] private float jumpCooldown;
        private bool readyToJump = true;

        [SerializeField] float extraGravity;

        float speed;

        Vector3 moveDirection;

        [Space]

        [Header("Slopes")]
        [SerializeField] float maxSlopeAngle = 35f;

        RaycastHit slopeHit;
        bool exitingSlope;

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

        [SerializeField] float floorDetectionRange;
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

            ExtraGravity();
        }

        void ExtraGravity() 
        {
            rb.AddForce(Vector3.down * extraGravity);
        }

        void Move()
        {
            moveDirection = orientation.forward * IM.y + orientation.right * IM.x;

            grounded = Physics.Raycast(orientation.position, Vector3.down, floorDetectionRange, whatIsGround);
        

            //Find actual velocity relative to where player is looking
            Vector2 mag = FindVelRelativeToLook();
            float xMag = mag.x, yMag = mag.y;

            //Counteract sliding and sloppy movement
            CounterMovement(IM.x, IM.y, mag);

            //If holding jump && ready to jump, then jump
            if (readyToJump && IM.Jumping)
                Jump();

            //If sliding down a ramp, add force down so player stays grounded and also builds speed
            if (IM.Crouching && grounded && OnSlope())
            {
                rb.AddForce(Vector3.down * Time.deltaTime * 3000);
                return;
            }


            //Some multipliers
            float multiplier = 1f, multiplierV = 1f, diagonalMultiplier = 1f;

            // Movement in air
            if (!grounded)
            {
                multiplier = 0.5f;
                multiplierV = 0.5f;
            }

            // Movement while sliding
            if (grounded && IM.Crouching) multiplierV = 0f;

            if (IM.x != 0 && IM.y != 0)
                diagonalMultiplier = 0.707f;

            if (IM.Sprinting == true && PS.currentStamina > PS.GetStaminaPrice("Sprint"))
            {
                speed = PS.moveSpeed * PS.sprintSpeedMultiplier;
                PS.Sprint(true);
            }
            else 
            {
                speed = PS.moveSpeed;
                PS.Sprint(false);
            }

            if (OnSlope() && !exitingSlope)
                rb.AddForce(GetSlopeMoveDirection() * speed * Time.deltaTime *  multiplier * multiplierV * diagonalMultiplier);
            else
                rb.AddForce(moveDirection.normalized * speed * Time.deltaTime * multiplier * multiplierV * diagonalMultiplier);

        }

        private void Jump()
        {
            if (grounded && readyToJump && PS.currentStamina > PS.GetStaminaPrice("Jump"))
            {
                readyToJump = false;

                //Add jump forces
                if (IM.Crouching) 
                {
                    rb.AddForce(Vector2.up * PS.jumpForce * PS.crouchJumpForceMultiplier * 1.5f);
                    rb.AddForce(normalVector * PS.jumpForce * PS.crouchJumpForceMultiplier * 0.5f);
                }
                else 
                {
                    rb.AddForce(Vector2.up * PS.jumpForce * 1.5f);
                    rb.AddForce(normalVector * PS.jumpForce * 0.5f);
                }

                //If jumping while falling, reset y velocity.
                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f)
                    rb.velocity = new Vector3(vel.x, 0, vel.z);
                else if (rb.velocity.y > 0)
                    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

                Invoke(nameof(ResetJump), jumpCooldown);

                PS.DrainStamina("Jump");

                exitingSlope = true;
            }
        }

        private void ResetJump()
        {
            readyToJump = true;

            exitingSlope = false;
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
            if (!grounded || IM.Jumping) return;

            //Slow down sliding
            if (IM.Crouching)
            {
                rb.AddForce(PS.moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
                return;
            }

            //Counter movement
            if (Math.Abs(mag.x) > threshold && Math.Abs(IM.x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
            {
                rb.AddForce(PS.moveSpeed * orientation.right * Time.deltaTime * -mag.x * counterMovement);
            }
            if (Math.Abs(mag.y) > threshold && Math.Abs(IM.y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
            {
                rb.AddForce(PS.moveSpeed * orientation.forward * Time.deltaTime * -mag.y * counterMovement);
            }


            //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
            if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > PS.maxSpeed)
            {
                float fallspeed = rb.velocity.y;
                Vector3 n = rb.velocity.normalized * PS.maxSpeed;
                rb.velocity = new Vector3(n.x, fallspeed, n.z);
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

        bool OnSlope() 
        {
            if (Physics.Raycast(orientation.position, Vector3.down, out slopeHit, floorDetectionRange)) 
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle < maxSlopeAngle && angle > 0;
            }
            return false;
        }

        Vector3 GetSlopeMoveDirection() 
        {
            return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
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
}
