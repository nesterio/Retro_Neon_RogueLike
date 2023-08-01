using System;
using SL.Wait;
using UnityEngine;
using inputManager = InputManagerData;

namespace PlayerScripts
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Object references")]
        [SerializeField] Rigidbody rb;
        [SerializeField] private CapsuleCollider collider;
        [SerializeField] GameObject playerModel;
        [SerializeField] Transform orientation;
        [Space]

        [Header("Movement")]
        [SerializeField] private float counterMovement = 1f;
        private const float Threshold = 0.01f;
        [SerializeField] private float jumpCooldown = 1f;
        private bool _readyToJump = true;

        float _speed;
        Vector3 _moveDirection;
        
        [SerializeField] float slopeCancelMultiplier = 100;

        private float walkSpeed => 
            PlayerManager.PlayerStats.MoveSpeed / 10f;
        private float runSpeed =>
            PlayerManager.PlayerStats.MoveSpeed * PlayerManager.PlayerStats.SprintSpeedMultiplier / 10f;
        [Space]

        [Header("Slopes")]
        [SerializeField] float maxSlopeAngle = 35f;
        RaycastHit _slopeHit;
        bool _exitingSlope;
        [Space]

        [Header("Crouch & Slide")]
        private Vector3 _crouchScale;
        private Vector3 _playerScale;
        [SerializeField] private float minSpeedForSlideMultiplier = 0.75f;
        [SerializeField] private float slideCounterMovement = 0.75f;

        //Sliding
        private Vector3 _wallNormalVector;
        private bool _grounded;

        [Space]
        [SerializeField] float floorDetectionRange;
        [SerializeField] private LayerMask whatIsGround;

        public static float CurrentSpeed { get; private set; }

        private bool _playingWalkSound = false;

        void Awake()
        {
            _playerScale = playerModel.transform.localScale;
            _crouchScale = new Vector3(_playerScale.x, _playerScale.y / 2f, _playerScale.z);
        }

        private void Start()
        {
            FModAudioManager.CreateSoundInstance(SoundInstanceType.Walk, "Step", false);
        }

        void Update() 
        {
            CurrentSpeed = AllowWeaponSway() ? rb.velocity.magnitude : 0;
            
            Vector2 mag = FindVelRelativeToLook();
            
            //Counteract sliding and sloppy movement
            CounterMovement(inputManager.x, inputManager.y, mag);
            
            if(!PlayerManager.CanMove)
                return;
            
            Move(mag);
            
            if (Input.GetKeyDown(KeyCode.LeftControl))
                StartCrouch();
            if (Input.GetKeyUp(KeyCode.LeftControl))
                StopCrouch();
        }

        void Move(Vector2 magnitude)
        {
            _moveDirection = orientation.forward * inputManager.y + orientation.right * inputManager.x;

            _grounded = Physics.Raycast(orientation.position, Vector3.down, floorDetectionRange, whatIsGround);
            
            //Find actual velocity relative to where player is looking
            
            float xMag = magnitude.x, yMag = magnitude.y;

            //If sliding down a ramp, add force down so player stays grounded and also builds speed
            if (inputManager.Crouching && _grounded && OnSlope())
            {
                rb.AddForce(Vector3.down * (Time.deltaTime * _speed));
                return;
            }
            
            // Some multipliers
            float multiplierHorizontal = 1f, multiplierVertical = 1f, diagonalMultiplier = 1f;

            // Movement in air
            if (!_grounded)
            {
                multiplierHorizontal = 0.5f;
                multiplierVertical = 0.5f;
            }

            // Movement while sliding
            if (_grounded && inputManager.Crouching) multiplierVertical = 0f;

            if (inputManager.x != 0 && inputManager.y != 0)
                diagonalMultiplier = 0.707f;
            
            if (OnSlope() && !_exitingSlope)
                rb.AddForce(GetSlopeMoveDirection() * (_speed * Time.deltaTime * multiplierHorizontal * multiplierVertical * diagonalMultiplier));
            else
                rb.AddForce(_moveDirection.normalized * (_speed * Time.deltaTime * multiplierHorizontal * multiplierVertical * diagonalMultiplier));

            // Add sound
            // wtf this needs refactor this looks ungodly
            if (_grounded && !inputManager.Crouching && CurrentSpeed > 0)
            {
                if (!_playingWalkSound)
                {
                    FModAudioManager.StartSoundInstance(SoundInstanceType.Walk);
                    
                    _playingWalkSound = true;
                }
            }
            else if(_playingWalkSound)
            {
                FModAudioManager.StopSoundInstance(SoundInstanceType.Walk); 
                
                _playingWalkSound = false;
            }

            if (!PlayerManager.CanMove)
            {
                _speed = 0;
                StopCrouch();
                return;
            }

            //If holding jump && ready to jump, then jump
            if (_readyToJump && inputManager.Jumping)
                Jump();

            // Run or walk
            if (inputManager.Sprinting && PlayerManager.PlayerStats.CurrentStamina > PlayerManager.PlayerStats.GetStaminaPrice("Sprint") && !inputManager.Crouching)
            {
                _speed = PlayerManager.PlayerStats.MoveSpeed * PlayerManager.PlayerStats.SprintSpeedMultiplier;
                PlayerManager.PlayerStats.Sprint(true);
            }
            else 
            {
                _speed = PlayerManager.PlayerStats.MoveSpeed;
                PlayerManager.PlayerStats.Sprint(false);
            }
        }

        private void Jump()
        {
            if (_grounded && _readyToJump && PlayerManager.PlayerStats.CurrentStamina > PlayerManager.PlayerStats.GetStaminaPrice("Jump"))
            {
                _readyToJump = false;

                //// Add jump forces ////
                // Add force to Up
                rb.AddForce(Vector2.up * (PlayerManager.PlayerStats.JumpForce 
                                          * (inputManager.Crouching ? PlayerManager.PlayerStats.CrouchJumpForceMultiplier : 1 )));
                // Add force to movement direction
                rb.AddForce(Vector3.up
                            * (PlayerManager.PlayerStats.JumpForce 
                               * (inputManager.Crouching ? PlayerManager.PlayerStats.CrouchJumpForceMultiplier : 1 ) 
                               * 0.33f));

                //If jumping while falling, reset y velocity.
                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f)
                    rb.velocity = new Vector3(vel.x, 0, vel.z);
                else if (rb.velocity.y > 0)
                    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

                var wait = Wait.Seconds(jumpCooldown, () =>
                {
                    _readyToJump = true;

                    _exitingSlope = false;
                });
                wait.Start();

                PlayerManager.PlayerStats.DrainStamina("Jump");

                _exitingSlope = true;
            }
        }

        private void StartCrouch()
        {
            collider.height = _crouchScale.y *2f;
            playerModel.transform.localScale = _crouchScale;
        
            if (rb.velocity.magnitude > walkSpeed * minSpeedForSlideMultiplier && _grounded) //// WTF MAGIC NUMBERS
                rb.AddForce(orientation.forward * PlayerManager.PlayerStats.SlideForce);
        
            PlayerManager.WSAB.isCrouching = true;
        }

        private void StopCrouch()
        {
            collider.height = _playerScale.y*2f;
            playerModel.transform.localScale = _playerScale;

            PlayerManager.WSAB.isCrouching = false;
        }
    
        private void CounterMovement(float x, float y, Vector2 mag)
        {
            if (!_grounded || inputManager.Jumping) return;

            // Slow down sliding
            if (inputManager.Crouching)
            {
                rb.AddForce(-rb.velocity.normalized * (PlayerManager.PlayerStats.MoveSpeed * Time.deltaTime * slideCounterMovement));
                return;
            }
            
            // Counter unintentional slope sliding
            if (OnSlope() && !inputManager.Crouching) // NOT WORKING :(
            {
                rb.AddForce(orientation.forward * (_slopeHit.normal.y * _slopeHit.normal.z));
                rb.AddForce(orientation.right * (_slopeHit.normal.y * _slopeHit.normal.x));
                //rb.AddForce(GetCancelSlopeMovement() * slopeCancelMultiplier );
            }

            // Counter movement
            if (Math.Abs(mag.x) > Threshold && Math.Abs(inputManager.x) < 0.05f || (mag.x < -Threshold && x > 0) || (mag.x > Threshold && x < 0))
            {
                rb.AddForce(orientation.right * (PlayerManager.PlayerStats.MoveSpeed * Time.deltaTime * -mag.x * counterMovement));
            }
            if (Math.Abs(mag.y) > Threshold && Math.Abs(inputManager.y) < 0.05f || (mag.y < -Threshold && y > 0) || (mag.y > Threshold && y < 0))
            {
                rb.AddForce(orientation.forward * (PlayerManager.PlayerStats.MoveSpeed * Time.deltaTime * -mag.y * counterMovement));
            }
            
            // Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
            if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > PlayerManager.PlayerStats.MaxSpeed)
            {
                var velocity = rb.velocity;
                float fallspeed = velocity.y;
                Vector3 n = velocity.normalized * PlayerManager.PlayerStats.MaxSpeed;
                velocity = new Vector3(n.x, fallspeed, n.z);
                rb.velocity = velocity;
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

        /*Vector3 GetCancelSlopeMovement()
        {
            float angle = Vector3.Angle(Vector3.zero, _slopeHit.normal);
            var f = rb.mass * 30 * (Mathf.Sin(angle) + Mathf.Cos(angle));
            Debug.Log(f);
            var slopeMoveDirection = Vector3.ProjectOnPlane(Vector3.right, _slopeHit.normal).normalized * Vector3.right;
            return new Vector3(slopeMoveDirection.x, slopeMoveDirection.y, slopeMoveDirection.z);
        }*/

        private bool AllowWeaponSway() 
        {
            if (inputManager.x != 0 || inputManager.y != 0) // Is this required??
            {
                if (_grounded)
                    return true;
            }
        
            return false;
        }
    }
}
