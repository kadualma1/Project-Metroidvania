using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region References

    [Header("References")]
    public PlayerMovementStats MovementStats;
    public PlayerStats Stats;
    [SerializeField] private Collider2D bodyColl;
    [SerializeField] private Collider2D feetColl;

    #endregion

    #region Components

    private Rigidbody2D rb;

    #endregion

    #region Movement Vars

    private Vector2 moveVelocity;
    private bool isFacingRight;

    #endregion

    #region Jump Vars

    public float VerticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed; 
    private int jumpsUsed;

    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApex;

    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    private float coyoteTimer;

    #endregion

    #region Collision Vars

    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private bool isGrounded;
    private bool bumpedHead;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        isFacingRight = true;

        if (MovementStats == null) MovementStats = GetComponent<PlayerMovementStats>();
        if (Stats == null) Stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        CollisionChecks();

        if (isGrounded)
        {
            Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MovementStats.AirAcceleration, MovementStats.AirDeceleration, InputManager.Movement);
        }

        HandleLanding();
        ApplyGravity();
        ApplyVerticalMovement();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    #endregion

    #region Movement

    private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0, 180, 0);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0, -180, 0);
        }
    }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            targetVelocity = new Vector2(moveInput.x, 0f) * MovementStats.MaxWalkSpeed;

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }
    }

    #endregion

    #region Jump & Gravity

    private void JumpChecks()
    {
        #region On Jump press...
        if (InputManager.JumpWasPressed)
        {
            jumpBufferTimer = MovementStats.JumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }
        #endregion

        #region On Jump release...
        if (InputManager.JumpWasReleased)
        {
            if (jumpBufferTimer > 0)
            {
                jumpReleasedDuringBuffer = true;
            }

            if (isJumping && VerticalVelocity > 0)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
                fastFallTime = 0f;
                isPastApex = false;
                timePastApexThreshold = 0f;

                //if (isPastApex)
                //{
                //    isPastApex = false;
                //    isFastFalling = true;
                //    fastFallTime = MovementStats.TimeForUpwardsCancel;
                //    VerticalVelocity = 0;
                //}
                //else
                //{
                //    isFastFalling = true;
                //    fastFallReleaseSpeed = VerticalVelocity;
                //}
            }
        }
        #endregion

        #region Jump initiation...

        if (CanGroundJump())
        {
            InitiateJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
                fastFallTime = 0f;
            }
        }

        //if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        //{
        //    InitiateJump(1);

        //    if (jumpReleasedDuringBuffer)
        //    {
        //        isFastFalling = true;
        //        fastFallReleaseSpeed = VerticalVelocity;
        //    }
        //}

        #endregion

        #region Multi-Jump...

        else if (CanExtraJump())
        {
            InitiateJump(1);
        }

        #endregion

        #region Air Jump after Coyote Time...

        else if (CanLateAirJump())
        {
            InitiateJump(2);
        }

        #endregion
    }

    private bool CanGroundJump()
    {
        return jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f);
    }

    private bool CanExtraJump()
    {
        return jumpBufferTimer > 0f && isJumping && jumpsUsed < Stats.MaxNumberOfJumps;
    }

    private bool CanLateAirJump()
    {
        return jumpBufferTimer > 0f && isFalling && coyoteTimer <= 0f && jumpsUsed < Stats.MaxNumberOfJumps;
    }

    private bool ShouldApplyApexHang()
    {
        return isPastApex && timePastApexThreshold < MovementStats.ApexHangTime;
    }

    private void InitiateJump(int numberOfJumps)
    {
        Debug.Log($"INIT JUMP | InitialJumpVelocity: {MovementStats.InitialJumpVelocity}");

        isJumping = true;
        isFalling = false;
        isFastFalling = false;

        isPastApex = false;
        timePastApexThreshold = 0;
        jumpBufferTimer = 0f;

        jumpReleasedDuringBuffer = false;

        jumpsUsed += numberOfJumps;

        VerticalVelocity = MovementStats.InitialJumpVelocity;
    }

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
        }

        VerticalVelocity = -0.01f;
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

    private void ApplyGravity()
    {
        if (isGrounded && VerticalVelocity <= 0)
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

    private void ApplyJumpCut()
    {
        if (fastFallTime < MovementStats.TimeForUpwardsCancel)
        {
            VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, fastFallTime / MovementStats.TimeForUpwardsCancel);
            fastFallTime += Time.fixedDeltaTime;
            return;
        }

        ApplyFallGravity();
    }

    private void ApplyRiseGravity()
    {
        isJumping = true;
        isFalling = false;

        VerticalVelocity += MovementStats.Gravity * Time.fixedDeltaTime;
    }

    private void ApplyFallGravity()
    {
        isJumping = false;
        isFalling = true;
        isPastApex = false;

        float gravityMultiplier = isFastFalling ? MovementStats.GravityOnReleaseMultiplier : 1f;

        VerticalVelocity += MovementStats.Gravity * gravityMultiplier * Time.fixedDeltaTime;
    }

    private void ApplyVerticalMovement()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision

    private void CollisionChecks()
    {
        IsGrounded();
    }

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x, MovementStats.GroundDetectionLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0, Vector2.down, MovementStats.GroundDetectionLength, MovementStats.GroundLayer);

        if (groundHit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        #region Debug View

        if (MovementStats.DebugShowIsGrounded)
        {
            Color rayColor;
            if (isGrounded) rayColor = Color.green;
            else rayColor = Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MovementStats.GroundDetectionLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MovementStats.GroundDetectionLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MovementStats.GroundDetectionLength), Vector2.right * boxCastSize.x, rayColor);
        }

        #endregion
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
}
