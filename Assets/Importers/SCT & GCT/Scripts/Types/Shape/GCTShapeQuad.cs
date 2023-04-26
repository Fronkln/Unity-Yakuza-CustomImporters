using UnityEngine;
using Yarhl.IO;

public class GCTShapeQuad : GCTShapePrimitive
{
    public override void ReadData(DataReader reader)
    {
        Indices = new uint[4];

        for (int i = 0; i < Indices.Length; i++)
            Indices[i] = reader.ReadUInt32();

        NormalIndex = reader.ReadUInt32();
        Product = reader.ReadSingle();
    }

    public override void WriteData(DataWriter writer)
    {
        foreach (uint i in Indices)
            writer.Write(i);

        writer.Write(NormalIndex);
        writer.Write(Product);
    }
}
