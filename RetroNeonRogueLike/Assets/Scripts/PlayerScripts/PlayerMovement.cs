using System;
using FMOD.Studio;
using SL.Wait;
using UnityEngine;

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
        public static bool Grounded { get; private set; }

        [Space]
        [SerializeField] float floorDetectionRange;
        [SerializeField] private LayerMask whatIsGround;

        public static float CurrentSpeed { get; private set; }

        private EventInstance _walkEventInstance;

        void Awake()
        {
            _playerScale = playerModel.transform.localScale;
            _crouchScale = new Vector3(_playerScale.x, _playerScale.y / 2f, _playerScale.z);
        }

        private void Start()
        {
            var eventInstance = FModAudioManager.CreateSoundInstance(SoundInstanceType.Walk, "Step", false);
            if (eventInstance != null)
                _walkEventInstance = eventInstance.Value;
        }

        void Update() 
        {
            CurrentSpeed = rb.velocity.magnitude;
            
            Vector2 mag = FindVelRelativeToLook();
            
            //Counteract sliding and sloppy movement
            CounterMovement(InputManagerData.x, InputManagerData.y, mag);
            
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
            _moveDirection = orientation.forward * InputManagerData.y + orientation.right * InputManagerData.x;

            Grounded = Physics.Raycast(orientation.position, Vector3.down, floorDetectionRange, whatIsGround);

            //If sliding down a ramp, add force down so player stays grounded and also builds speed
            if (InputManagerData.Crouching && Grounded && OnSlope())
            {
                rb.AddForce(Vector3.down * (Time.deltaTime * _speed));
                return;
            }
            
            // Some multipliers
            float multiplierHorizontal = 1f, multiplierVertical = 1f, diagonalMultiplier = 1f;

            // Movement in air
            if (!Grounded)
            {
                multiplierHorizontal = 0.5f;
                multiplierVertical = 0.5f;
            }

            // Movement while sliding
            if (Grounded && InputManagerData.Crouching) multiplierVertical = 0f;

            if (InputManagerData.x != 0 && InputManagerData.y != 0)
                diagonalMultiplier = 0.707f;
            
            // Add movement force
            if (OnSlope() && !_exitingSlope)
                rb.AddForce(GetSlopeMoveDirection() * (_speed * Time.deltaTime * multiplierHorizontal * multiplierVertical * diagonalMultiplier));
            else
                rb.AddForce(_moveDirection.normalized * (_speed * Time.deltaTime * multiplierHorizontal * multiplierVertical * diagonalMultiplier));

            // Add sound
            _walkEventInstance.getPlaybackState(out var walkSoundState);
            if (Grounded && CurrentSpeed > 0 && !InputManagerData.Crouching)
            {
                if(walkSoundState == PLAYBACK_STATE.STOPPED)
                    FModAudioManager.StartSoundInstance(SoundInstanceType.Walk);
            }
            else if(walkSoundState == PLAYBACK_STATE.PLAYING)
            {
                FModAudioManager.StopSoundInstance(SoundInstanceType.Walk); 
            }

            if (!PlayerManager.CanMove)
            {
                _speed = 0;
                StopCrouch();
                return;
            }

            //If holding jump && ready to jump, then jump
            if (_readyToJump && InputManagerData.Jumping)
                Jump();

            // Run or walk
            if (InputManagerData.Sprinting && PlayerManager.PlayerStats.CurrentStamina > PlayerManager.PlayerStats.GetStaminaPrice("Sprint") && !InputManagerData.Crouching)
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
            if (Grounded && _readyToJump && PlayerManager.PlayerStats.CurrentStamina > PlayerManager.PlayerStats.GetStaminaPrice("Jump"))
            {
                _readyToJump = false;

                //// Add jump forces ////
                // Add force to Up
                rb.AddForce(Vector2.up * (PlayerManager.PlayerStats.JumpForce 
                                          * (InputManagerData.Crouching ? PlayerManager.PlayerStats.CrouchJumpForceMultiplier : 1 )));
                // Add force to movement direction
                rb.AddForce(_moveDirection
                            * (PlayerManager.PlayerStats.JumpForce 
                               * PlayerManager.PlayerStats.JumpForceForwardMultiplier
                               * (InputManagerData.Crouching ? PlayerManager.PlayerStats.CrouchJumpForceMultiplier : 1 )));

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

        //---- this needs refactor ----//
        private void StartCrouch() 
        {
            collider.height = _crouchScale.y *2f;
            playerModel.transform.localScale = _crouchScale;
        
            if (rb.velocity.magnitude > walkSpeed * minSpeedForSlideMultiplier && Grounded) //// WTF MAGIC NUMBERS
                rb.AddForce(orientation.forward * PlayerManager.PlayerStats.SlideForce);
        
            PlayerManager.WSAB.isCrouching = true;
        }
        private void StopCrouch()
        {
            collider.height = _playerScale.y*2f;
            playerModel.transform.localScale = _playerScale;

            PlayerManager.WSAB.isCrouching = false;
        }
        //---------------------------//
        
        private void CounterMovement(float x, float y, Vector2 mag)
        {
            if (!Grounded || InputManagerData.Jumping) return;

            // Slow down sliding
            if (InputManagerData.Crouching)
            {
                rb.AddForce(-rb.velocity.normalized * (PlayerManager.PlayerStats.MoveSpeed * Time.deltaTime * slideCounterMovement));
                return;
            }
            
            //----------- NOT WORKING :( -----------//
            // Counter unintentional slope sliding
            if (OnSlope() && !InputManagerData.Crouching) 
            {
                rb.AddForce(orientation.forward * (_slopeHit.normal.y * _slopeHit.normal.z * Time.deltaTime * counterMovement));
                rb.AddForce(orientation.right * (_slopeHit.normal.y * _slopeHit.normal.x * Time.deltaTime * counterMovement));
                //rb.AddForce(GetCancelSlopeMovement() * slopeCancelMultiplier );
            }

            // Counter movement
            if (Math.Abs(mag.x) > Threshold && Math.Abs(InputManagerData.x) < 0.05f || (mag.x < -Threshold && x > 0) || (mag.x > Threshold && x < 0))
            {
                rb.AddForce(orientation.right * (PlayerManager.PlayerStats.MoveSpeed * Time.deltaTime * -mag.x * counterMovement));
            }
            if (Math.Abs(mag.y) > Threshold && Math.Abs(InputManagerData.y) < 0.05f || (mag.y < -Threshold && y > 0) || (mag.y > Threshold && y < 0))
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

        Vector3 GetSlopeMoveDirection() => Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal);

        /*Vector3 GetCancelSlopeMovement()
        {
            float angle = Vector3.Angle(Vector3.zero, _slopeHit.normal);
            var f = rb.mass * 30 * (Mathf.Sin(angle) + Mathf.Cos(angle));
            Debug.Log(f);
            var slopeMoveDirection = Vector3.ProjectOnPlane(Vector3.right, _slopeHit.normal).normalized * Vector3.right;
            return new Vector3(slopeMoveDirection.x, slopeMoveDirection.y, slopeMoveDirection.z);
        }*/
    }
}
