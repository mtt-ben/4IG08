using System.Collections.Generic;
using UnityEngine;

using list = System.Collections.Generic.List<Particle>;
using f_list = System.Collections.Generic.List<float>;

using NUnit.Framework.Constraints;
public class Simulation : MonoBehaviour
{
    public GameObject Player;
    public GameObject Base_Particle; // The reference base particle object
    public list particles; // The list of particles
    public list[,] grid; // The grid for spatial partitioning
    public list particlesToAdd = new(); // Particles to be added in the next step
    public int frameRate = 60;

    // Simulation parameters
    private float H => Config.Instance.KERNEL_RADIUS; // Kernel radius
    private float G => Config.Instance.G; // Gravitational acceleration
    private float X_min => Config.Instance.X_MIN; // Minimum x boundary
    private float X_max => Config.Instance.X_MAX; // Maximum x boundary
    private float Y_min => Config.Instance.Y_MIN; // Minimum y boundary
    private float Y_max => Config.Instance.Y_MAX; // Maximum y boundary
    private int Grid_size_x => Config.Instance.GRID_SIZE_X; // The number of grid cells in the x direction
    private int Grid_size_y => Config.Instance.GRID_SIZE_Y; // The number of grid cells in the y direction
    private float DT => Config.Instance.DT; // Time step for the simulation
    private float lin_visc => Config.Instance.LIN_VISCOSITY; // Linear viscosity coefficient
    private float quad_visc => Config.Instance.QUAD_VISCOSITY; // Quadratic viscosity coefficient

    void Start()
    {
        grid = new list[Grid_size_x, Grid_size_y];
        for (int i = 0; i < Grid_size_x; i++)
        {
            for (int j = 0; j < Grid_size_y; j++)
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

            int M = Mathf.CeilToInt(H / (X_max - X_min) * Grid_size_x);
            int L = Mathf.CeilToInt(H / (Y_max - Y_min) * Grid_size_y);

            if (M < 1) M = 1;
            if (L < 1) L = 1;

            for (int i = -M; i <= M; i++)
            {
                for (int j = -L; j <= L; j++)
                {
                    int gridX = p.grid_x + i;
                    int gridY = p.grid_y + j;
                    if (gridX < 0 || gridX >= Grid_size_x || gridY < 0 || gridY >= Grid_size_y) continue;
                    if (grid[gridX, gridY] == null) continue;

                    foreach (Particle neighbor in grid[gridX, gridY])
                    {
                        if (neighbor == p) continue;

                        float q = Vector2.Distance(p.position, neighbor.position) / H;
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

            float pressure = Config.Instance.PRESSURE_K * (p.density - Config.Instance.REST_DENSITY);
            float nearPressure = Config.Instance.NEAR_PRESSURE_K * p.density_near;

            if (pressure > Config.Instance.MAX_PRESSURE) pressure = Config.Instance.MAX_PRESSURE;
            if (nearPressure > Config.Instance.MAX_PRESSURE) nearPressure = Config.Instance.MAX_PRESSURE;

            Vector2 dx = Vector2.zero;

            for (int i = 0; i < neighbors.Count; i++)
            {
                Particle neighbor = neighbors[i];
                float q = qList[i];

                float closeness = 1 - q;
                float D_val = pressure * closeness + nearPressure * closeness * closeness;
                Vector2 dir = (p.position - neighbor.position).normalized;
                Vector2 displacement_for_pair = DT * DT * D_val * dir;
                neighbor.position += displacement_for_pair * 0.5f;
                dx -= displacement_for_pair * 0.5f;
            }

            p.position += dx;
        }
    }

    void ApplyViscosity()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            Particle p = particles[i];
            for (int j = i + 1; j < particles.Count; j++)
            {
                Particle neighbor = particles[j];
                if (p == neighbor) continue;

                float q = Vector2.Distance(p.position, neighbor.position) / H;
                if (q < 1f)
                {
                    float u = Vector2.Dot(p.velocity - neighbor.velocity, (p.position - neighbor.position).normalized);
                    float closeness = 1 - q;
                    Vector2 I = DT * closeness * (lin_visc * u + quad_visc * u * u) * (p.position - neighbor.position).normalized;
                    p.velocity -= I * 0.5f;
                    neighbor.velocity += I * 0.5f;
                }
            }
        }
    }

    void ResolveCollisions()
    {
        foreach (Particle p in particles)
        {
            p.ResolveAllCollisions();
        }
    }

    void Update()
    {
        Application.targetFrameRate = frameRate;
    }

    // Simulation Step
    void FixedUpdate()
    {
        // Apply gravity
        foreach (Particle p in particles)
        {
            p.velocity += new Vector2(0, -G * DT);
            p.collisionNormals.Clear();
        }

        ApplyViscosity();
        
        foreach (Particle p in particles)
        {
            p.prevPosition = p.position;
            p.position += p.velocity * DT;
        }

        ClearGrid();
        PopulateGrid();

        DoubleDensityRelaxation();
        ResolveCollisions();

        foreach (Particle p in particles)
        {
            p.velocity = (p.position - p.prevPosition) / DT;

            // Need to handle collisions after updating positions
            foreach (Vector2 normal in p.collisionNormals)
            {
                if (Vector2.Dot(p.velocity, normal) < 0f)
                {
                    p.velocity = Vector2.Reflect(p.velocity, normal) * Config.Instance.DAMPING;
                }
            }

            if (!float.IsNaN(p.position.x) && !float.IsNaN(p.position.y) && p != null)
                p.transform.position = p.position;
        }

        RemoveOutOfBoundsParticles();
        AddNewParticles();
    }

    void ClearGrid()
    {
        for (int i = 0; i < Grid_size_x; i++)
        {
            for (int j = 0; j < Grid_size_y; j++)
            {
                grid[i, j].Clear();
            }
        }
    }

    void PopulateGrid()
    {
        foreach (Particle p in particles)
        {
            // Calculate grid coordinates based on current particle position
            // Clamp to ensure they stay within bounds
            int gridX = Mathf.Clamp((int)((p.position.x - X_min) / (X_max - X_min) * Grid_size_x), 0, Grid_size_x - 1);
            int gridY = Mathf.Clamp((int)((p.position.y - Y_min) / (Y_max - Y_min) * Grid_size_y), 0, Grid_size_y - 1);

            // Assign to particle for later neighbor lookups
            p.grid_x = gridX;
            p.grid_y = gridY;

            // Add particle to the grid cell
            grid[gridX, gridY].Add(p);
        }
    }

    void RemoveOutOfBoundsParticles()
    {
        list particlesToRemove = new list();
        foreach (Particle p in particles)
        {
            if (p.position.x < X_min || p.position.x > X_max || p.position.y < Y_min || p.position.y > Y_max)
            {
                particlesToRemove.Add(p);
            }
        }

        foreach (Particle p in particlesToRemove)
        {
            if (p == null) continue; // Safety check
            RemoveParticle(p); // This already handles removing from the grid
            Destroy(p.gameObject);
        }
    }

    void AddNewParticles()
    {
        foreach (Particle p in particlesToAdd)
        {
            particles.Add(p);

            // When adding, ensure particle's grid_x/y are also set for the first time
            int gridX = Mathf.Clamp((int)((p.position.x - X_min) / (X_max - X_min) * Grid_size_x), 0, Grid_size_x - 1);
            int gridY = Mathf.Clamp((int)((p.position.y - Y_min) / (Y_max - Y_min) * Grid_size_y), 0, Grid_size_y - 1);

            grid[gridX, gridY].Add(p);
            p.grid_x = gridX;
            p.grid_y = gridY;
        }
        particlesToAdd.Clear();
    }

    public void RemoveParticle(Particle p)
    {
        particles.Remove(p);

        int gridX = Mathf.Clamp((int)((p.prevPosition.x - X_min) / (X_max - X_min) * Grid_size_x), 0, Grid_size_x - 1);
        int gridY = Mathf.Clamp((int)((p.prevPosition.y - Y_min) / (Y_max - Y_min) * Grid_size_y), 0, Grid_size_y - 1);

        if (grid[gridX, gridY].Contains(p))
        {
            grid[gridX, gridY].Remove(p);
        }
    }
    
    public void AddParticle(Particle p)
    {
        particlesToAdd.Add(p);
        p.gameObject.SetActive(true); // Ensure the particle is active
    }
}
