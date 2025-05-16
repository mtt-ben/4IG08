using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Particle : MonoBehaviour
{
    // Particle fields
    public Vector2 velocity = Vector2.zero;
    public Vector2 position;
    public Vector2 prevPosition;
    public List<(float, float)> springs;
    public bool inPlayer;
    public float density = 0f;
    public float density_near = 0f;
    public float pressure = 0f;

    // Particle properties
    public float RestDensity
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_REST_DENSITY;
            else
                return Config.REST_DENSITY;
        }
    }
    public float Stiffness
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_STIFFNESS;
            else
                return Config.STIFFNESS;
        }
    }
    public float NearStiffness
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_STIFFNESS_NEAR;
            else
                return Config.STIFFNESS_NEAR;
        }
    }
    public float KernelRadius
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_KERNEL_RADIUS;
            else
                return Config.KERNEL_RADIUS;
        }
    }
    public float SpringStiffness
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_SPRING_STIFFNESS;
            else
                return Config.SPRING_STIFFNESS;
        }
    }
    public float PLASTICITY // alpha
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_PLASTICITY;
            else
                return Config.PLASTICITY;
        }
    }
    public float YIELD_RATIO // gamma
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_YIELD_RATIO;
            else
                return Config.YIELD_RATIO;
        }
    }
    public float MinDistRatio
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_MIN_DIST_RATIO;
            else
                return Config.MIN_DIST_RATIO;
        }
    }
    public float LinViscosity
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_LIN_VISCOSITY;
            else
                return Config.LIN_VISCOSITY;
        }
    }
    public float QuadViscosity
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_QUAD_VISCOSITY;
            else
                return Config.QUAD_VISCOSITY;
        }
    }
    public float MaxPressure
    {
        get
        {
            if (inPlayer)
                return Config.INPLAYER_MAX_PRESSURE;
            else
                return Config.MAX_PRESSURE;
        }
    }
    
    void Start()
    {
        return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
