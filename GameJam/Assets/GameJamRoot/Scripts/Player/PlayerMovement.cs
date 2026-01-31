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
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
        // Initiate jump with jump buffering and coyote time
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            Debug.Log("SALTANDO!"); // DEBUG
            InitiateJump(1);
            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }

        }
        // Double jump
        else if (_jumpBufferTimer > 0f && (_isJumping || _isAirDashing || _isDashFastFalling) && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);

            if (_isDashFastFalling)
            {
                _isDashFastFalling = false;
            }
        }

        // Air Jump after coyote time lapsed
        else if (_jumpBufferTimer > 0f && _isFastFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;

        }

    }
    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
    private void InitiateDash()
    {
        _dashDirection = InputManager.Movement;

        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(_dashDirection, MoveStats.DashDirections[0]);

        for (int i = 0; i < MoveStats.DashDirections.Length; i++)
        {
            // skip if we hit it bang on 
            if (_dashDirection == MoveStats.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }

            float distance = Vector2.Distance(_dashDirection, MoveStats.DashDirections[i]);

            // Check if this is a diagonal direcion and apply bias

            bool isDiagonal = (Mathf.Abs(MoveStats.DashDirections[i].x) == 1 && Mathf.Abs(MoveStats.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= MoveStats.DashDiagonallyBias;
            }

            else if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = MoveStats.DashDirections[i];
            }

            // Handle direction with NO input

            if (closestDirection == Vector2.zero)
            {
                if (_isFacingRight)
                {
                    closestDirection = Vector2.right;
                }
                else
                {
                    closestDirection = Vector2.left;
                }

            }
        }

        _dashDirection = closestDirection;
        _numberOfDashesUsed++;
        _isDashing = true;
        _dashTimer = 0f;
        _dashOnGroundTimer = MoveStats.TimeBtwDashesOnGround;

        ResetJumpValues();
    }

>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
    #region Collision
=======
    #region CollisionChecks

    private void isGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);

        // DEBUG - Quitar después
        Debug.DrawRay(boxCastOrigin, Vector2.down * MoveStats.GroundDetectionRayLength, Color.red);

        if (_groundHit.collider != null)
        {
            _isGrounded = true;
            Debug.Log("TOCANDO SUELO: " + _groundHit.collider.name);
        }
        else
        {
            _isGrounded = false;
        }
    }
    private void Bumpedhead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidht, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }

        #region Debug Visualization

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidht;

            Color rayColor;
            if (_bumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor); ;
        }

        #endregion
    }
>>>>>>> Stashed changes
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
}
