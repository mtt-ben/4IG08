using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using static Config;

public class Particle : MonoBehaviour
{
    // Particle fields
    public Vector2 velocity = Vector2.zero;
    public Vector2 acceleration = Vector2.zero;
    public Vector2 position;
    public int grid_x = 0;
    public int grid_y = 0;
    public float density = 0f;

    // Collision-related
    private CircleCollider2D circleCollider;
    public float damping = 0.8f;  // Damping factor for collision velocity response

    // Particle properties
    public float RestDensity => REST_DENSITY;
    public float KernelRadius => KERNEL_RADIUS;
    public float mass = 1;
    public float pressure = 0f;

    // List or set of neighboring particles
    public List<Particle> neighbors = new();

    public bool showNeighbors = false;
    public bool showVelocity = false;
    public float KernelRadiusSolid = 0.08f; // Radius for solid collision detection


    void Start()
    {
        position = transform.position;
        grid_x = (int)((position.x - X_MIN) / (X_MAX - X_MIN) * GRID_SIZE_X);
        grid_y = (int)((position.y - Y_MIN) / (Y_MAX - Y_MIN) * GRID_SIZE_Y);

        // Initialize or add CircleCollider2D (set as trigger to avoid physics interference)
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
        }
        circleCollider.radius = KernelRadius;
    }

    // Update is called by Simulation.cs
    public void UpdateState()
    {
        if (position.x < X_MIN || position.x > X_MAX || position.y < Y_MIN || position.y > Y_MAX)
        {
            // Reset the particle's position if it goes out of bounds
            position = new Vector2(Mathf.Clamp(position.x, X_MIN, X_MAX), Mathf.Clamp(position.y, Y_MIN, Y_MAX));
        }

        // Update the grid position
        grid_x = (int)((position.x - X_MIN) / (X_MAX - X_MIN) * GRID_SIZE_X);
        grid_y = (int)((position.y - Y_MIN) / (Y_MAX - Y_MIN) * GRID_SIZE_Y);

        // Update velocity and position
        velocity += acceleration * DT; // Update velocity with acceleration
        velocity = Vector2.ClampMagnitude(velocity, 9f); // Adjust the max speed as needed
        position += velocity * DT; // Update position with velocity
        acceleration = Vector2.zero; // Reset acceleration after applying it

        // Update the particle's transform position
        transform.position = position;
    }

    // Collision detection returns (collided?, normal, penetration)
    public (bool, Vector2, float) CheckCollision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, KernelRadiusSolid);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.gameObject.layer != LayerMask.NameToLayer("Water"))
            {
                Vector2 closestPoint = hit.ClosestPoint(position);
                Vector2 diff = position - closestPoint;
                float dist = diff.magnitude;

                if (dist < KernelRadiusSolid && dist > 0f)  // Avoid division by zero
                {
                    Vector2 normal = diff.normalized;
                    float penetration = KernelRadiusSolid - dist;
                    return (true, normal, penetration);
                }
            }
        }
        return (false, Vector2.zero, 0f);
    }

    // Handle collision response: reflect velocity and resolve penetration
    public void HandleCollision(Vector2 normal, float penetration)
    {
        velocity = Vector2.Reflect(velocity, normal) * damping;
        position += normal * penetration;
        UpdateState();
    }

    public void DrawVelocity()
    {
        if (showVelocity)
        {
            Debug.DrawLine(position, position + velocity, Color.yellow, 0.1f, false);
        }
    }

    // Visualize neighbors using Gizmos (draw a green circle around the particle)
    void OnDrawGizmosSelected()
    {
        if (showNeighbors)
        {
            Gizmos.color = Color.green;
            int segments = 32;
            Vector3 center = (Vector3)position;
            float angleStep = 2 * Mathf.PI / segments;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0), 0) * KernelRadius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * KernelRadius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }

    public void DrawDensity()
    {
        // Change the color of the SpriteRenderer based on density
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            // Normalize density for color mapping (adjust maxDensity as needed)
            float maxDensity = 10f;
            float t = Mathf.Clamp01(density / maxDensity);
            // Color from blue (low density) to red (high density)
            sr.color = Color.Lerp(Color.blue, Color.red, t);
        }
    }
}
