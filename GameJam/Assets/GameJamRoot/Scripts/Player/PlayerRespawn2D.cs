using UnityEngine;
using System.Collections;

public class PlayerRespawn2D : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 1.5f;

    private bool isDead = false;
    private Vector3 _dynamicRespawnPosition; // ← NUEVO
    private bool _useDynamicRespawn = false; // ← NUEVO

    Rigidbody2D rb;
    Collider2D[] colliders;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (InputManager.RespawnWasPressed)
        {
            Debug.Log("🔄 RESPAWN FORZADO CON R");
            StopAllCoroutines();
            ForceRespawn();
        }
    }

    // ← NUEVO MÉTODO
    public void UpdateRespawnPoint(Vector3 newPosition)
    {
        _dynamicRespawnPosition = newPosition;
        _useDynamicRespawn = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("❤️ Vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;
        Debug.Log("💀 PLAYER MUERTO");

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        foreach (Collider2D c in colliders)
            c.enabled = false;

        sr.enabled = false;

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        ForceRespawn();
    }

    void ForceRespawn()
    {
        Debug.Log("✅ PLAYER RESPAWNEADO");

        isDead = false;
        currentHealth = maxHealth;

        // ← MODIFICADO: Usar posición dinámica si está disponible
        Vector3 targetPosition = _useDynamicRespawn
            ? _dynamicRespawnPosition
            : respawnPoint.position;

        transform.position = targetPosition;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = true;

        foreach (Collider2D c in colliders)
            c.enabled = true;

        sr.enabled = true;
    }
}