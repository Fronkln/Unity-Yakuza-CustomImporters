using UnityEngine;
using Yarhl.IO;

[System.Serializable]
public class SCTShape
{
    public Vector3 Normal;
    public int Unknown = 0;
    public uint[] Indices;
    public uint Flags;
    public ushort IndiceFlags = 0;

    public SCTSphereBounds SphereBounds;
    public int[] UnknownData = new int[4] { -1, -1, -1, -1 };
    public int UnknownValue;

    public GCTShapeType Type;

    //Doesnt write everything, only the data that is included at the initial chunk
    public void Write(DataWriter writer)
    {
        writer.WriteVector3(Normal);
        writer.Write(Unknown);

        if(Type == GCTShapeType.Triangle)
        {
            foreach (uint i in Indices)
                writer.Write(i);
        }
        else
        {
            writer.WriteTimes(0, 2);
            writer.Write((ushort)Indices[0]);
            writer.Write(IndiceFlags);
            writer.Write((ushort)Indices[1]);
            writer.Endianness = EndiannessMode.LittleEndian;
            writer.Write((ushort)Indices[2]);
            writer.Endianness = EndiannessMode.BigEndian;
            writer.Write((ushort)Indices[3]);
        }

        writer.Write(Flags);
    }
}
