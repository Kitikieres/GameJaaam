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
    private bool _bumpedHead;

   
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private int _numberOfJumpsUsed;

    
    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _dashDirection;
    private int _numberOfDashesUsed;

    
    private bool _isFused;

   
    private bool _movementLocked;

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
        Jump();
        Fall();
        Dash();

        if (_isGrounded)
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        else
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        _rb.linearVelocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }

    private void Move(float acceleration, float deceleration, Vector2 input)
    {
        if (_isDashing) return;

        if (Mathf.Abs(input.x) >= MoveStats.MoveThreshold)
        {
            TurnCheck(input);

            float targetSpeed = InputManager.RunIsHeld
                ? input.x * MoveStats.MaxRunSpeed
                : input.x * MoveStats.MaxWalkSpeed;

            HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
        }
    }

    private void TurnCheck(Vector2 input)
    {
        if (_isFacingRight && input.x < 0f) Turn(false);
        else if (!_isFacingRight && input.x > 0f) Turn(true);
    }

    private void Turn(bool right)
    {
        _isFacingRight = right;
        transform.Rotate(0f, 180f, 0f);
    }

    #region Jump
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
    #endregion

    #region Dash
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
            _numberOfDashesUsed++;
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
    #endregion

    #region Collision
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
            _numberOfDashesUsed = 0;
            _isJumping = false;
        }
    }
    #endregion

    // 🔓 LLAMADO DESDE LA MÁSCARA
    public void FuseWithMask()
    {
        _isFused = true;
        Debug.Log("🔥 Máscara completa: dash y doble salto activados");
    }

    // 🔒 USADO POR LA CINEMÁTICA
    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;
        if (locked)
        {
            HorizontalVelocity = 0f;
            VerticalVelocity = 0f;
        }
    }
}
