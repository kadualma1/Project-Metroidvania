using UnityEngine;

[CreateAssetMenu(menuName = "MovementStats")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Horizontal Movement")]
    [Range(1, 100)] public float MaxWalkSpeed = 12f;
    [Range(.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(.25f, 50f)] public float AirAcceleration = 5f;
    [Range(.25f, 50f)] public float AirDeceleration = 5f;

    [Header("Jump")]
    public float JumpHeight = 6f;
    [Range(1, 1.1f)] public float JumpHeightCompensation = 1.054f;
    public float TimeToJumpApex = .35f;
    [Range(.01f, 5)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int InitialNumberOfJumps = 1;
    [Range(.02f, .3f)] public float TimeForUpwardsCancel = .027f;
    [Range(.5f, 1f)] public float ApexThreshold = .97f;
    [Range(.01f, 1f)] public float ApexHangTime = .075f;
    [Range(0, 1)] public float JumpBufferTime = .125f;
    [Range(0, 1)] public float JumpCoyoteTime = .1f;

    [Header("Collision Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionLength = .02f;
    public float HeadDetectionLength = .02f;
    [Range(0, 1)] public float HeadWidth = .75f;

    [Header("Debug")]
    public bool DebugShowIsGrounded = true;
    public bool DebugShowBumpedHead = true;

    [Header("Jump Debug")]
    public bool ShowJumpArc = true;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int Steps = 90;

    public float AdjustedJumpHeight { get; set; }
    public float Gravity { get; set; }
    public float InitialJumpVelocity { get; set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensation;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeToJumpApex;
    }
}
