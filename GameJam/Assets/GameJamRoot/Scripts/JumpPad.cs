using UnityEngine;

public class Trampoline2D : MonoBehaviour
{
    public float bounceVelocity = 18f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        player.TrampolineBounce(bounceVelocity);
    }
}
