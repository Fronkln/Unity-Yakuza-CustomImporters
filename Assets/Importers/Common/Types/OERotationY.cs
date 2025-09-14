using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OERotationY
{
    public static float ToAngle(ushort oeRotation)
    {
        float angle = (oeRotation * 360f) / ushort.MaxValue;
        return angle;
    }

    public static ushort ToOERotation(float oeRotation)
    {
        ushort angle = (ushort)((oeRotation * ushort.MaxValue) / 360f);
        return angle;
    }
}
