using UnityEngine;

public class Spikes2D : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("El jugador murió");

            
            other.gameObject.SetActive(false);

         
        }
    }
}

