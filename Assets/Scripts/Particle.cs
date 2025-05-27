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

    void Start()
    {
        position = transform.position;
        prevPosition = position;
        springs = new List<(float, float)>();
        grid_x = (int)((position.x - Config.X_MIN) / (Config.X_MAX - Config.X_MIN) * Config.GRID_SIZE_X);
        grid_y = (int)((position.y - Config.Y_MIN) / (Config.Y_MAX - Config.Y_MIN) * Config.GRID_SIZE_Y);

        // Initialize or add CircleCollider2D (set as trigger to avoid physics interference)
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
        }
        circleCollider.radius = Config.COLLISION_RADIUS;
    }

    // Update is called by Simulation.cs
    public void UpdateState()
    {
        // Apply velocity to position
        prevPosition = position;
        position += velocity * Config.DT;

        // Update the grid position
        grid_x = (int)((position.x - Config.X_MIN) / (Config.X_MAX - Config.X_MIN) * Config.GRID_SIZE_X);
        grid_y = (int)((position.y - Config.Y_MIN) / (Config.Y_MAX - Config.Y_MIN) * Config.GRID_SIZE_Y);

        // Update the particle's transform position
        if (!float.IsNaN(position.x) && !float.IsNaN(position.y))
            transform.position = position;
    }
    
    // Collision detection returns (collided?, normal, penetration)
    public (bool, Vector2, float) CheckCollision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, Config.COLLISION_RADIUS, LayerMask.GetMask("Default ", "Ground"));

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.gameObject.layer != LayerMask.NameToLayer("Water"))
            {
                Vector2 closestPoint = hit.ClosestPoint(position);
                Vector2 diff = position - closestPoint;
                float dist = diff.magnitude;

                if (dist < Config.COLLISION_RADIUS && dist > 0f)  // Avoid division by zero
                {
                    Vector2 normal = diff.normalized;
                    float penetration = Config.COLLISION_RADIUS - dist;
                    return (true, normal, penetration);
                }
            }
        }
        return (false, Vector2.zero, 0f);
    }

    // Handle collision response: reflect velocity and resolve penetration
    public void HandleCollision(Vector2 normal, float penetration)
    {
        velocity = ReflectVelocity(velocity, normal) * damping;
        position += normal * penetration;
    }

    private Vector2 ReflectVelocity(Vector2 velocity, Vector2 normal)
    {
        return Vector2.Reflect(velocity, normal);
    }
}
