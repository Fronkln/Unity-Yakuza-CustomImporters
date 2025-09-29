using UnityEngine;
using Yarhl.IO;

public static class DataWriterExtensions
{
    public static int Align(this DataWriter writer, int alignment)
    {
        int mod = (int)writer.Stream.Position % alignment;

        if (mod == 0)
            return 0;

        int neededBytes = alignment - mod;

        writer.WriteTimes(0, neededBytes);
        return neededBytes;
    }

    public static void WritePXDVector3(this DataWriter writer, PXDVector3 vec)
    {
        writer.Write(vec.x);
        writer.Write(vec.y);
        writer.Write(vec.z);
    }

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
