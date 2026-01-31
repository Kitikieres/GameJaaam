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

    private bool _isFacingRight;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isFalling;

    private int _numberOfJumpsUsed;

    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _dashDirection;

    private bool _isFused;
    private bool _movementLocked;

    private bool _overrideVertical;
    private float _overrideVerticalValue;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _isFacingRight = true;
    }

    private void Update()
    {
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
        else
        {
            Jump();
            Fall();
        }

        Dash();
        Move();
        ApplyVelocity();
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

            HorizontalVelocity = Mathf.Lerp(
                HorizontalVelocity,
                targetSpeed,
                MoveStats.GroundAcceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            HorizontalVelocity = Mathf.Lerp(
                HorizontalVelocity,
                0f,
                MoveStats.GroundDeceleration * Time.fixedDeltaTime
            );
        }
    }

    private void TurnCheck(float inputX)
    {
        if (_isFacingRight && inputX < 0f) Turn(false);
        else if (!_isFacingRight && inputX > 0f) Turn(true);
    }

    private void Turn(bool right)
    {
        _isFacingRight = right;
        transform.Rotate(0f, 180f, 0f);
    }

    private void JumpChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            if (_isGrounded || (_isFused && _numberOfJumpsUsed < 2))
            {
                _numberOfJumpsUsed++;
                VerticalVelocity = MoveStats.InitialJumpVelocity;
                _isJumping = true;
            }
        }
    }

    private void Jump()
    {
        if (_isJumping)
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
    }

    private void Fall()
    {
        if (!_isGrounded && !_isJumping && !_isDashing)
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
    }

    private void DashCheck()
    {
        if (!_isFused) return;

        if (InputManager.DashWasPressed && !_isDashing)
        {
            _isDashing = true;
            _dashDirection = InputManager.Movement.normalized;
            if (_dashDirection == Vector2.zero)
                _dashDirection = _isFacingRight ? Vector2.right : Vector2.left;

            _dashTimer = MoveStats.DashTime;
        }
    }

    private void Dash()
    {
        if (!_isDashing) return;

        HorizontalVelocity = _dashDirection.x * MoveStats.DashSpeed;
        VerticalVelocity = _dashDirection.y * MoveStats.DashSpeed;

        _dashTimer -= Time.fixedDeltaTime;
        if (_dashTimer <= 0f)
            _isDashing = false;
    }

    private void CollisionChecks()
    {
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

        _rb.linearVelocity = new Vector2(HorizontalVelocity, VerticalVelocity);
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
}
