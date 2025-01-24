using System.Linq;
using UnityEngine;
using Yarhl.IO;

public class SCTWriter : MonoBehaviour
{
    public static void Write(SCTHeader sctFile, string path)
    {
        if (sctFile.QuadShapes.Any(x => x.IndiceFlags > 0))
            throw new System.Exception("SCT Exporter currently does not support collisions with optimized quads in it, sorry!");


        DataWriter writer = new DataWriter(new DataStream()) { Endianness =  EndiannessMode.BigEndian };
        writer.Write("SCTD", false);
        writer.Write(33619968);
        writer.Write(sctFile.Flags);
        writer.Write(0);
        writer.Write(sctFile.Unknown);

        long pointersRegion = writer.Stream.Position;

        //Fill it temporarily
        writer.WriteTimes(0, 108);


        SCTShape[] allShapes = sctFile.TriangleShapes.Concat(sctFile.QuadShapes).ToArray();

        long trianglesStart = writer.Stream.Position;

        foreach(SCTShape triangleShape in sctFile.TriangleShapes)
            triangleShape.Write(writer);
        
        long quadsStart = writer.Stream.Position;

        foreach (SCTShape quadShape in sctFile.QuadShapes)
            quadShape.Write(writer);

        long boundingSphereStart = writer.Stream.Position;

        foreach(SCTShape shape in allShapes)
        {
            writer.WriteVector3(shape.SphereBounds.Center);
            writer.Write(shape.SphereBounds.Radius);
        }

        long unkReg2Start = writer.Stream.Position;

        foreach(SCTShape shape in allShapes)
        {
            foreach (int i in shape.UnknownData)
                writer.Write(i);
        }

        long unkReg3Start = writer.Stream.Position;

        foreach (SCTShape shape in allShapes)
        {
            writer.Write(shape.UnknownValue);
        }

        long verticesStart = writer.Stream.Position;

        foreach (Vector3 vec in sctFile.Vertices)
            writer.WriteVector3(vec);

        writer.Stream.Position = pointersRegion;
        writer.Write(sctFile.TriangleShapes.Length);
        writer.Write(sctFile.Vertices.Length);
        writer.Write(sctFile.Unknown2);
        writer.Write((uint)trianglesStart);
        writer.Write((uint)verticesStart);
        writer.Write(sctFile.QuadShapes.Length);
        writer.WriteTimes(0, 8);
        writer.Write((uint)boundingSphereStart);
        writer.Write((uint)unkReg2Start);
        writer.Write((uint)unkReg3Start);

        writer.WriteVector3(sctFile.Bounds.Center);
        writer.Write(sctFile.Bounds.Radius);
        writer.Write(sctFile.UnknownRegion);

        writer.Stream.WriteTo(path);
    }
}
