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
    public bool showVelocity = false;
    

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
    float Kernel(Vector2 p, Vector2 q)
    {
        float h = KERNEL_RADIUS;
        Vector2 rVec = p - q;
        float r2 = rVec.sqrMagnitude;

        if (r2 >= h * h)
            return 0f;

        float coef = 4f / (Mathf.PI * Mathf.Pow(h, 8));
        return coef * Mathf.Pow(h * h - r2, 3);
    }

    Vector2 gradientKernel(Vector2 p, Vector2 q)
    {
        float h = KERNEL_RADIUS;
        Vector2 rVec = p - q;
        float r2 = rVec.sqrMagnitude;

        if (r2 == 0f || r2 >= h * h)
            return Vector2.zero;

        float coef = -24f / (Mathf.PI * Mathf.Pow(h, 8));
        return coef * Mathf.Pow(h * h - r2, 2) * rVec;
    }


    void updatePosition()
    {
        foreach (Particle p in particles)
        {
            p.prevPosition = p.position;
            p.position += p.velocity * DT;
        }
    }

    void updateVelocity()
    {
        foreach (Particle p in particles)
        {
            p.velocity += p.acceleration * DT;
        }
    }

    void updateGrid()
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

    void buildNeighbors()
    {
        updateGrid();
        
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
                        if (r < p.KernelRadius) p.neighbors.Add(q);
                    }
                }
            }
        }
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

            foreach (Particle q in p.neighbors)
            {
                float r = Vector2.Distance(p.position, q.position);
                if (r < p.KernelRadius)
                {
                    float closeness = 1 - r / p.KernelRadius;
                    p.density += closeness * closeness;
                    p.density_near += closeness * closeness * closeness;

                    // Store neighbor information
                    neighborUnitX.Add((q.position.x - p.position.x) / r);
                    neighborUnitY.Add((q.position.y - p.position.y) / r);
                    neighborcloseness.Add(closeness);
                }
            }

            float pressure = p.Stiffness * (p.density - p.RestDensity);
            float nearPressure = p.NearStiffness * p.density_near;

            if (pressure > 1f) pressure = 1f;
            if (nearPressure > 1f) nearPressure = 1f;

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
            p.position += disp; // I DON'T LIKE MODIFICATING THE POSITION HERE
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

    void drawVelocity()
    {
        foreach(Particle p in particles)
        {
            p.drawVelocity();
        }
    }
    void drawDensity()
    {
        foreach (Particle p in particles)
        {
            p.drawDensity();
        }
    }

    void applyBodyForce()
    {
        // Apply body force to the particles
        foreach (Particle p in particles)
        {
            p.acceleration += new Vector2(0, -G);
        }
    }

    void computePressure()
    {
        foreach (Particle p in particles)
        {
            // EQUATION OF STATE
            const float k = 0.04f;
            p.pressure = p.density<0 ? k * (Mathf.Pow(p.RestDensity / p.density, 7f) - 1f):0f;
        }
    }

    void computeDensity()
    {
        foreach (Particle p in particles)
        {
            p.density = 0f;
            foreach (Particle q in p.neighbors)
            {
                p.density += Kernel(p.position, q.position);
            }
        }
    }

    void applyPressure()
    {
        foreach (Particle p in particles)
        {
            Vector2 pressureAcc = Vector2.zero;
            foreach (Particle q in p.neighbors)
            {
                float coefficient = p.pressure / Mathf.Pow(p.density, 2) + q.pressure / Mathf.Pow(q.density, 2);
                pressureAcc +=  coefficient * gradientKernel(p.position, q.position);
            }
            p.acceleration += pressureAcc;
        }
    }


    // Algorithm 1 : Simulation Step
    void Update()
    {
        // Update the grid and neighbors
        buildNeighbors();
        computeDensity();
        // GRAVITY
        applyBodyForce();

        // // PRESSURE
        // computePressure();
        // applyPressure();

        // // VISCOSITY
        // ViscosityImpulse();



        // SPRING 
        // SpringAdjustment();
        // SpringDisplacement();
        // DoubleDensityRelaxation();



        updateVelocity();
        updatePosition();
        ResolveCollisions();
        foreach (Particle p in particles) p.UpdateState();

        if (showVelocity) drawVelocity();
        drawDensity();
        // Debug.Log("Update is running");
        
    }
}
