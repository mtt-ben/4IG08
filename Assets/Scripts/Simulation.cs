using System.Collections.Generic;
using UnityEngine;

using list = System.Collections.Generic.List<Particle>;
using f_list = System.Collections.Generic.List<float>;

using static Config;
using static SmoothingKernel;
using NUnit.Framework.Constraints;

public class Simulation : MonoBehaviour
{
    public GameObject Player;
    public GameObject Base_Particle; // The reference base particle object
    public list particles; // The list of particles
    public int grid_size_x = GRID_SIZE_X; // The number of grid cells in the x direction
    public int grid_size_y = GRID_SIZE_Y; // The number of grid cells in the y direction
    public list[,] grid; // The grid for spatial partitioning
    public float x_min = X_MIN; // The minimum x coordinate for the grid
    public float x_max = X_MAX; // The maximum x coordinate for the grid
    public float y_min = Y_MIN; // The minimum y coordinate for the grid
    public float y_max = Y_MAX; // The maximum y coordinate for the grid
    public float DT = Config.DT; // The time step for the simulation
    public SmoothingKernel kernel = new();
    public bool showVelocity = false;
    public bool isPauses = true;
    

    void Start()
    {
        grid = new list[grid_size_x, grid_size_y];
        for (int i = 0; i < grid_size_x; i++)
        {
            for (int j = 0; j < grid_size_y; j++)
            {
                grid[i, j] = new list();
            }
        }

        particles = new list(FindObjectsByType<Particle>(FindObjectsSortMode.None));
    }


    // NEED TO IMPLEMENT THE KERNEL FUNCTIONS


    void UpdateGrid()
    {
        // Build the grid
        for (int i = 0; i < grid_size_x; i++)
        {
            for (int j = 0; j < grid_size_y; j++)
            {
                grid[i, j].Clear();
            }
        }
        foreach (Particle p in particles)
        {
            if (p.grid_x < 0 || p.grid_x >= grid_size_x || p.grid_y < 0 || p.grid_y >= grid_size_y)
            {
                continue; // Skip particles outside the grid
            }
            grid[p.grid_x, p.grid_y].Add(p);
        }
    }

    void BuildNeighbors()
    {
        UpdateGrid();
        
        foreach (Particle p in particles)
        {
            p.neighbors.Clear();
            for (int i = p.grid_x - 1; i <= p.grid_x + 1; i++)
            {
                for (int j = p.grid_y - 1; j <= p.grid_y + 1; j++)
                {
                    // Skip out-of-bounds grid cells
                    if (!(i >= 0 && i < grid_size_x && j >= 0 && j < grid_size_y)) continue;
                    foreach (Particle q in grid[i, j])
                    {
                        if (p == q) continue;
                        float r = Vector2.Distance(p.position, q.position);
                        if (r < kernel.supportRadius) p.neighbors.Add(q);
                    }
                }
            }
        }
    }

    void ResolveCollisions()
    {
        foreach (Particle p in particles)
        {
            var (collided, normal, penetration) = p.CheckCollision();
            if (collided)
            {
                p.HandleCollision(normal, penetration);
            }
        }
    }

    void DrawVelocity()
    {
        foreach(Particle p in particles)
        {
            p.DrawVelocity();
        }
    }

    void DrawDensity()
    {
        foreach (Particle p in particles)
        {
            p.DrawDensity();
        }
    }

    void ApplyBodyForce()
    {
        // Apply body force to the particles
        foreach (Particle p in particles)
        {
            p.acceleration += new Vector2(0, -G); // Gravity force
        }
    }

    float EquationOfState(float d, float d0, float k, float gamma=7f)
    {
        return k * (Mathf.Pow(d/d0, gamma) - 1f);
    }

    void ComputePressure()
    {
        foreach (Particle p in particles)
        {
            // Use the equation of state (EOS) function
            const float k = 0.01f;
            p.pressure = p.density > 0 ? EquationOfState(p.density, p.RestDensity, k) : 0f;
            p.pressure = p.pressure < 0 ? 0f : p.pressure; // Ensure pressure is non-negative
        }
    }

    void ComputeDensity()
    {
        foreach (Particle p in particles)
        {
            p.density = p.mass * 1f; // Reset density to the mass of the particle at its position
            foreach (Particle q in p.neighbors)
            {
                p.density += q.mass * kernel.W(p.position-q.position);
            }
        }
    }

    void ApplyPressure()
    {
        foreach (Particle p in particles)
        {
            Vector2 pressureAcc = Vector2.zero;
            foreach (Particle q in p.neighbors)
            {
                float coefficient = p.pressure / Mathf.Pow(p.density, 2) + q.pressure / Mathf.Pow(q.density, 2);
                Vector2 rij = p.position - q.position;
                pressureAcc += q.mass * coefficient * kernel.grad_W(rij);
            }
            p.acceleration -= pressureAcc;
        }
    }


    // Algorithm 1 : Simulation Step
    void LateUpdate()
    {
        if (isPauses) return;

        // Update the grid and neighbors
        BuildNeighbors();
        ComputeDensity();

        // GRAVITY
        // ApplyBodyForce();

        // PRESSURE
        ComputePressure();
        ApplyPressure();

        ResolveCollisions();
        foreach (Particle p in particles) p.UpdateState();

        if (showVelocity) DrawVelocity();
        DrawDensity();
        DrawVelocity();
    }
}
