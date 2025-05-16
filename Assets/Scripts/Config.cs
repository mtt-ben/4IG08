using UnityEngine;

public class Config : MonoBehaviour
{
    //Simulation parameters
    public static float DT = 20f; // Time step
    public static float G = 0.5f; // Acceleration of gravity

    // Two set of parameters are necessary for the game
    // Normal water parameters
    public static float REST_DENSITY = 3.0f; // Default density, will be compared to local density to calculate pressure
    public static float STIFFNESS = 0.1f; // Pressure factor
    public static float STIFFNESS_NEAR = STIFFNESS * 10f; // Near pressure factor, pressure when particles are close to each other
    public static float KERNEL_RADIUS = 0.08f; // Spacing between particles, used to calculate pressure
    public static float SPRING_STIFFNESS = 0f; // Spring stiffness
    public static float PLASTICITY = 0.5f; // alpha
    public static float YIELD_RATIO = 0.25f; // gamma
    public static float MIN_DIST_RATIO = 0.25f; // Minimum distance ratio
    public static float LIN_VISCOSITY = 0f; // Viscosity factor
    public static float QUAD_VISCOSITY = 0.1f; // Viscosity factor
    public static float MAX_PRESSURE = 1f; // Maximum pressure

    // inPlayer water parameters
    public static float INPLAYER_REST_DENSITY = 3.0f; // Default density, will be compared to local density to calculate pressure
    public static float INPLAYER_STIFFNESS = 0.1f; // Pressure factor
    public static float INPLAYER_STIFFNESS_NEAR = STIFFNESS * 10f; // Near pressure factor, pressure when particles are close to each other
    public static float INPLAYER_KERNEL_RADIUS = 0.08f; // Spacing between particles, used to calculate pressure
    public static float INPLAYER_SPRING_STIFFNESS = 0f; // Spring stiffness
    public static float INPLAYER_PLASTICITY = 0.5f; // alpha
    public static float INPLAYER_YIELD_RATIO = 0.25f; // gamma
    public static float INPLAYER_MIN_DIST_RATIO = 0.25f; // Minimum distance ratio
    public static float INPLAYER_LIN_VISCOSITY = 0f; // Viscosity factor
    public static float INPLAYER_QUAD_VISCOSITY = 0.1f; // Viscosity factor
    public static float INPLAYER_MAX_PRESSURE = 1f; // Maximum pressure
}
