using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region References

    [Header("References")]
    public PlayerMovementStats MovementStats;
    public PlayerStats Stats;

    [SerializeField] private Collider2D bodyColl;
    [SerializeField] private Collider2D feetColl;
    [SerializeField] private bool debugJumpLogic = true;
    [SerializeField] private bool debugGroundState = true;

    #endregion

    #region Camera Stuff

    [Header("Camera Stuff")]
    [SerializeField] private GameObject cameraFollowGO;

    private CameraFollowObject cameraFollowObject;

    private float fallSpeedYDampingChangeThreshold;

    #endregion

    #region Components

    private Rigidbody2D rb;

    #endregion

    #region Horizontal Movement State

    public bool IsFacingRight;

    private Vector2 moveVelocity;

    #endregion

    #region Vertical Movement State

    public float VerticalVelocity { get; private set; }

    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;

    private float fastFallTime;
    private float fastFallReleaseSpeed;

    private int jumpsUsed;
    private bool hasJumpedSinceLastGrounded;

    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApex;

    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    private float coyoteTimer;

    #endregion

    #region Collision State

    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;

    private bool isGrounded;
    private bool bumpedHead;

    #endregion

    #region Unity Methods

    private void Start()
    {
        IsFacingRight = true;

        if (MovementStats == null) MovementStats = GetComponent<PlayerMovementStats>();
        if (Stats == null) Stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();

        cameraFollowObject = cameraFollowGO.GetComponent<CameraFollowObject>();

        fallSpeedYDampingChangeThreshold = CameraManager.Instance.FallSpeedYDampingChangeThreshold;
    }

    private void Update()
    {
        CountTimers();
        HandleJumpInput();

        if (isFalling)
            CameraManager.Instance.LerpYDamping(isFalling);
        else
        {
            CameraManager.Instance.SetLerpedFromPlayerFalling(false);
            CameraManager.Instance.LerpYDamping(isFalling);
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        HandleHorizontalMovement();
        HandleLanding();
        ApplyGravity();
        ApplyVerticalMovement();
    }

    #endregion

    #region Horizontal Movement

    private void HandleHorizontalMovement()
    {
        if (isGrounded)
        {
            Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MovementStats.AirAcceleration, MovementStats.AirDeceleration, InputManager.Movement);
        }
    }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * MovementStats.MaxWalkSpeed;
            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (IsFacingRight && moveInput.x < 0f)
        {
            Turn(false);
        }
        else if (!IsFacingRight && moveInput.x > 0f)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        IsFacingRight = turnRight;

        if (turnRight)
        {
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            transform.Rotate(0f, -180f, 0f);
        }
        cameraFollowObject.CallTurn();
    }

    #endregion

    #region Jump Input And Rules

    private void HandleJumpInput()
    {
        HandleJumpPress();
        HandleJumpRelease();
        TryStartJump();
    }

    private void HandleJumpPress()
    {
        if (!InputManager.JumpWasPressed) return;

        jumpBufferTimer = MovementStats.JumpBufferTime;
        jumpReleasedDuringBuffer = false;
    }

    private void HandleJumpRelease()
    {
        if (!InputManager.JumpWasReleased) return;

        if (jumpBufferTimer > 0f)
        {
            jumpReleasedDuringBuffer = true;
        }

        if (isJumping && VerticalVelocity > 0f)
        {
            isFastFalling = true;
            fastFallReleaseSpeed = VerticalVelocity;
            fastFallTime = 0f;

            isPastApex = false;
            timePastApexThreshold = 0f;
        }
    }

    private void TryStartJump()
    {
        if (CanGroundJump())
        {
            StartJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
                fastFallTime = 0f;
            }

            return;
        }

        if (CanAirJump())
        {
            StartJump(1);
        }
    }

    private bool CanGroundJump()
    {
        return jumpBufferTimer > 0f
            && !isJumping
            && (isGrounded || coyoteTimer > 0f);
    }

    private bool CanAirJump()
    {
        return jumpBufferTimer > 0f
            && isFalling
            && coyoteTimer <= 0f
            && hasJumpedSinceLastGrounded
            && jumpsUsed < Stats.MaxNumberOfJumps;
    }

    private void StartJump(int numberOfJumps)
    {
        isJumping = true;
        isFalling = false;
        isFastFalling = false;

        isPastApex = false;
        timePastApexThreshold = 0f;

        jumpBufferTimer = 0f;
        jumpReleasedDuringBuffer = false;

        jumpsUsed += numberOfJumps;
        hasJumpedSinceLastGrounded = true;

        VerticalVelocity = MovementStats.InitialJumpVelocity;
    }

    #endregion

    #region Landing

    private void HandleLanding()
    {
        if (!isGrounded || VerticalVelocity > 0f) return;

        if (isJumping || isFalling)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;

            fastFallTime = 0f;
            fastFallReleaseSpeed = 0f;

            isPastApex = false;
            timePastApexThreshold = 0f;

            jumpsUsed = 0;
            hasJumpedSinceLastGrounded = false;
        }

        VerticalVelocity = -0.01f;
    }

    #endregion

    #region Gravity

    private void ApplyGravity()
    {
        if (isGrounded && VerticalVelocity <= 0f)
        {
            VerticalVelocity = -0.01f;
            return;
        }

        if (bumpedHead && VerticalVelocity > 0f)
        {
            isFastFalling = true;
            fastFallReleaseSpeed = 0f;
            fastFallTime = MovementStats.TimeForUpwardsCancel;
        }

        UpdateApexState();

        if (ShouldApplyApexHang())
        {
            VerticalVelocity = 0f;
            timePastApexThreshold += Time.fixedDeltaTime;
            return;
        }

        if (isFastFalling && VerticalVelocity > 0f)
        {
            ApplyJumpCut();
        }
        else if (VerticalVelocity > 0f)
        {
            ApplyRiseGravity();
        }
        else
        {
            ApplyFallGravity();
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MovementStats.MaxFallSpeed, 50f);
    }

    private void ApplyRiseGravity()
    {
        isJumping = true;
        isFalling = false;

        VerticalVelocity += MovementStats.Gravity * Time.fixedDeltaTime;
    }

    private void ApplyJumpCut()
    {
        if (fastFallTime < MovementStats.TimeForUpwardsCancel)
        {
            VerticalVelocity = Mathf.Lerp(
                fastFallReleaseSpeed,
                0f,
                fastFallTime / MovementStats.TimeForUpwardsCancel
            );

            fastFallTime += Time.fixedDeltaTime;
            return;
        }

        ApplyFallGravity();
    }

    private void ApplyFallGravity()
    {
        isJumping = false;
        isFalling = true;
        isPastApex = false;

        float gravityMultiplier = isFastFalling ? MovementStats.GravityOnReleaseMultiplier : 1f;
        VerticalVelocity += MovementStats.Gravity * gravityMultiplier * Time.fixedDeltaTime;
    }

    private void UpdateApexState()
    {
        if (VerticalVelocity <= 0f || isFastFalling)
        {
            isPastApex = false;
            return;
        }

        apexPoint = Mathf.InverseLerp(MovementStats.InitialJumpVelocity, 0f, VerticalVelocity);

        if (apexPoint >= MovementStats.ApexThreshold)
        {
            if (!isPastApex)
            {
                isPastApex = true;
                timePastApexThreshold = 0f;
            }
        }
        else
        {
            isPastApex = false;
            timePastApexThreshold = 0f;
        }
    }

    private bool ShouldApplyApexHang()
    {
        return isPastApex && timePastApexThreshold < MovementStats.ApexHangTime;
    }

    private void ApplyVerticalMovement()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision Checks

    private void CollisionChecks()
    {
        GroundCheck();
        HeadCheck();
    }

    private void GroundCheck()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x, MovementStats.GroundDetectionLength);

        groundHit = Physics2D.BoxCast(
            boxCastOrigin,
            boxCastSize,
            0f,
            Vector2.down,
            MovementStats.GroundDetectionLength,
            MovementStats.GroundLayer
        );

        isGrounded = groundHit.collider != null;

        if (MovementStats.DebugShowIsGrounded)
        {
            Color rayColor = isGrounded ? Color.green : Color.red;

            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2f, boxCastOrigin.y),
                Vector2.down * MovementStats.GroundDetectionLength,
                rayColor
            );

            Debug.DrawRay(
                new Vector2(boxCastOrigin.x + boxCastSize.x / 2f, boxCastOrigin.y),
                Vector2.down * MovementStats.GroundDetectionLength,
                rayColor
            );

            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2f, boxCastOrigin.y - MovementStats.GroundDetectionLength),
                Vector2.right * boxCastSize.x,
                rayColor
            );
        }
    }

    private void HeadCheck()
    {
        Vector2 boxCastOrigin = new Vector2(bodyColl.bounds.center.x, bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(bodyColl.bounds.size.x * MovementStats.HeadWidth, MovementStats.HeadDetectionLength);

        headHit = Physics2D.BoxCast(
            boxCastOrigin,
            boxCastSize,
            0f,
            Vector2.up,
            MovementStats.HeadDetectionLength,
            MovementStats.GroundLayer
        );

        bumpedHead = headHit.collider != null;

        if (MovementStats.DebugShowBumpedHead)
        {
            Color rayColor = bumpedHead ? Color.green : Color.red;

            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2f, boxCastOrigin.y),
                Vector2.up * MovementStats.HeadDetectionLength,
                rayColor
            );

            Debug.DrawRay(
                new Vector2(boxCastOrigin.x + boxCastSize.x / 2f, boxCastOrigin.y),
                Vector2.up * MovementStats.HeadDetectionLength,
                rayColor
            );

            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2f, boxCastOrigin.y + MovementStats.HeadDetectionLength),
                Vector2.right * boxCastSize.x,
                rayColor
            );
        }
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        jumpBufferTimer -= Time.deltaTime;
        if (jumpBufferTimer < 0f) jumpBufferTimer = 0f;

        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
            if (coyoteTimer < 0f) coyoteTimer = 0f;
        }
        else
        {
            coyoteTimer = MovementStats.JumpCoyoteTime;
        }
    }

    #endregion

    #region Debug

    private void LogJumpSnapshot(string label)
    {
        if (!debugJumpLogic) return;

        Debug.Log(
            $"[{label}] " +
            $"isGrounded={isGrounded} | " +
            $"coyoteTimer={coyoteTimer:F3} | " +
            $"jumpBufferTimer={jumpBufferTimer:F3} | " +
            $"isJumping={isJumping} | " +
            $"isFalling={isFalling} | " +
            $"hasJumpedSinceLastGrounded={hasJumpedSinceLastGrounded} | " +
            $"jumpsUsed={jumpsUsed} | " +
            $"maxJumps={Stats.MaxNumberOfJumps} | " +
            $"verticalVelocity={VerticalVelocity:F3}"
        );
    }

    private void LogGroundChange(bool newGroundedState)
    {
        if (!debugGroundState) return;

        Debug.Log(
            $"[GROUND STATE CHANGED] isGrounded={newGroundedState} | " +
            $"coyoteTimer={coyoteTimer:F3} | " +
            $"verticalVelocity={VerticalVelocity:F3}"
        );
    }

    #endregion
}