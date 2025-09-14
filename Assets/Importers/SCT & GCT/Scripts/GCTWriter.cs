using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Yarhl.IO;
public static class GCTWriter
{
    public static void Write(GCTHeader gctFile, bool oeGCT, string path)
    {
        DataWriter writer = new DataWriter(new DataStream()) { Endianness = EndiannessMode.BigEndian };
        writer.Write("GCTD", false);

        if (oeGCT)
            writer.Write((uint)4278255616);
        else
            writer.Write((uint)4278190080);

        writer.Endianness = (!oeGCT ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian);

        writer.Write(gctFile.Flags);
        writer.Write(0); //Filesize, return later

        //return to these later
        long nodePtrPos = writer.Stream.Position;
        writer.WriteTimes(0, 8);

        long shapePtrPos = writer.Stream.Position;
        writer.WriteTimes(0, 8);

        long nodeAaBoxPtrPos = writer.Stream.Position;
        writer.WriteTimes(0, 4);

        long shapeAaBoxPtrPos = writer.Stream.Position;
        writer.WriteTimes(0, 4);

        long vectorPtrPos = writer.Stream.Position;
        writer.WriteTimes(0, 8);

        writer.Write(gctFile.HitFilter);
        writer.Write(gctFile.NodeDepth);
        writer.WriteTimes(0, 8);

        //Write bounds
        writer.Write(gctFile.Bounds.Center.x);
        writer.Write(gctFile.Bounds.Center.y);
        writer.Write(gctFile.Bounds.Center.z);
        writer.Write(gctFile.Bounds.Extents.magnitude);
        writer.Write(gctFile.Bounds.Extents.x);
        writer.Write(gctFile.Bounds.Extents.y);
        writer.Write(gctFile.Bounds.Extents.z);
        writer.Write(gctFile.Bounds.HitFilter);

        writer.WriteTimes(0, 36);

        writer.Write(0);
        writer.Write(gctFile.Shapes.Length);
        writer.WriteTimes(0, 84);
        writer.WriteOfType(gctFile.Name);

        uint nodeStart = (uint)writer.Stream.Position;
        writer.Write(gctFile.Shapes.Length);
        writer.WriteTimes(0, 124);


        uint shapeStart = (uint)writer.Stream.Position;

        foreach(GCTShape shape in gctFile.Shapes)
        {
            writer.Write(shape.Header.Flags);
            writer.Write(shape.Header.Attributes);

            shape.WriteData(writer);
        }

        writer.WriteTimes(0, 64);


        uint nodeAABoxStart = (uint)writer.Stream.Position;

        foreach (GCTAABox box in gctFile.NodeAABoxes)
            writer.WriteGCTAABox(box);

        uint shapeAABoxStart = (uint)writer.Stream.Position;

        foreach (GCTAABox box in gctFile.ShapeAABoxes)
            writer.WriteGCTAABox(box);

        writer.WriteTimes(0, 32);

        uint verticesStart = (uint)writer.Stream.Position;

        foreach (Vector3 vec in gctFile.Vertices)
         writer.WriteVector3(vec);

        writer.WriteTimes(0, 36);

        writer.Stream.Position = nodePtrPos;
        writer.Write(nodeStart);
        writer.Write(gctFile.NodeAABoxes.Length);

        writer.Stream.Position = shapePtrPos;
        writer.Write(shapeStart);
        writer.Write(gctFile.Shapes.Length);

        writer.Stream.Position = nodeAaBoxPtrPos;
        writer.Write(nodeAABoxStart);

        writer.Stream.Position = shapeAaBoxPtrPos;
        writer.Write(shapeAABoxStart);

        writer.Stream.Position = vectorPtrPos;
        writer.Write(verticesStart);

        writer.Stream.Position = 12;
        writer.Write((uint)writer.Stream.Length);
        writer.Stream.WriteTo(path);
    }
}
