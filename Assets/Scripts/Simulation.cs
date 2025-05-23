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
    public float DT = Config.DT; // The time step for the simulation

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

    float cubicSpline(float r)
    {
        if(0<=r && r<1) return 1 - 3 / 2 * r * r + 3/4 * r * r * r;
        else if(1<=r && r<2) return 4 - 6 * r + 3 * r * r;
        else return 0;
    }

    float cubicSplineGradient(float r)
    {
        if(0<=r && r<1) return -3 * r + 9/4 * r * r;
        else if(1<=r && r<2) return -3/4 * (2-r) * (2-r);
        else return 0;
    }

    float Kernel(Vector2 p, Vector2 q)
    {
        float alpha = 10 / (7 * Mathf.PI * Mathf.Pow(KERNEL_RADIUS, 3));
        return alpha * cubicSpline(Vector2.Distance(p, q) / KERNEL_RADIUS);
    }

    Vector2 gradientKernel(Vector2 p, Vector2 q)
    {
        float alpha = 10 / (7 * Mathf.PI * Mathf.Pow(KERNEL_RADIUS, 3));
        float r = Vector2.Distance(p, q);
        if (r < 0.0001f) return Vector2.zero; // Avoid division by zero
        return alpha / KERNEL_RADIUS * (p - q) / r * cubicSplineGradient(r / KERNEL_RADIUS);
    }

    Vector2 gradientKernel(Vector2 rij)
    {
        float r = rij.magnitude;
        if (r < 0.0001f) return Vector2.zero; // Avoid division by zero
        float alpha = 10 / (7 * Mathf.PI * Mathf.Pow(KERNEL_RADIUS, 3));
        return alpha / KERNEL_RADIUS * rij / r * cubicSplineGradient(r / KERNEL_RADIUS);
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
            const float k = 0.01f;
            p.pressure = p.density>0 ? k * (Mathf.Pow(p.density/p.RestDensity, 7f) - 1f):0f;
        }
    }

    void computeDensity()
    {
        foreach (Particle p in particles)
        {
            p.density = 0f;
            foreach (Particle q in p.neighbors)
            {
                p.density += q.mass * Kernel(p.position, q.position);
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
                // Debug.Log("Gradient Kernel = "+ gradientKernel(p.position, q.position));
                float coefficient = p.pressure / Mathf.Pow(p.density, 2) + q.pressure / Mathf.Pow(q.density, 2);
                // Debug.Log("Coeff = "+ coefficient);
                // Debug.Log("Pressure Acc p = "+ p.pressure);
                // Debug.Log("Pressure Acc q = "+ q.pressure);
                Vector2 rij = p.position - q.position;
                pressureAcc += q.mass * coefficient * gradientKernel(rij);
            }
            p.acceleration += pressureAcc;
            Debug.Log("Acceleration of particle : "+ p.acceleration);
        }
    }


    // Algorithm 1 : Simulation Step
    void FixedUpdate()
    {
        Debug.DrawLine(Vector3.zero, Vector3.right * 2f, Color.green);

        if (isPauses) return;
        // Update the grid and neighbors
        buildNeighbors();
        computeDensity();
        // GRAVITY
        applyBodyForce();

        // // PRESSURE
        computePressure();
        applyPressure();

        // // VISCOSITY
        // ViscosityImpulse();



        updateVelocity();
        updatePosition();
        ResolveCollisions();
        foreach (Particle p in particles) p.UpdateState();

        if (showVelocity) drawVelocity();
        drawDensity();
        // Debug.Log("Update is running");
        
    }
}
