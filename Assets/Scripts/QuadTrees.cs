using UnityEngine;


public class QuadTree
{
    // Attributes of a quadtree node

    public Vector2 topLeft;      // Top-left corner of this node
    public Vector2 bottomRight;  // Bottom-right corner of this node
    
    public float size;       // Width/height of this node (assuming square region)
    public int capacity;     // Maximum number of particles before subdivision
    public Particle[] particles; // Particles contained in this node
    public bool divided;     // Has this node been subdivided?
    public QuadTree northeast, northwest, southeast, southwest; // Children nodes
    public QuadTree parent; // Parent node


    public bool IsLeaf()
    {
        return !divided;
    }

    public QuadTree(Vector2 topLeft, Vector2 bottomRight, float size, int capacity)
    {
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
        this.size = size;
        this.capacity = capacity;
        this.particles = new Particle[capacity];
        this.divided = false;
        this.northeast = null;
        this.northwest = null;
        this.southeast = null;
        this.southwest = null;
    }

    public bool Insert(Particle particle)
    {
        // // Insert a particle into the quadtree
        // if(capacity > particles.Count())
        // {
        //     particles[particles.Count()] = particle;
        //     particles.Count();
        //     return true;
        // }
        // else if (!divided) Subdivide();

        // // Try to insert into one of the children
        // if (northeast.Insert(particle)) return true;
        // if (northwest.Insert(particle)) return true;
        // if (southeast.Insert(particle)) return true;
        // if (southwest.Insert(particle)) return true;
        return false;
    }

    public void Subdivide(Vector2 position)
    {
        // Subdivide this node into four children
        // Implementation goes here
    }

    public bool Contains(Vector2 point)
    {
        // Check if a point is within this node's bounds
        // Implementation goes here
        return false;
    }

    public void Query(Rect area, System.Collections.Generic.List<Particle> found)
    {
        // Find all particles within a given area
        // Implementation goes here
    }

    public void Clear()
    {
        // Remove all particles and child nodes
        // Implementation goes here
    }

    public bool MoveParticleUp(Vector2 position)
    {
        // Move the particle at the given position up (increase y by 1 unit)
        return true;
    }


}
