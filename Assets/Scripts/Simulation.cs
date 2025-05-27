using System.Collections.Generic;
using UnityEngine;

using list = System.Collections.Generic.List<Particle>;
using f_list = System.Collections.Generic.List<float>;

using static Config;
using NUnit.Framework.Constraints;
public class Simulation : MonoBehaviour
{
    public GameObject Player;
    public GameObject Base_Particle; // The reference base particle object
    public list particles; // The list of particles
    public int grid_size_x = 60; // The number of grid cells in the x direction
    public int grid_size_y = 30; // The number of grid cells in the y direction
    public list[,] grid; // The grid for spatial partitioning
    public float x_min = -20f; // The minimum x coordinate for the grid
    public float x_max = 20f; // The maximum x coordinate for the grid
    public float y_min = -10f; // The minimum y coordinate for the grid
    public float y_max = 10f; // The maximum y coordinate for the grid

    // Physics parameters
    public float DT = 0.03f; // Time step
    public float k = 1f; // Pressure constant
    public float k_near = 10f; // Near pressure constant
    public float h = KERNEL_RADIUS;

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

    void DoubleDensityRelaxation()
    {
        foreach (Particle p in particles)
        {
            list neighbors = new();
            f_list qList = new();

            p.density = 0f;
            p.density_near = 0f;

            int M = Mathf.CeilToInt(h / (x_max - x_min) * grid_size_x);
            int L = Mathf.CeilToInt(h / (y_max - y_min) * grid_size_y);

            if (M < 1) M = 1;
            if (L < 1) L = 1;

            for (int i = -M; i <= M; i++)
            {
                for (int j = -L; j <= L; j++)
                {
                    int gridX = p.grid_x + i;
                    int gridY = p.grid_y + j;
                    if (gridX < 0 || gridX >= grid_size_x || gridY < 0 || gridY >= grid_size_y) continue;
                    if (grid[gridX, gridY] == null) continue;

                    foreach (Particle neighbor in grid[gridX, gridY])
                    {
                        // if (neighbor == p) continue; // Skip self (maybe)

                        float q = Vector2.Distance(p.position, neighbor.position) / h;
                        if (q < 1f)
                        {
                            p.density += (1 - q) * (1 - q);
                            p.density_near += (1 - q) * (1 - q) * (1 - q);

                            neighbors.Add(neighbor);
                            qList.Add(q);
                        }
                    }
                }
            }

            float pressure = k * (p.density - REST_DENSITY);
            float nearPressure = k_near * p.density_near;
            Vector2 dx = Vector2.zero;

            for (int i = 0; i < neighbors.Count; i++)
            {
                Particle neighbor = neighbors[i];
                float q = qList[i];

                Vector2 D = DT * DT * (pressure * (1 - q) + nearPressure * (1 - q) * (1 - q)) * (neighbor.position - p.position);
                neighbor.position += D / 2f;
                dx -= D / 2f;
            }

            p.position += dx;

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

    // Simulation Step
    void LateUpdate()
    {
        // Apply gravity
        foreach (Particle p in particles)
        {
             p.velocity += new Vector2(0, -G * DT);
        }

        // Clear grid
        for (int i = 0; i < grid_size_x; i++)
        {
            for (int j = 0; j < grid_size_y; j++)
            {
                grid[i, j].Clear();
            }
        }

        // Place particles in grid
        foreach (Particle p in particles)
        {
            if (p.grid_x >= 0 && p.grid_x < grid_size_x &&
                p.grid_y >= 0 && p.grid_y < grid_size_y)
            {
                grid[p.grid_x, p.grid_y].Add(p);
            }
        }

        DoubleDensityRelaxation();
        ResolveCollisions();

        // Update particle states
        list particlesToRemove = new();
        foreach (Particle p in particles)
        {
            if (p.position.x < x_min || p.position.x > x_max || p.position.y < y_min || p.position.y > y_max)
            {
                particlesToRemove.Add(p);
                continue;
            }
            p.UpdateState();
        }

        // Remove particles that are out of bounds
        foreach (Particle p in particlesToRemove)
        {
            RemoveParticle(p);
            Destroy(p.gameObject);
        }
    }

    public void AddParticle(Particle p)
    {
        particles.Add(p);

        int gridX = Mathf.Clamp((int)((p.position.x - x_min) / (x_max - x_min) * grid_size_x), 0, grid_size_x - 1);
        int gridY = Mathf.Clamp((int)((p.position.y - y_min) / (y_max - y_min) * grid_size_y), 0, grid_size_y - 1);

        grid[gridX, gridY].Add(p);

        p.grid_x = gridX;
        p.grid_y = gridY;
    }

    public void RemoveParticle(Particle p)
    {
        particles.Remove(p);

        int gridX = Mathf.Clamp((int)((p.prevPosition.x - x_min) / (x_max - x_min) * grid_size_x), 0, grid_size_x - 1);
        int gridY = Mathf.Clamp((int)((p.prevPosition.y - y_min) / (y_max - y_min) * grid_size_y), 0, grid_size_y - 1);

        if (grid[gridX, gridY].Contains(p))
        {
            grid[gridX, gridY].Remove(p);
        }
    }
}
