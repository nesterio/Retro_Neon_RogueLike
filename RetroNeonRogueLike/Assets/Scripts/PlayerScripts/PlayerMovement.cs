using System;
using UnityEngine;
using inputManager = InputManagerData;

namespace PlayerScripts
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Object references")]
        [SerializeField] Rigidbody rb;
        [SerializeField] GameObject playerModel;
        [SerializeField] Transform orientation;
        [SerializeField] PlayerStats playerStat;
        [SerializeField] WeaponSwayAndBob WSAB;

        [Space]

        [Header("Movement")]

        [SerializeField] private float counterMovement = 0.175f;

        private const float Threshold = 0.01f;

        [SerializeField] private float jumpCooldown;
        private bool _readyToJump = true;

        float _speed;

        Vector3 _moveDirection;

        [Space]

        [Header("Slopes")]
        [SerializeField] float maxSlopeAngle = 35f;

        RaycastHit _slopeHit;
        bool _exitingSlope;

        [Space]

        [Header("Crouch & Slide")]
        private Vector3 _crouchScale;
        private Vector3 _playerScale;
        [SerializeField] private float slideForce = 400;
        [SerializeField] private float slideCounterMovement = 0.2f;

        //Sliding
        private readonly Vector3 _normalVector = Vector3.up;
        private Vector3 _wallNormalVector;

        private bool _grounded;

        [Space]

        [SerializeField] float floorDetectionRange;
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

            WSAB.currentSpeed = AllowWeaponSway() ? rb.velocity.magnitude : 0;
        }

        void Move()
        {
            _moveDirection = orientation.forward * inputManager.y + orientation.right * inputManager.x;

            _grounded = Physics.Raycast(orientation.position, Vector3.down, floorDetectionRange, whatIsGround);
            
            //Find actual velocity relative to where player is looking
            Vector2 mag = FindVelRelativeToLook();
            float xMag = mag.x, yMag = mag.y;

            //Counteract sliding and sloppy movement
            CounterMovement(inputManager.x, inputManager.y, mag);

            //If holding jump && ready to jump, then jump
            if (_readyToJump && inputManager.Jumping)
                Jump();

            //If sliding down a ramp, add force down so player stays grounded and also builds speed
            if (inputManager.Crouching && _grounded && OnSlope())
            {
                rb.AddForce(Vector3.down * (Time.deltaTime * 3000));
                return;
            }
            
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

            if (inputManager.Sprinting && playerStat.currentStamina > playerStat.GetStaminaPrice("Sprint"))
            {
                _speed = playerStat.moveSpeed * playerStat.sprintSpeedMultiplier;
                playerStat.Sprint(true);
            }
            else 
            {
                _speed = playerStat.moveSpeed;
                playerStat.Sprint(false);
            }

            if (OnSlope() && !_exitingSlope)
                rb.AddForce(GetSlopeMoveDirection() * (_speed * Time.deltaTime * multiplier * multiplierV * diagonalMultiplier));
            else
                rb.AddForce(_moveDirection.normalized * (_speed * Time.deltaTime * multiplier * multiplierV * diagonalMultiplier));
        }

        private void Jump()
        {
            if (_grounded && _readyToJump && playerStat.currentStamina > playerStat.GetStaminaPrice("Jump"))
            {
                _readyToJump = false;

                //Add jump forces
                if (inputManager.Crouching) 
                {
                    rb.AddForce(Vector2.up * (playerStat.jumpForce * playerStat.crouchJumpForceMultiplier * 1.5f));
                    rb.AddForce(_normalVector * (playerStat.jumpForce * playerStat.crouchJumpForceMultiplier * 0.5f));
                }
                else 
                {
                    rb.AddForce(Vector2.up * (playerStat.jumpForce * 1.5f));
                    rb.AddForce(_normalVector * (playerStat.jumpForce * 0.5f));
                }

                //If jumping while falling, reset y velocity.
                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f)
                    rb.velocity = new Vector3(vel.x, 0, vel.z);
                else if (rb.velocity.y > 0)
                    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

                Invoke(nameof(ResetJump), jumpCooldown);

                playerStat.DrainStamina("Jump");

                _exitingSlope = true;
            }
        }

        private void ResetJump()
        {
            _readyToJump = true;

            _exitingSlope = false;
        }

    
        private void StartCrouch()
        {
            Debug.Log("start crouch");
            GetComponent<CapsuleCollider>().height = 1f;
            playerModel.transform.localScale = _crouchScale;
        
            if (rb.velocity.magnitude > 0.5f&& _grounded)
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
                rb.AddForce(-rb.velocity.normalized * (playerStat.moveSpeed * Time.deltaTime * slideCounterMovement));
                return;
            }

            //Counter movement
            if (Math.Abs(mag.x) > Threshold && Math.Abs(inputManager.x) < 0.05f || (mag.x < -Threshold && x > 0) || (mag.x > Threshold && x < 0))
            {
                rb.AddForce(orientation.right * (playerStat.moveSpeed * Time.deltaTime * -mag.x * counterMovement));
            }
            if (Math.Abs(mag.y) > Threshold && Math.Abs(inputManager.y) < 0.05f || (mag.y < -Threshold && y > 0) || (mag.y > Threshold && y < 0))
            {
                rb.AddForce(orientation.forward * (playerStat.moveSpeed * Time.deltaTime * -mag.y * counterMovement));
            }


            //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
            if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > playerStat.MaxSpeed)
            {
                float fallspeed = rb.velocity.y;
                Vector3 n = rb.velocity.normalized * playerStat.MaxSpeed;
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
            if (Physics.Raycast(orientation.position, Vector3.down, out _slopeHit, floorDetectionRange)) 
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                return angle < maxSlopeAngle && angle > 0;
            }
            return false;
        }

        Vector3 GetSlopeMoveDirection() 
        {
            return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
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
