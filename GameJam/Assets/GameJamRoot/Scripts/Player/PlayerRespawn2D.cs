using UnityEngine;
using UnityEngine.SceneManagement;  
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
            Debug.Log("RESPAWN FORZADO CON R");
            StopAllCoroutines();
            RestartLevel();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Vida: " + currentHealth);

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

        Debug.Log("PLAYER MUERTO");

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
        RestartLevel();
    }

    
    void RestartLevel()
    {
        Debug.Log("NIVEL REINICIADO");

       
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ForceRespawn()
    {
        Debug.Log("PLAYER RESPAWNEADO");

        isDead = false;
        currentHealth = maxHealth;

        transform.position = respawnPoint.position;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = true;

        foreach (Collider2D c in colliders)
            c.enabled = true;

        sr.enabled = true;
    }
}
