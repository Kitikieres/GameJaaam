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

    // Movement vars
    public float HorizontalVelocity { get; private set; }
    private bool _isFacingRight;
    private bool _isMovementLocked;

    // Mask vars
    private bool _hasMask;
    private float _speedMultiplier = 1f;
    private float _jumpMultiplier = 1f;

    // Collision check vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    // Jump vars
    public float VerticalVelocity { get; private set; }

    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    // Apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    // Jump buffer vars
    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;

    // Coyote time vars
    private float _coyoteTimer;

    // Dash vars
    private bool _isDashing;
    private bool _isAirDashing;
    private float _dashTimer;
    private float _dashOnGroundTimer;
    private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    private bool _isDashFastFalling;
    private float _dashFastFallTime;
    private float _dashFastFallReleaseSpeed;

    private void Awake()
    {
        _isFacingRight = true;
        _hasMask = false;
        _isMovementLocked = false;
        _speedMultiplier = 1f;
        _jumpMultiplier = 1f;

        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CollisionChecks();
        CounterTimers();
        JumpChecks();
        LandCheck();

        if (_hasMask)
        {
            DashCheck();
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        Fall();
        Dash();

        if (_isMovementLocked)
        {
            HorizontalVelocity = 0f;
        }
        else if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }

        ApplyVelocity();
    }

    #region Mask Fusion Methods

    public void SetMovementLocked(bool locked)
    {
        _isMovementLocked = locked;
        if (locked)
        {
            HorizontalVelocity = 0f;
            VerticalVelocity = 0f;
            _rb.linearVelocity = Vector2.zero;
        }
    }

    public void FuseWithMask()
    {
        _hasMask = true;

        if (MoveStats.EnablePostMaskBonuses)
        {
            _speedMultiplier = MoveStats.PostMaskSpeedMultiplier;
            _jumpMultiplier = MoveStats.PostMaskJumpMultiplier;
            Debug.Log($"¡Máscara obtenida! Velocidad x{_speedMultiplier}, Salto x{_jumpMultiplier}");
        }
        else
        {
            Debug.Log("¡Dash desbloqueado!");
        }
    }

    public bool HasMask()
    {
        return _hasMask;
    }

    // NUEVO - Para el trampolín
    public void TrampolineBounce(float bounceForce)
    {
        VerticalVelocity = bounceForce;
        _isJumping = true;
        _isFalling = false;
        _isFastFalling = false;
        _numberOfJumpsUsed = 0;
    }

    #endregion

    private void ApplyVelocity()
    {
        if (!_isDashing)
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        }
        else
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -50f, 50f);
        }
        _rb.linearVelocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }

    private void OnDrawGizmos()
    {
        if (MoveStats == null) return;

        if (MoveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(MoveStats.MaxWalkSpeed, Color.white);
        }
        if (MoveStats.ShowRunJumpArc)
        {
            DrawJumpArc(MoveStats.MaxRunSpeed, Color.red);
        }
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!_isDashing && !_isMovementLocked)
        {
            if (Mathf.Abs(moveInput.x) >= MoveStats.MoveThreshold)
            {
                TurnCheck(moveInput);

                float targetVelocity = 0f;
                if (InputManager.RunIsHeld)
                {
                    targetVelocity = moveInput.x * MoveStats.MaxRunSpeed * _speedMultiplier;
                }
                else
                {
                    targetVelocity = moveInput.x * MoveStats.MaxWalkSpeed * _speedMultiplier;
                }

                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(moveInput.x) < MoveStats.MoveThreshold)
            {
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
            }
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0f)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0f)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    #endregion

    private void LandCheck()
    {
        if ((_isJumping || _isFalling || _isDashFastFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            ResetJumpValues();
            ResetDashes();

            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;

            if (_isDashFastFalling && _isGrounded)
            {
                ResetDashValues();
                return;
            }

            ResetDashValues();
        }
    }

    private void Fall()
    {
        if (!_isGrounded && !_isJumping && !_isDashing && !_isDashFastFalling)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }
    }

    #region Jump

    private void ResetJumpValues()
    {
        _isJumping = false;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
    }

    private void JumpChecks()
    {
        if (_isMovementLocked) return;

        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleaseDuringBuffer = false;

            if (MoveStats.DebugShowJumpInfo)
            {
                Debug.Log($"Jump pressed! Grounded: {_isGrounded} | Coyote: {_coyoteTimer:F3}");
            }
        }

        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleaseDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // Salto normal con buffer y coyote time
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump();
            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
    }

    private void InitiateJump()
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f; // Resetear coyote time al saltar
        _numberOfJumpsUsed = 1;
        VerticalVelocity = MoveStats.InitialJumpVelocity * _jumpMultiplier;

        if (MoveStats.DebugShowJumpInfo)
        {
            Debug.Log($"JUMP! Velocity: {VerticalVelocity:F2}");
        }
    }

    private void Jump()
    {
        if (_isJumping)
        {
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }
        }

        if (VerticalVelocity >= 0f)
        {
            _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity * _jumpMultiplier, 0f, VerticalVelocity);

            if (_apexPoint > MoveStats.ApexThreshold)
            {
                if (!_isPastApexThreshold)
                {
                    _isPastApexThreshold = true;
                    _timePastApexThreshold = 0f;
                }

                if (_isPastApexThreshold)
                {
                    _timePastApexThreshold += Time.fixedDeltaTime;
                    if (_timePastApexThreshold < MoveStats.ApexHangTime)
                    {
                        VerticalVelocity = 0f;
                    }
                    else
                    {
                        VerticalVelocity = -0.01f;
                    }
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                }
            }
        }
        else if (!_isFastFalling)
        {
            VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
        }
        else if (VerticalVelocity < 0f)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }
        }

        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Dash

    private void DashCheck()
    {
        if (_isMovementLocked || !_hasMask) return;

        if (InputManager.DashWasPressed)
        {
            if (_isGrounded && _dashOnGroundTimer < 0 && !_isDashing)
            {
                InitiateDash();
            }
            else if (!_isGrounded && !_isDashing && _numberOfDashesUsed < MoveStats.NumberOfDashes)
            {
                _isAirDashing = true;
                InitiateDash();
            }
        }
    }

    private void InitiateDash()
    {
        _dashDirection = InputManager.Movement;

        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(_dashDirection, MoveStats.DashDirections[0]);

        for (int i = 0; i < MoveStats.DashDirections.Length; i++)
        {
            if (_dashDirection == MoveStats.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }

            float distance = Vector2.Distance(_dashDirection, MoveStats.DashDirections[i]);

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

    private void Dash()
    {
        if (_isDashing)
        {
            _dashTimer += Time.fixedDeltaTime;
            if (_dashTimer >= MoveStats.DashTime)
            {
                if (_isGrounded)
                {
                    ResetDashes();
                }

                _isAirDashing = false;
                _isDashing = false;

                if (!_isJumping)
                {
                    _dashFastFallTime = 0f;
                    _dashFastFallReleaseSpeed = VerticalVelocity;

                    if (!_isGrounded)
                    {
                        _isDashFastFalling = true;
                    }
                }

                return;
            }

            HorizontalVelocity = MoveStats.DashSpeed * _dashDirection.x;

            if (_dashDirection.y != 0f || _isAirDashing)
            {
                VerticalVelocity = MoveStats.DashSpeed * _dashDirection.y;
            }
        }
        else if (_isDashFastFalling)
        {
            if (VerticalVelocity > 0f)
            {
                if (_dashFastFallTime < MoveStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity = Mathf.Lerp(_dashFastFallReleaseSpeed, 0f, (_dashFastFallTime / MoveStats.DashTimeForUpwardsCancel));
                }
                else if (_dashFastFallTime >= MoveStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity += MoveStats.Gravity * MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
        }
    }

    private void ResetDashValues()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }

    private void ResetDashes()
    {
        _numberOfDashesUsed = 0;
    }

    #endregion

    #region CollisionChecks

    private void isGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);

        // Debug visual
        if (MoveStats.DebugShowIsGroundedBox)
        {
            Color debugColor = _groundHit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(boxCastOrigin + Vector2.left * boxCastSize.x / 2, Vector2.down * MoveStats.GroundDetectionRayLength, debugColor);
            Debug.DrawRay(boxCastOrigin + Vector2.right * boxCastSize.x / 2, Vector2.down * MoveStats.GroundDetectionRayLength, debugColor);
        }

        if (_groundHit.collider != null)
        {
            _isGrounded = true;
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

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidht;
            Color rayColor = _bumpedHead ? Color.green : Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }
    }

    private void CollisionChecks()
    {
        isGrounded();
        Bumpedhead();
    }

    #endregion

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        if (_feetColl == null) return;

        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = MoveStats.DrawRight ? moveSpeed : -moveSpeed;
        Vector2 velocity = new Vector2(speed, MoveStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcResolution;

        for (int i = 0; i < MoveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < MoveStats.TimeTillJumpApex)
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0, MoveStats.Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime)
            {
                float apexTime = simulationTime - MoveStats.TimeTillJumpApex;
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime;
            }
            else
            {
                float descendTime = simulationTime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * MoveStats.ApexHangTime;
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, MoveStats.Gravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), MoveStats.GroundLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    #region Timers

    private void CounterTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (_isGrounded)
        {
            _coyoteTimer = MoveStats.JumpCoyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        if (_isGrounded)
        {
            _dashOnGroundTimer -= Time.deltaTime;
        }
    }
    #endregion


}