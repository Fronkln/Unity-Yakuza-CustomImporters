using UnityEngine;
using Yarhl.IO;

public static class DataWriterExtensions
{
    public static void WriteVector3(this DataWriter writer, Vector3 vec)
    {
        writer.Write(vec.x);
        writer.Write(vec.y);
        writer.Write(vec.z);
    }

    public static void WriteVector4(this DataWriter writer, Vector4 vec)
    {
        writer.Write(vec.x);
        writer.Write(vec.y);
        writer.Write(vec.z);
        writer.Write(vec.w);
    }

    public static void WriteGCTAABox(this DataWriter writer, GCTAABox box)
    {
        WriteVector4(writer, box.Center);
        WriteVector3(writer, box.Extents);
        writer.Write(box.HitFilter);
    }
}
