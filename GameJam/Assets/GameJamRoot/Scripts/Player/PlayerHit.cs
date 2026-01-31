using UnityEngine;

public class Spikes2D : MonoBehaviour
{
    public int damage = 100;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("TOCÓ SPIKES");

            // BUSCA EL SCRIPT EN EL PADRE
            PlayerRespawn2D player = other.GetComponentInParent<PlayerRespawn2D>();

            if (player != null)
            {
                Debug.Log("APLICANDO DAÑO");
                player.TakeDamage(damage);
            }
            else
            {
                Debug.Log("NO SE ENCONTRÓ PlayerRespawn2D EN EL PADRE");
            }
        }
    }
}


