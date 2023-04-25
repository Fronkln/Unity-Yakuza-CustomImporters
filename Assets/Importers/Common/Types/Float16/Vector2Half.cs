using System;
using UnityEngine;

public struct Vector2Half
{
    public Half x;
    public Half y;

    public Vector2Half(Half x, Half y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2(Vector2Half halfVec)
    {
        return new Vector2(halfVec.x, halfVec.y);
    }

    public static implicit operator Vector2Half(Vector2 vec2)
    {
        return new Vector2Half(new Half(vec2.x),  new Half(vec2.y));
    }
}
