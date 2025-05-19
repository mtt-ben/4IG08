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
    public int grid_size_x = GRID_SIZE_X; // The number of grid cells in the x direction
    public int grid_size_y = GRID_SIZE_Y; // The number of grid cells in the y direction
    public list[,] grid; // The grid for spatial partitioning
    public float x_min = X_MIN; // The minimum x coordinate for the grid
    public float x_max = X_MAX; // The maximum x coordinate for the grid
    public float y_min = Y_MIN; // The minimum y coordinate for the grid
    public float y_max = Y_MAX; // The maximum y coordinate for the grid

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
    /*
     * Algorithm 2 : Double Density Relaxation
     */
    {
        foreach (Particle p in particles)
        {
            list neighbors = new();
            f_list neighborUnitX = new();
            f_list neighborUnitY = new();
            f_list neighborcloseness = new();

            p.density = 0f;
            p.density_near = 0f;

            float grid_X = p.position.x / (x_max - x_min) * grid_size_x;
            float grid_Y = p.position.y / (y_max - y_min) * grid_size_y;

            for (int i = p.grid_x - (int)grid_X; i <= p.grid_x + (int)grid_X; i++)
            {
                for (int j = p.grid_y - (int)grid_Y; j <= p.grid_y + (int)grid_Y; j++)
                {
                    if (i >= 0 && i < grid_size_x && j >= 0 && j < grid_size_y)
                    {
                        foreach (Particle q in grid[i, j])
                        {
                            if (p != q)
                            {
                                float r = Vector2.Distance(p.position, q.position);
                                if (r < p.KernelRadius)
                                {
                                    float closeness = 1 - r / p.KernelRadius;
                                    p.density += closeness * closeness;
                                    p.density_near += closeness * closeness * closeness;

                                    // Store neighbor information
                                    neighbors.Add(q);
                                    neighborUnitX.Add((q.position.x - p.position.x) / r);
                                    neighborUnitY.Add((q.position.y - p.position.y) / r);
                                    neighborcloseness.Add(closeness);
                                }
                            }
                        }
                    }
                }
            }

            float pressure = p.Stiffness * (p.density - p.RestDensity);
            float nearPressure = p.NearStiffness * p.density_near;

            if (pressure > 1f)
            {
                pressure = 1f;
            }
            
            if (nearPressure > 1f)
            {
                nearPressure = 1f;
            }

            Vector2 disp = Vector2.zero;

            for (int i = 0; i < neighbors.Count; i++)
            {
                float closeness = neighborcloseness[i];
                float D = (pressure * closeness + nearPressure * closeness * closeness) / 2f;
                float DX = D * neighborUnitX[i];
                float DY = D * neighborUnitY[i];

                // Apply displacement to the neighbor
                neighbors[i].position += new Vector2(DX, DY);

                // Apply displacement to the current particle
                disp -= new Vector2(DX, DY);
            }

            // Update particle position
            p.position += disp;
        }
    }

    void SpringDisplacement()
    /*
     * Algorithm 3 : Spring Displacement
     */
    {

    }

    void SpringAdjustment()
    /*
     * Algorithm 4 : Spring Adjustment
     */
    {
        // done in the springDisplacement function
    }

    void ResolveCollisions()
    /*
     * Algorithm 6 : Particle-body interactions
     */
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

    void ViscosityImpulse()
    /*
     * Algorithm 5 : Viscosity Impulse
     */
    {

    }

    // Algorithm 1 : Simulation Step
    void FixedUpdate()
    {
        // Apply gravity
        foreach (Particle p in particles)
        {
            p.velocity += new Vector2(0, -G * DT);
        }

        ViscosityImpulse();

        // Save previous position and update position
        foreach (Particle p in particles)
        {
            p.prevPosition = p.position;
            p.position += p.velocity * DT;
            p.UpdateState();
        }

        SpringAdjustment();
        SpringDisplacement();
        DoubleDensityRelaxation();
        ResolveCollisions();

        // Use previous position to compute next velocity
        foreach (Particle p in particles)
        {
            p.velocity = (p.position - p.prevPosition) / DT;
            p.UpdateState();
        }
    }
}
