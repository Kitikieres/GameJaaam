using System.Collections;
using UnityEngine;

public class MaskFusion2D : MonoBehaviour
{
    private Timer _timer;

    [Header("Fusion Settings")]
    public Transform fusionPoint;
    public Sprite fullMaskSprite;

    [Header("Goal Line")]
    public GameObject goalLine; // 👉 Línea de meta que se activará

    private bool _isFused;

    private PlayerMovement _playerMovement;
    private SpriteRenderer _spriteRenderer;
    private CameraCinematic2D _cameraCinematic;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Cámara
        if (Camera.main != null)
            _cameraCinematic = Camera.main.GetComponent<CameraCinematic2D>();

        // Timer
        _timer = FindObjectOfType<Timer>();

        // Aseguramos que la meta esté desactivada al inicio
        if (goalLine != null)
            goalLine.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isFused) return;

        if (other.CompareTag("MaskHalf"))
        {
            StartCoroutine(FusionSequence(other.gameObject));
        }
    }

    private IEnumerator FusionSequence(GameObject otherHalf)
    {
        _isFused = true;

        // Bloquear movimiento
        if (_playerMovement != null)
            _playerMovement.SetMovementLocked(true);

        // Cinemática de cámara
        if (_cameraCinematic != null)
            yield return StartCoroutine(_cameraCinematic.PlayFusionCinematic());

        // Fusión real
        Fuse(otherHalf);

        // Desbloquear movimiento
        if (_playerMovement != null)
            _playerMovement.SetMovementLocked(false);
    }

    private void Fuse(GameObject otherHalf)
    {
        // Desactivar collider de la otra mitad
        Collider2D col = otherHalf.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Quitar rigidbody
        Rigidbody2D rb = otherHalf.GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);

        // Parent + posición
        otherHalf.transform.SetParent(transform);
        otherHalf.transform.position = fusionPoint != null ? fusionPoint.position : transform.position;

        // Cambiar sprite a máscara completa
        if (fullMaskSprite != null && _spriteRenderer != null)
        {
            _spriteRenderer.sprite = fullMaskSprite;
            otherHalf.SetActive(false);
        }

        // Activar estado fused en el player
        if (_playerMovement != null)
            _playerMovement.FuseWithMask();

        // Activar segundo temporizador
        if (_timer != null)
            _timer.ActivateSecondTimer();

        // 🔥 ACTIVAR LÍNEA DE META
        if (goalLine != null)
            goalLine.SetActive(true);

        Debug.Log("✨ FUSIÓN COMPLETA - META ACTIVADA ✨");
    }
}