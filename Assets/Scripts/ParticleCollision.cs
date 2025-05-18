using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    public Particle particle;  // Reference to your Particle script on this GameObject
    public float collisionDamping = 0.8f;  // Energy loss on collision (0-1)

    void Start()
    {
        // Auto-get the Particle script if not assigned
        if (particle == null)
            particle = GetComponent<Particle>();

        // Ensure Rigidbody2D is Kinematic
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.simulated = true;
        }
        else
        {
            Debug.LogWarning("No Rigidbody2D found on particle!");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (particle == null)
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            // Reflect particle velocity along the collision normal and apply damping
            particle.velocity = Vector2.Reflect(particle.velocity, normal) * collisionDamping;

            // Push particle slightly away from the collision surface to prevent sticking
            particle.position += normal * 0.01f;
        }
    }
}

