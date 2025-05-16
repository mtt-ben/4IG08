using UnityEngine;

public class Particle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Initialize SPH particle state variables
        public float mass = 1.0f;
        public float density = 0.0f;
        public float pressure = 0.0f;
        public Vector3 velocity = Vector3.zero;
        public Vector3 force = Vector3.zero;
        public Vector3 position;
        
    void Start()
    {
        return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
