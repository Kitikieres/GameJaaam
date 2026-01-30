using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PlayerMovement")]

public class PlayerMovementStats : ScriptableObject
{
    [Header("Walker")]
    [Range(1f,100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f,50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 10f)] public float GroundDeceleration = 20f;
    [Range(0.25f,50f)] public float AirAcceleration = 5f;
    [Range(0.25f,10f)] public float AirDeceleration = 5f;

    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    [Header("Ground/Collision Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.3f;
    [Range(0f,1f)] public float HeadWidht = 0.75f;

}
