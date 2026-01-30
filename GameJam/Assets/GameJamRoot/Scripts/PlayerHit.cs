using UnityEngine;

public class PlayerDeathOnSpikes : MonoBehaviour
{
    [Header("Death Settings")]
    public bool instantDeath = true;

    private bool isDead = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

      
        if (other.CompareTag("Spikes"))
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("El jugador ha muerto");


        gameObject.SetActive(false);

    }
}
