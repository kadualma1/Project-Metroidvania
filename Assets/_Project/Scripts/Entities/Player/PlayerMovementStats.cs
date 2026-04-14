using UnityEngine;

public class PlayerMovementStats : ScriptableObject
{
    [Header("Horizontal Movement")]
    [Range(1, 100)] public float MaxWalkSpeed = 12f;
    [Range(.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(.25f, 50f)] public float AirAcceleration = 5f;
    [Range(.25f, 50f)] public float AirDeceleration = 5f;

    [Header("Collision Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionLength = .02f;
    public float HeadDetectionLength = .02f;
    [Range(0, 1)] public float HeadWidth = .75f;


}
