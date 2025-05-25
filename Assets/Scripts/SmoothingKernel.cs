using System.Collections.Generic;
using UnityEngine;

using static Config;

public class SmoothingKernel
{
    public float smoothingLen = KERNEL_RADIUS;
    public float supportRadius = 2*KERNEL_RADIUS;
    public float alpha = 10 / (7 * Mathf.PI * Mathf.Pow(KERNEL_RADIUS,2));

    public SmoothingKernel() { }

    public float cubicSpline(float r)
    {
        if (0 <= r && r < 1) return 1 - 3 / 2 * r * r + 3 / 4 * r * r * r;
        else if (1 <= r && r < 2) return 1 / 4 * Mathf.Pow(2 - r, 3);
        else return 0;
    }

    public float cubicSplineGradient(float r)
    {
        if (0 <= r && r < 1) return -3 * r + 9 / 4 * r * r;
        else if (1 <= r && r < 2) return -3 / 4 * Mathf.Pow(2 - r, 2);
        else return 0;
    }

    public float W(Vector2 r) {
        float rMag = r.magnitude;
        if (rMag > supportRadius || rMag < 0.001f) return 0;
        return alpha * cubicSpline(rMag / smoothingLen);
    }

    public float W(Vector2 xi, Vector2 xj) {
        return W(xi - xj);
    }

    public Vector2 grad_W(Vector2 r) {
        float rMag = r.magnitude;
        if (rMag > supportRadius || rMag<0.001f) return Vector2.zero;
        return alpha * r * cubicSplineGradient(rMag / smoothingLen) / (smoothingLen* rMag);
    }
    public Vector2 grad_W(Vector2 xi, Vector2 xj)
    {
        return grad_W(xi - xj);
    }
   
}
