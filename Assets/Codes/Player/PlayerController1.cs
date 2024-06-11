using System;
using UnityEngine;
using System.Collections;

namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        [SerializeField] private GameObject WinPoint;
        public bool PlayerWin = false;
        private bool isDashing = false;
        private float lastDashTime;
        [SerializeField] private TrailRenderer tr;
        private bool isFacingRight = true;
        [SerializeField] public bool canDoubleJump = false;
        private Timer timer;
        private PowerUps powerUps;
        float originalMaxSpeed;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;

        private void Start()
        {
            originalMaxSpeed = _stats.MaxSpeed;
        }
        public void PowerUpsSpeed(float newSpeed)
        {
            StartCoroutine(TemporarilyIncreaseSpeed(5f, 3f));
        }

        private IEnumerator TemporarilyIncreaseSpeed(float newSpeed, float duration)
        {
            float originalMaxSpeed = _stats.MaxSpeed;
            _stats.MaxSpeed += newSpeed;
            Debug.Log("MaxSpeed temporariamente alterado para: " + newSpeed);


            yield return new WaitForSeconds(duration);

            _stats.MaxSpeed = originalMaxSpeed;
            Debug.Log("MaxSpeed restaurado para: " + originalMaxSpeed);
        }


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            tr = GetComponent<TrailRenderer>();
            timer = FindObjectOfType<Timer>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (!PlayerWin)
            {
                _stats.MaxSpeed = 10;
                _stats.JumpPower = 25;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.CompareTag("WinPoint"))
            {
                PlayerWin = true;
                print("col");
            }
            {
                if (col.gameObject.CompareTag("DoubleJump"))
                {
                    canDoubleJump = true;

                }
            }
        }

        private void WinGame()
        {
            if (PlayerWin)
            {
                print("playerwin");
                _stats.MaxSpeed = 0;
                _stats.JumpPower = 0;
                
                if (timer != null)
                {
                    timer.IsGamePaused = true;
                }
            }
        }




        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
            WinGame();
            ApplyMoviement();
            HandleDashing();
            Flip();
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                _stats.MaxSpeed = 10;
                _stats.MaxFallSpeed = 60;
                _stats.JumpPower = 10;
            }

            print(_stats.MaxSpeed);
            if(_grounded) 
            {
                canDoubleJump = false;
            }
        }

        private void Flip()
        {
            if (_frameInput.Move.x != 0)
            {
                bool movingRight = _frameInput.Move.x > 0;
                if (movingRight != isFacingRight)
                {
                    isFacingRight = movingRight;
                    Vector3 localScale = transform.localScale;
                    localScale.x *= -1f;
                    transform.localScale = localScale;
                }
            }
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void HandleDashing()
        {
            if (Time.time - lastDashTime < _stats.DashCooldown || isDashing)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift)) // Using Left Shift for dash
            {
                StartCoroutine(Dash());
            }
        }

        private IEnumerator Dash()
        {
            isDashing = true;
            lastDashTime = Time.time;

            // Activate TrailRenderer
            if (tr != null)
            {
                tr.emitting = true;
            }

            // Store the original values
            float originalFallAcceleration = _stats.FallAcceleration;
            float originalJumpPower = _stats.JumpPower;
            float originalJumpEndEarlyGravityModifier = _stats.JumpEndEarlyGravityModifier;
            float originalJumpAscentGravityModifier = _stats.JumpAscentGravityModifier;
            float originalMaxFallSpeed = _stats.MaxFallSpeed;

            // Set the vertical movement values to zero
            _stats.FallAcceleration = 0;
            _stats.JumpPower = 0;
            _stats.JumpEndEarlyGravityModifier = 0;
            _stats.JumpAscentGravityModifier = 0;
            _stats.MaxFallSpeed = 0;

            // Set the player's vertical velocity to zero
            _rb.velocity = new Vector2(0, 0);

            // Store the player's initial Y position
            float initialYPosition = transform.position.y;

            // Set the dash speed and direction
            float dashDirection = isFacingRight ? 1 : -1;
            _frameVelocity.x = _stats.DashPower * dashDirection;

            float dashEndTime = Time.time + _stats.DashDuration;
            while (Time.time < dashEndTime)
            {
                // Maintain the player's Y position
                transform.position = new Vector3(transform.position.x, initialYPosition, transform.position.z);
                yield return null;
            }

            // Restore the original values
            _stats.FallAcceleration = originalFallAcceleration;
            _stats.JumpPower = originalJumpPower;
            _stats.JumpEndEarlyGravityModifier = originalJumpEndEarlyGravityModifier;
            _stats.JumpAscentGravityModifier = originalJumpAscentGravityModifier;
            _stats.MaxFallSpeed = originalMaxFallSpeed;

            // Deactivate TrailRenderer
            if (tr != null)
            {
                tr.emitting = false;
            }

            isDashing = false;
        }

        private void ApplyMoviement()
        {
            if (!isDashing)
            {
                _rb.velocity = _frameVelocity;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            HandleJump();
            HandleDirection();
            HandleGravity();
            ApplyMovement();
        }

        #region Collisions

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion

        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;
        private bool _hasDoubleJumped; // flag to check if double jump has been used

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote || (canDoubleJump && !_hasDoubleJumped)) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;

            if (!_grounded && !CanUseCoyote)
            {
                _hasDoubleJumped = true; // set double jump flag
                canDoubleJump = false; // disable further double jumps
            }

            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
                _hasDoubleJumped = false; // reset double jump when grounded
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;

                // Apply gravity modification during ascent
                if (_frameVelocity.y > 0)
                {
                    inAirGravity *= _stats.JumpAscentGravityModifier;
                }

                if (_endedJumpEarly && _frameVelocity.y > 0)
                {
                    inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                }

                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }

#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}