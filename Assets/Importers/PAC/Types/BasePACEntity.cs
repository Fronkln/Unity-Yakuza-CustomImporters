using UnityEngine;
using Yarhl.IO;

public class BasePACEntity
{
    public ushort Type;
    public ushort ID;

    public PACEntityCCC CCC;

    public Vector3 Position;
    public ushort RotY;

    public byte[] UnknownData;

    internal virtual void ProcessEntityData(DataReader reader, string[] stringTable)
    {
        Position = reader.ReadVector3();
        RotY = reader.ReadUInt16();

        reader.SkipAhead(2);
    }
}
