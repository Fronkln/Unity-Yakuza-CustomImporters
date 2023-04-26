using UnityEngine;
using Yarhl.IO;

public class GCTShapeTriangle : GCTShapePrimitive
{
    public override void ReadData(DataReader reader)
    {
        Indices = new uint[3];

        for (int i = 0; i < Indices.Length; i++)
            Indices[i] = reader.ReadUInt32();

        reader.ReadUInt32();

        NormalIndex = reader.ReadUInt32();
        Product = reader.ReadSingle();
    }

    public override void WriteData(DataWriter writer)
    {
        foreach (uint i in Indices)
            writer.Write(i);

        writer.Write(0);

        writer.Write(NormalIndex);
        writer.Write(Product);
    }
}
