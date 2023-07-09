using System;
using UnityEngine;
using inputManager = InputManagerData;

namespace PlayerScripts
{
    public class PlayerMovement_Old : MonoBehaviour
    {
        [Header("Object references")]
        [SerializeField] Rigidbody rb;
        [SerializeField] GameObject playerModel;
        [SerializeField] Transform orientation;
        [SerializeField] PlayerStats pStats;
        [SerializeField] WeaponSwayAndBob WSAB;

        [Space]

        [Header("Movement")]

        [SerializeField] private float counterMovement = 0.175f;

        private const float Threshold = 0.01f;

        [SerializeField] private float jumpCooldown;
        private bool _readyToJump = true;

        [Space]

        [Header("Slopes")]
        public float maxSlopeAngle = 35f;
        private float _slopeThreshold = 0.1f;
        [SerializeField] private float distanceToSlope;
        [SerializeField] private float slopeRayHeight = 0f;

        [SerializeField] private bool onSlope;

        [Space]

        [Header("Crouch & Slide")]
        private Vector3 _crouchScale;
        private Vector3 _playerScale;
        [SerializeField] private float slideForce = 400;
        [SerializeField] private float slideCounterMovement = 0.2f;

        //Sliding
        private Vector3 _normalVector = Vector3.up;
        private Vector3 _wallNormalVector;

        private bool _grounded;

        [Space]

        [SerializeField] private LayerMask whatIsGround;

        void Awake()
        {
            _playerScale = playerModel.transform.localScale;
            _crouchScale = new Vector3(_playerScale.x, _playerScale.y / 2f, _playerScale.z);
        }

        void FixedUpdate()
        {
            Move();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                StartCrouch();
            if (Input.GetKeyUp(KeyCode.LeftControl))
                StopCrouch();
        }

        void Move()
        {
            //Extra gravity
            rb.AddForce(Vector3.down * (Time.deltaTime * 10));

            //Find actual velocity relative to where player is looking
            Vector2 mag = FindVelRelativeToLook();
            float xMag = mag.x, yMag = mag.y;

            //Counteract sliding and sloppy movement
            CounterMovement(inputManager.x, inputManager.y, mag);

            //If holding jump && ready to jump, then jump
            if (_readyToJump && inputManager.Jumping) Jump();

            //If sliding down a ramp, add force down so player stays grounded and also builds speed
            if (inputManager.Crouching && _grounded && _readyToJump)
            {
                rb.AddForce(Vector3.down * (Time.deltaTime * 3000));
                return;
            }

            //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
            if (inputManager.x > 0 && xMag > pStats.MaxSpeed) inputManager.x = 0;
            if (inputManager.x < 0 && xMag < -pStats.MaxSpeed) inputManager.x = 0;
            if (inputManager.y > 0 && yMag > pStats.MaxSpeed) inputManager.y = 0;
            if (inputManager.y < 0 && yMag < -pStats.MaxSpeed) inputManager.y = 0;

            //Some multipliers
            float multiplier = 1f, multiplierV = 1f, diagonalMultiplier = 1f;

            // Movement in air
            if (!_grounded)
            {
                multiplier = 0.5f;
                multiplierV = 0.5f;
            }

            // Movement while sliding
            if (_grounded && inputManager.Crouching) multiplierV = 0f;

            if (inputManager.x != 0 && inputManager.y != 0)
                diagonalMultiplier = 0.707f;


            //Apply forces to move player
            if (inputManager.Sprinting == true && pStats.currentStamina > pStats.GetStaminaPrice("Sprint"))
            {
                rb.AddForce(orientation.forward * (inputManager.y * pStats.moveSpeed * pStats.sprintSpeedMultiplier * Time.deltaTime * multiplier * multiplierV * diagonalMultiplier));
                rb.AddForce(orientation.right * (inputManager.x * pStats.moveSpeed * pStats.sprintSpeedMultiplier * Time.deltaTime * multiplier * diagonalMultiplier));
                pStats.Sprint(true);
            }
            else
            {
                rb.AddForce(orientation.forward * (inputManager.y * pStats.moveSpeed * Time.deltaTime * multiplier * multiplierV * diagonalMultiplier));
                rb.AddForce(orientation.right * (inputManager.x * pStats.moveSpeed * Time.deltaTime * multiplier * diagonalMultiplier));
                pStats.Sprint(false);
            }

        }

        private void Jump()
        {
            if (_grounded && _readyToJump && pStats.currentStamina > pStats.GetStaminaPrice("Jump"))
            {
                _readyToJump = false;

                //Add jump forces
                if (inputManager.Crouching)
                {
                    rb.AddForce(Vector2.up * (pStats.jumpForce * pStats.crouchJumpForceMultiplier * 1.5f));
                    rb.AddForce(_normalVector * (pStats.jumpForce * pStats.crouchJumpForceMultiplier * 0.5f));
                }
                else
                {
                    rb.AddForce(Vector2.up * (pStats.jumpForce * 1.5f));
                    rb.AddForce(_normalVector * (pStats.jumpForce * 0.5f));
                }

                //If jumping while falling, reset y velocity.
                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f)
                    rb.velocity = new Vector3(vel.x, 0, vel.z);
                else if (rb.velocity.y > 0)
                    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

                Invoke(nameof(ResetJump), jumpCooldown);

                pStats.DrainStamina("Jump");
            }
        }

        private void ResetJump()
        {
            _readyToJump = true;
        }


        private void StartCrouch()
        {
            Debug.Log("start crouch");
            GetComponent<CapsuleCollider>().height = 1f;
            playerModel.transform.localScale = _crouchScale;

            if (rb.velocity.magnitude > 0.5f && _grounded)
                rb.AddForce(orientation.forward * slideForce);

            WSAB.isCrouching = true;
        }

        private void StopCrouch()
        {
            Debug.Log("stop crouch");
            GetComponent<CapsuleCollider>().height = 2f;
            playerModel.transform.localScale = _playerScale;

            WSAB.isCrouching = false;

        }

        private void CounterMovement(float x, float y, Vector2 mag)
        {
            if (!_grounded || inputManager.Jumping) return;

            //Slow down sliding
            if (inputManager.Crouching)
            {
                rb.AddForce(-rb.velocity.normalized * (pStats.moveSpeed * Time.deltaTime * slideCounterMovement));
                return;
            }

            //Counter movement
            if (Math.Abs(mag.x) > Threshold && Math.Abs(inputManager.x) < 0.05f || (mag.x < -Threshold && x > 0) || (mag.x > Threshold && x < 0))
            {
                rb.AddForce(orientation.right * (pStats.moveSpeed * Time.deltaTime * -mag.x * counterMovement));
            }
            if (Math.Abs(mag.y) > Threshold && Math.Abs(inputManager.y) < 0.05f || (mag.y < -Threshold && y > 0) || (mag.y > Threshold && y < 0))
            {
                rb.AddForce(orientation.forward * (pStats.moveSpeed * Time.deltaTime * -mag.y * counterMovement));
            }


            //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
            if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > pStats.MaxSpeed)
            {
                float fallspeed = rb.velocity.y;
                Vector3 n = rb.velocity.normalized * pStats.MaxSpeed;
                rb.velocity = new Vector3(n.x, fallspeed, n.z);
            }

            if (onSlope)
            {
                if (!_grounded)
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
                    _grounded = true;
                    cancellingGrounded = false;
                    _normalVector = normal;
                    CancelInvoke(nameof(StopGrounded));
                }

                if (Vector3.Angle(normal, Vector3.up) > 0 && Vector3.Angle(normal, Vector3.up) < maxSlopeAngle && _grounded)
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
            _grounded = false;
        }

        private bool AllowWeaponSway()
        {
            if (inputManager.x != 0 || inputManager.y != 0)
            {
                if (_grounded)
                    return true;
            }

            return false;
        }

    }
}