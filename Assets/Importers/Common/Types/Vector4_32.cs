using UnityEngine;

/// <summary>
/// A Vector4 represented with 4 bytes (why???)
/// </summary>
public class Vector4_32
{
    public byte x;
    public byte y;
    public byte z;
    public byte w;

    public Vector4_32(byte x, byte y, byte z, byte w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static implicit operator Vector4(Vector4_32 vec)
    {
        float x = (((float)vec.x) - 128.0f) / 128.0f;
        float y = (((float)vec.y) - 128.0f) / 128.0f;
        float z = (((float)vec.z) - 128.0f) / 128.0f;
        float w = (((float)vec.w) - 128.0f) / 128.0f;

        return new Vector4(x, y, z, w);
    }
}
