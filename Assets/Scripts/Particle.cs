using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using static Config;

public class Particle : MonoBehaviour
{
    // Particle fields
    public Vector2 velocity = Vector2.zero;
    public Vector2 position;
    public Vector2 prevPosition;
    public List<(float, float)> springs;
    public bool inPlayer;
    public int grid_x = 0;
    public int grid_y = 0;
    public float density = 0f;
    public float density_near = 0f;

    // Collision-related
    private CircleCollider2D circleCollider;
    public float damping = 0.8f;  // Damping factor for collision velocity response

    // Particle properties
    public float RestDensity => inPlayer ? INPLAYER_REST_DENSITY : REST_DENSITY;
    public float Stiffness => inPlayer ? INPLAYER_STIFFNESS : STIFFNESS;
    public float NearStiffness => inPlayer ? INPLAYER_STIFFNESS_NEAR : STIFFNESS_NEAR;
    public float KernelRadius => inPlayer ? INPLAYER_KERNEL_RADIUS : KERNEL_RADIUS;
    public float SpringStiffness => inPlayer ? INPLAYER_SPRING_STIFFNESS : SPRING_STIFFNESS;
    public float PLASTICITY => inPlayer ? INPLAYER_PLASTICITY : PLASTICITY;
    public float YIELD_RATIO => inPlayer ? INPLAYER_YIELD_RATIO : YIELD_RATIO;
    public float MinDistRatio => inPlayer ? INPLAYER_MIN_DIST_RATIO : MIN_DIST_RATIO;
    public float LinViscosity => inPlayer ? INPLAYER_LIN_VISCOSITY : LIN_VISCOSITY;
    public float QuadViscosity => inPlayer ? INPLAYER_QUAD_VISCOSITY : QUAD_VISCOSITY;
    public float MaxPressure => inPlayer ? INPLAYER_MAX_PRESSURE : MAX_PRESSURE;

    void Start()
    {
        position = transform.position;
        prevPosition = position;
        springs = new List<(float, float)>();
        inPlayer = false;
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

        // Update the particle's transform position
        transform.position = position;
    }
    
    // Collision detection returns (collided?, normal, penetration)
    public (bool, Vector2, float) CheckCollision()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, KernelRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != this.gameObject)
            {
                Vector2 closestPoint = hit.ClosestPoint(position);
                Vector2 diff = position - closestPoint;
                float dist = diff.magnitude;

                if (dist < KernelRadius && dist > 0f)  // Avoid division by zero
                {
                    Vector2 normal = diff.normalized;
                    float penetration = KernelRadius - dist;
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
        UpdateState();
    }

    private Vector2 ReflectVelocity(Vector2 velocity, Vector2 normal)
    {
        return Vector2.Reflect(velocity, normal);
    }
}
