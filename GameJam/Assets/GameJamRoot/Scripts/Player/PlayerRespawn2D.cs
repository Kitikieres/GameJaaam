using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn2D : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 🔁 REINICIAR NIVEL CON R (vivo o muerto)
        if (InputManager.RespawnWasPressed)
        {
            Debug.Log("Reinicio de nivel con R");
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
        Debug.Log("Jugador muerto → reiniciando nivel");

        RestartLevel();
    }

    void RestartLevel()
    {
        // Reinicia la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
