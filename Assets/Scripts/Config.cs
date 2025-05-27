using UnityEngine;

public class Config : MonoBehaviour
{
    //Simulation parameters
    public static float DT = 0.01f; // Time step
    public static float G = 0.5f; // Acceleration of gravity
    public static int GRID_SIZE_X = 60; // Number of grid cells in the x direction
    public static int GRID_SIZE_Y = 30; // Number of grid cells in the y direction
    public static float X_MIN = -20f; // Minimum x coordinate for the grid
    public static float X_MAX = 20f; // Maximum x coordinate for the grid
    public static float Y_MIN = -10f; // Minimum y coordinate for the grid
    public static float Y_MAX = 10f; // Maximum y coordinate for the grid

    // Normal water parameters
    public static float REST_DENSITY = 3f; // Default density, will be compared to local density to calculate pressure
    public static float KERNEL_RADIUS = 1f; // Spacing between particles, used to calculate pressure
    public static float COLLISION_RADIUS = 0.2f; // Collision radius, used to detect collisions with other particles
}
