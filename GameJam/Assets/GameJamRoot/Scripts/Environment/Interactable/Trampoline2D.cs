using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float bounceVelocity = 25f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

            if (player != null)
            {
                player.TrampolineBounce(bounceVelocity);
            }
        }
    }
}
