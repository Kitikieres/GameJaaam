using System.Collections;
using UnityEngine;

public class MaskFusion2D : MonoBehaviour
{
    private Timer _timer;

    [Header("Fusion Settings")]
    public Transform fusionPoint;
    public Sprite fullMaskSprite;

    private bool _isFused;
    private PlayerMovement _playerMovement;
    private SpriteRenderer _spriteRenderer;
    private CameraCinematic2D _cameraCinematic;
    private PlayerRespawn2D _playerRespawn; // ← NUEVO

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cameraCinematic = Camera.main.GetComponent<CameraCinematic2D>();
        _playerRespawn = GetComponent<PlayerRespawn2D>(); // ← NUEVO

        _timer = FindFirstObjectByType<Timer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isFused) return;

        if (other.CompareTag("MaskHalf"))
            StartCoroutine(FusionSequence(other.gameObject));
    }

    private IEnumerator FusionSequence(GameObject otherHalf)
    {
        _isFused = true;
        _playerMovement.SetMovementLocked(true);

        if (_cameraCinematic != null)
            yield return StartCoroutine(_cameraCinematic.PlayFusionCinematic());

        Fuse(otherHalf);

        _playerMovement.SetMovementLocked(false);
    }

    private void Fuse(GameObject otherHalf)
    {
        Collider2D col = otherHalf.GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Rigidbody2D rb = otherHalf.GetComponent<Rigidbody2D>();
        if (rb) Destroy(rb);

        otherHalf.transform.SetParent(transform);
        otherHalf.transform.position = fusionPoint != null ? fusionPoint.position : transform.position;

        if (fullMaskSprite != null)
        {
            _spriteRenderer.sprite = fullMaskSprite;
            otherHalf.SetActive(false);
        }

        _playerMovement.FuseWithMask();

        if (_timer != null)
            _timer.ActivateSecondTimer();

        // ← NUEVO: Actualizar punto de respawn a la posición actual
        if (_playerRespawn != null)
        {
            _playerRespawn.UpdateRespawnPoint(transform.position);
            Debug.Log("🎭 Punto de respawn actualizado a posición de la máscara");
        }

        Debug.Log("✨ FUSIÓN COMPLETA");
    }
}