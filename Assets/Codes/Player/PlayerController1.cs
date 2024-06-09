using System;
using UnityEngine;
using System.Collections;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

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
        bool PlayerWin = false;
        private bool isDashing = false;
        private float lastDashTime;
        [SerializeField] TrailRenderer tr;
        private bool isFacingRight = true;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;

        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            tr = GetComponent<TrailRenderer>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (PlayerWin == false)
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

        }
        private void WinGame()
        {
            if (PlayerWin == true)
            {
                print("playerwin");
                _stats.MaxSpeed = 0;
                _stats.JumpPower = 0;
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
        }
        private void Flip()
        {
            {
                // Only flip if there is horizontal movement
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
            if (isFacingRight == false)
            {
                print("fli");
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

            // Ativa o TrailRenderer
            if (tr != null)
            {
                tr.emitting = true;
            }

            // Armazena o valor original da gravidade e das outras propriedades
            float originalFallAcceleration = _stats.FallAcceleration;
            float originalJumpPower = _stats.JumpPower;
            float originalJumpEndEarlyGravityModifier = _stats.JumpEndEarlyGravityModifier;
            float originalJumpAscentGravityModifier = _stats.JumpAscentGravityModifier;
            float originalMaxFallSpeed = _stats.MaxFallSpeed;

            // Zera a gravidade e outras propriedades relacionadas ao movimento vertical
            _stats.FallAcceleration = 0;
            _stats.JumpPower = 0;
            _stats.JumpEndEarlyGravityModifier = 0;
            _stats.JumpAscentGravityModifier = 0;
            _stats.MaxFallSpeed = 0;

            // Zera a velocidade vertical do jogador
            _rb.velocity = new Vector2(0, 0);

            // Armazena a posição vertical atual do jogador
            float initialYPosition = transform.position.y;

            // Define a velocidade do dash
            float dashDirection = isFacingRight ? 1 : -1;
            _frameVelocity.x = _stats.DashPower * dashDirection;

            float dashEndTime = Time.time + _stats.DashDuration;
            while (Time.time < dashEndTime)
            {
                // Mantém a posição y do jogador constante
                transform.position = new Vector3(transform.position.x, initialYPosition, transform.position.z);
                yield return null;
            }

            // Restaura o valor original das propriedades
            _stats.FallAcceleration = originalFallAcceleration;
            _stats.JumpPower = originalJumpPower;
            _stats.JumpEndEarlyGravityModifier = originalJumpEndEarlyGravityModifier;
            _stats.JumpAscentGravityModifier = originalJumpAscentGravityModifier;
            _stats.MaxFallSpeed = originalMaxFallSpeed;

            // Desativa o TrailRenderer
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

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
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
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;

                // Aplica a modificação de gravidade durante a subida
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