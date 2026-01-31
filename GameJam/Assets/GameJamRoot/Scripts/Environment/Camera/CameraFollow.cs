using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public Vector2 offset;
    public float smoothTime = 0.15f;

    [Header("Confiner Settings")]
    public bool useConfiner = true;
    public Vector2 minBounds = new Vector2(-10f, -10f);
    public Vector2 maxBounds = new Vector2(10f, 10f);

    [Header("Dead Zone (para evitar ver el techo)")]
    public bool useDeadZone = true;
    public float topDeadZone = 2f; // Distancia desde el borde superior donde la cámara deja de seguir
    public float bottomDeadZone = 0f; // Opcional: también para el suelo

    [Header("Debug")]
    public bool showBoundsGizmos = true;
    public bool showDeadZoneGizmos = true;

    private Vector3 _velocity = Vector3.zero;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        // Aplicar dead zone ANTES del smooth
        if (useDeadZone)
        {
            targetPosition = ApplyDeadZone(targetPosition);
        }

        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            smoothTime
        );

        // Aplicar confiner DESPUÉS del smooth
        if (useConfiner)
        {
            smoothedPosition = ClampCameraPosition(smoothedPosition);
        }

        transform.position = smoothedPosition;
    }

    private Vector3 ApplyDeadZone(Vector3 targetPos)
    {
        float cameraHalfHeight = _camera.orthographicSize;

        // Calcular límites de la dead zone
        float topLimit = maxBounds.y - cameraHalfHeight - topDeadZone;
        float bottomLimit = minBounds.y + cameraHalfHeight + bottomDeadZone;

        // Solo aplicar dead zone en Y
        float clampedY = targetPos.y;

        // Si el target está muy arriba, no seguirlo más allá del límite
        if (targetPos.y > topLimit)
        {
            clampedY = topLimit;
        }
        // Si el target está muy abajo (opcional)
        else if (targetPos.y < bottomLimit)
        {
            clampedY = bottomLimit;
        }

        return new Vector3(targetPos.x, clampedY, targetPos.z);
    }

    private Vector3 ClampCameraPosition(Vector3 position)
    {
        float cameraHalfHeight = _camera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * _camera.aspect;

        float clampedX = Mathf.Clamp(
            position.x,
            minBounds.x + cameraHalfWidth,
            maxBounds.x - cameraHalfWidth
        );

        float clampedY = Mathf.Clamp(
            position.y,
            minBounds.y + cameraHalfHeight,
            maxBounds.y - cameraHalfHeight
        );

        return new Vector3(clampedX, clampedY, position.z);
    }

    private void OnDrawGizmos()
    {
        if (!showBoundsGizmos || !useConfiner) return;

        // Dibujar bounds en verde
        Gizmos.color = Color.green;
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        // Dibujar dead zone en rojo
        if (showDeadZoneGizmos && useDeadZone)
        {
            Camera cam = GetComponent<Camera>();
            if (cam == null) return;

            float cameraHalfHeight = cam.orthographicSize;

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Rojo semi-transparente

            // Dead zone superior
            if (topDeadZone > 0)
            {
                float topDeadZoneY = maxBounds.y - cameraHalfHeight - topDeadZone;
                Vector3 topDZLeft = new Vector3(minBounds.x, topDeadZoneY, 0);
                Vector3 topDZRight = new Vector3(maxBounds.x, topDeadZoneY, 0);

                Gizmos.DrawLine(topDZLeft, topDZRight);

                // Zona prohibida (arriba de esta línea, la cámara no sigue)
                Vector3 forbiddenTopLeft = new Vector3(minBounds.x, topDeadZoneY, 0);
                Vector3 forbiddenTopRight = new Vector3(maxBounds.x, topDeadZoneY, 0);
                Vector3 forbiddenBottomRight = new Vector3(maxBounds.x, maxBounds.y, 0);
                Vector3 forbiddenBottomLeft = new Vector3(minBounds.x, maxBounds.y, 0);

                Gizmos.DrawLine(forbiddenTopLeft, forbiddenTopRight);
                Gizmos.DrawLine(forbiddenTopRight, forbiddenBottomRight);
                Gizmos.DrawLine(forbiddenBottomRight, forbiddenBottomLeft);
                Gizmos.DrawLine(forbiddenBottomLeft, forbiddenTopLeft);
            }

            // Dead zone inferior (opcional)
            if (bottomDeadZone > 0)
            {
                float bottomDeadZoneY = minBounds.y + cameraHalfHeight + bottomDeadZone;
                Vector3 bottomDZLeft = new Vector3(minBounds.x, bottomDeadZoneY, 0);
                Vector3 bottomDZRight = new Vector3(maxBounds.x, bottomDeadZoneY, 0);

                Gizmos.DrawLine(bottomDZLeft, bottomDZRight);
            }
        }
    }
}