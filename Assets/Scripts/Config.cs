using UnityEngine;

public class Config : MonoBehaviour
{
    [Header("Time & Simulation Bounds")]
    public float DT = 0.04f; // Now a public field, can be changed
    public float X_MIN = -20f;
    public float X_MAX = 20f;
    public float Y_MIN = -10f;
    public float Y_MAX = 10f;
    public int GRID_SIZE_X = 60;
    public int GRID_SIZE_Y = 30;

    [Header("SPH Constants")]
    public float KERNEL_RADIUS = 0.5f;
    public float G = 9.81f;
    public float REST_DENSITY = 10f;
    public float COLLISION_RADIUS = 0.25f;

    [Header("Pressure & Viscosity")]
    public float PRESSURE_K = 1f;
    public float NEAR_PRESSURE_K = 10f;
    public float MAX_PRESSURE = 1f;
    public float LIN_VISCOSITY = 0.1f; // Linear viscosity coefficient
    public float QUAD_VISCOSITY = 0.1f; // Quadratic viscosity coefficient

    [Header("Collision & Damping")]
    public float DAMPING = 0.8f; // Damping factor for collision velocity response

    public static Config Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}
