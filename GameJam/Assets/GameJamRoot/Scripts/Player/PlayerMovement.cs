using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    public float HorizontalVelocity { get; private set; }
    public float VerticalVelocity { get; private set; }
    public bool IsFacingRight => _isFacingRight;
    public bool IsGrounded => _isGrounded;
    public bool IsDashing => _isDashing;
    public bool IsFused => _isFused;

    private bool _isFacingRight;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isFalling;

    private int _numberOfJumpsUsed;

    // COYOTE TIME
    private float _coyoteTimeCounter;
    private bool _wasGroundedLastFrame;

    // JUMP BUFFER
    private float _jumpBufferCounter;

    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _dashDirection;

    private bool _isFused;
    private bool _movementLocked;

    private bool _overrideVertical;
    private float _overrideVerticalValue;

    private Vector2 _currentVelocity;
    private Vector3 _scale;

    // FIX: Flag para saber si acabamos de saltar este frame
    private bool _justJumped;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _isFacingRight = true;
        _scale = transform.localScale;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        if (_movementLocked) return;

        UpdateTimers();
        JumpChecks();
        DashCheck();
    }

    private void FixedUpdate()
    {
        if (_movementLocked)
        {
            ApplyVelocity();
            return;
        }

        CollisionChecks();

        if (_overrideVertical)
        {
            VerticalVelocity = _overrideVerticalValue;
            _overrideVertical = false;
        }
        else if (!_justJumped) // FIX: NO aplicar gravedad el frame que saltamos
        {
            ApplyGravity();
        }

        // Reset del flag de salto
        _justJumped = false;

        Dash();
        Move();
        ApplyVelocity();
    }

    private void UpdateTimers()
    {
        if (_isGrounded)
        {
            _coyoteTimeCounter = MoveStats.JumpCoyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (InputManager.JumpWasPressed)
        {
            _jumpBufferCounter = MoveStats.JumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void Move()
    {
        if (_isDashing) return;

        float inputX = InputManager.Movement.x;

        if (Mathf.Abs(inputX) > MoveStats.MoveThreshold)
        {
            TurnCheck(inputX);

            float targetSpeed = InputManager.RunIsHeld
                ? inputX * MoveStats.MaxRunSpeed
                : inputX * MoveStats.MaxWalkSpeed;

            float acceleration = _isGrounded
                ? MoveStats.GroundAcceleration
                : MoveStats.AirAcceleration;

            HorizontalVelocity = Mathf.Lerp(
                HorizontalVelocity,
                targetSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            float deceleration = _isGrounded
                ? MoveStats.GroundDeceleration
                : MoveStats.AirDeceleration;

            HorizontalVelocity = Mathf.Lerp(
                HorizontalVelocity,
                0f,
                deceleration * Time.fixedDeltaTime
            );

            if (Mathf.Abs(HorizontalVelocity) < 0.01f)
            {
                HorizontalVelocity = 0f;
            }
        }
    }

    private void TurnCheck(float inputX)
    {
        if (_isFacingRight && inputX < 0f)
            Turn(false);
        else if (!_isFacingRight && inputX > 0f)
            Turn(true);
    }

    private void Turn(bool right)
    {
        _isFacingRight = right;
        _scale.x = right ? Mathf.Abs(_scale.x) : -Mathf.Abs(_scale.x);
        transform.localScale = _scale;
    }

    private void JumpChecks()
    {
        bool wantsToJump = _jumpBufferCounter > 0f;
        if (!wantsToJump) return;

        bool canJumpFromGround = _isGrounded;
        bool canCoyoteJump = _coyoteTimeCounter > 0f && _numberOfJumpsUsed == 0;
        bool canDoubleJump = _isFused && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed;

        if (canJumpFromGround || canCoyoteJump || canDoubleJump)
        {
            _numberOfJumpsUsed++;
            VerticalVelocity = MoveStats.InitialJumpVelocity;
            _isJumping = true;
            _isFalling = false;
            _justJumped = true; // FIX: Marcar que acabamos de saltar

            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
        }
    }

    private void ApplyGravity()
    {
        if (_isGrounded && !_isJumping)
        {
            VerticalVelocity = 0f;
            return;
        }

        if (_isJumping || !_isGrounded)
        {
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        if (VerticalVelocity < 0f && _isJumping)
        {
            _isJumping = false;
            _isFalling = true;
        }
    }

    private void DashCheck()
    {
        if (!_isFused || _isDashing) return;

        if (InputManager.DashWasPressed)
        {
            _isDashing = true;

            _dashDirection = InputManager.Movement.normalized;
            if (_dashDirection == Vector2.zero)
                _dashDirection = _isFacingRight ? Vector2.right : Vector2.left;

            _dashTimer = MoveStats.DashTime;

            _isJumping = false;
            _isFalling = false;
        }
    }

    private void Dash()
    {
        if (!_isDashing) return;

        HorizontalVelocity = _dashDirection.x * MoveStats.DashSpeed;
        VerticalVelocity = _dashDirection.y * MoveStats.DashSpeed;

        _dashTimer -= Time.fixedDeltaTime;

        if (_dashTimer <= 0f)
        {
            _isDashing = false;
            HorizontalVelocity *= 0.5f;
            VerticalVelocity *= 0.5f;
        }
    }

    private void CollisionChecks()
    {
        _wasGroundedLastFrame = _isGrounded;

        _isGrounded = Physics2D.OverlapBox(
            _feetColl.bounds.center,
            _feetColl.bounds.size,
            0f,
            MoveStats.GroundLayer
        );

        if (_isGrounded)
        {
            _numberOfJumpsUsed = 0;
            _isJumping = false;
            _isFalling = false;
        }
    }

    private void ApplyVelocity()
    {
        VerticalVelocity = Mathf.Clamp(
            VerticalVelocity,
            -MoveStats.MaxFallSpeed,
            50f
        );

        _currentVelocity.x = HorizontalVelocity;
        _currentVelocity.y = VerticalVelocity;
        _rb.linearVelocity = _currentVelocity;
    }

    public void FuseWithMask()
    {
        _isFused = true;
    }

    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;
        if (locked)
        {
            HorizontalVelocity = 0f;
            VerticalVelocity = 0f;
        }
    }

    public void TrampolineBounce(float velocity)
    {
        _overrideVertical = true;
        _overrideVerticalValue = velocity;
        _isJumping = true;
        _isFalling = false;
        _numberOfJumpsUsed = 0;
    }

    public void ResetJumps()
    {
        _numberOfJumpsUsed = 0;
    }

    public void CancelDash()
    {
        _isDashing = false;
        _dashTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (_feetColl == null) return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(_feetColl.bounds.center, _feetColl.bounds.size);
    }
}