using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public static Vector3 RotateVector(Vector3 original, float angle)
    {
        Vector3 newVector = new Vector3(
            original.x * Mathf.Cos(angle) - original.z * Mathf.Sin(angle),
            original.y,
            original.x * Mathf.Sin(angle) + original.z * Mathf.Cos(angle)
            );
        return newVector;
    }

    public static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}