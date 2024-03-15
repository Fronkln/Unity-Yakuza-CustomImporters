using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OERotationY : MonoBehaviour
{
    public static float ToAngle(ushort oeRotation)
    {
        float angle = (oeRotation * 360f) / ushort.MaxValue;
        return angle;
    }

    public static ushort ToOERotation(ushort oeRotation)
    {
        ushort angle = (ushort)((oeRotation * ushort.MaxValue) / 360f);
        return angle;
    }
}
