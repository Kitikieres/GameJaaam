using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerMovement")]

public class PlayerMovementStats : ScriptableObject
{
    [Header("=== MOVEMENT ===")]
    [Range(0, 1f)] public float MoveThreshold = 0.25f;
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;

    [Header("=== RUN ===")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    [Header("=== GROUND/COLLISION CHECKS ===")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.3f;
    [Range(0f, 1f)] public float HeadWidht = 0.75f;

    [Header("=== JUMP ===")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;

    [Header("=== JUMP CUT ===")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;

    [Header("=== JUMP APEX ===")]
    [Tooltip("Qué tan cerca del punto más alto activa el hang time (0.97 = 97% del camino hacia arriba)")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Tooltip("Tiempo que el jugador flota en el punto más alto")]
    [Range(0f, 0.5f)] public float ApexHangTime = 0.075f;

    [Header("=== JUMP BUFFER ===")]
    [Tooltip("Tiempo que recuerda el input de salto antes de tocar el suelo")]
    [Range(0f, 0.5f)] public float JumpBufferTime = 0.125f;

    [Header("=== COYOTE TIME ===")]
    [Tooltip("Tiempo que puede saltar después de dejar el suelo")]
    [Range(0f, 0.5f)] public float JumpCoyoteTime = 0.1f;

    [Header("=== DASH (Solo con máscara) ===")]
    [Tooltip("Duración del dash")]
    [Range(0.05f, 0.5f)] public float DashTime = 0.11f;
    [Tooltip("Velocidad del dash")]
    [Range(1f, 200f)] public float DashSpeed = 40f;
    [Tooltip("Tiempo entre dashes en el suelo")]
    [Range(0f, 1f)] public float TimeBtwDashesOnGround = 0.225f;
    [Tooltip("Número de dashes permitidos en el aire")]
    [Range(1, 5)] public int NumberOfDashes = 2;
    [Tooltip("Preferencia hacia direcciones diagonales")]
    [Range(0f, 0.5f)] public float DashDiagonallyBias = 0.4f;

    [Header("=== DASH CANCEL ===")]
    [Range(0.01f, 5f)] public float DashGravityOnReleaseMultiplier = 1f;
    [Range(0.02f, 0.3f)] public float DashTimeForUpwardsCancel = 0.027f;

    [Header("=== POST-MASK BONUSES (Opcional) ===")]
    [Tooltip("Activar para aplicar bonuses después de obtener la máscara")]
    public bool EnablePostMaskBonuses = false;
    [Tooltip("Multiplicador de velocidad después de la máscara")]
    [Range(1f, 2f)] public float PostMaskSpeedMultiplier = 1.2f;
    [Tooltip("Multiplicador de altura de salto después de la máscara")]
    [Range(1f, 2f)] public float PostMaskJumpMultiplier = 1.1f;

    [Header("=== DEBUG ===")]
    public bool DebugShowIsGroundedBox = false;
    public bool DebugShowHeadBumpBox = false;
    public bool DebugShowJumpInfo = false;

    [Header("=== JUMP VISUALIZATION TOOL ===")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    public readonly Vector2[] DashDirections = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1).normalized,
        new Vector2(0, 1),
        new Vector2(-1, 1).normalized,
        new Vector2(-1, 0),
        new Vector2(-1, -1).normalized,
        new Vector2(0, -1),
        new Vector2(1, -1).normalized,
    };

    // Propiedades calculadas
    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

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
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}