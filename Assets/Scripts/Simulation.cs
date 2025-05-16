using System.Collections.Generic;
using UnityEngine;

using list = System.Collections.Generic.List<Particle>;

using static Config;
public class Simulation : MonoBehaviour
{
    public GameObject Player;
    public GameObject Base_Particle; // The reference base particle object
    public QuadTree particles_tree; // The QuadTree for spatial partitioning
    public list particles; // The list of particles

    void Start()
    {

    }

    static void PopulateQuadTree()
    /*
     * Populate the QuadTree with particles
     */
    {

    }

    static void DoubleDensityRelaxation()
    /*
     * Algorithm 2 : Double Density Relaxation
     */
    {

    }

    static void SpringDisplacement()
    /*
     * Algorithm 3 : Spring Displacement
     */
    {

    }

    static void SpringAdjustment()
    /*
     * Algorithm 4 : Spring Adjustment
     */
    {
        // done in the springDisplacement function
    }

    static void ResolveCollisions()
    /*
     * Algorithm 6 : Particle-body interactions
     */
    {

    }

    static void ViscosityImpulse()
    /*
     * Algorithm 5 : Viscosity Impulse
     */
    {

    }

    // Algorithm 1 : Simulation Step
    void Update()
    {
        PopulateQuadTree();

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
        }

        SpringAdjustment();
        SpringDisplacement();
        DoubleDensityRelaxation();
        ResolveCollisions();

        // Use previous position to compute next velocity
        foreach (Particle p in particles)
        {
            p.velocity = (p.position - p.prevPosition) / DT;
        }
    }
}
