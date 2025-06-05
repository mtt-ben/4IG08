using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Particle : MonoBehaviour
{
    // Particle fields
    public Vector2 velocity = Vector2.zero;
    public Vector2 position;
    public Vector2 prevPosition;
    public List<(float, float)> springs;
    public int grid_x = 0;
    public int grid_y = 0;
    public float density = 0f;
    public float density_near = 0f;

    // Collision-related
    private CircleCollider2D circleCollider;
    public float damping = 0.8f;  // Damping factor for collision velocity response
    public List<Vector2> collisionNormals = new(); // Store normals of collisions

    void Start()
    {
        position = transform.position;
        prevPosition = position;
        springs = new List<(float, float)>();
        grid_x = (int)((position.x - Config.Instance.X_MIN) / (Config.Instance.X_MAX - Config.Instance.X_MIN) * Config.Instance.GRID_SIZE_X);
        grid_y = (int)((position.y - Config.Instance.Y_MIN) / (Config.Instance.Y_MAX - Config.Instance.Y_MIN) * Config.Instance.GRID_SIZE_Y);

        // Initialize or add CircleCollider2D (set as trigger to avoid physics interference)
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
        }
        circleCollider.radius = Config.Instance.COLLISION_RADIUS;
    }

    public List<(Vector2 normal, float penetration)> GetCollisions()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, Config.Instance.COLLISION_RADIUS, LayerMask.GetMask("Default", "Ground"));
        List<(Vector2 normal, float penetration)> collisions = new();

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.gameObject.layer != LayerMask.NameToLayer("Water"))
            {
                Vector2 closestPoint = hit.ClosestPoint(position);
                Vector2 diff = position - closestPoint;
                float dist = diff.magnitude;

                if (dist < Config.Instance.COLLISION_RADIUS && dist > 0f)
                {
                    Vector2 normal = diff.normalized;
                    float penetration = Config.Instance.COLLISION_RADIUS - dist;
                    collisions.Add((normal, penetration));
                }
            }
        }

        return collisions;
    }

    public void ResolveAllCollisions(int maxIterations = 3)
    {
        for (int i = 0; i < maxIterations; i++)
        {
            var collisions = GetCollisions();
            if (collisions.Count == 0) break;

            foreach (var (normal, penetration) in collisions)
            {
                // Resolve penetration
                position += normal * penetration;
                collisionNormals.Add(normal);
            }
        }
    }

}
