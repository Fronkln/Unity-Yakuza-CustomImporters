using System;
using UnityEngine;
using Yarhl.IO;

[Serializable]
public class BasePACEntityDataY5
{
    public Vector3 Position;
    public short Angle;
    public byte Data2Count;
    public byte Flags;

    public byte[] UnreadData;

    public virtual void ReadData(DataReader reader)
    {

    }

    public virtual void WriteData(DataWriter writer)
    {

    }

    public static BasePACEntityDataY5 Read(DataReader reader, byte propertyType, int size)
    {
        BasePACEntityDataY5 data = null;

        switch(propertyType)
        {
            default:
                data = new BasePACEntityDataY5();
                break;
        }

        long dataStart = reader.Stream.Position;
        long dataEnd = reader.Stream.Position + size;

        data.Position = reader.ReadVector3();
        data.Angle = reader.ReadInt16();
        data.Data2Count = reader.ReadByte();
        data.Flags = reader.ReadByte();

        data.ReadData(reader);

        long postReadPos = reader.Stream.Position;

        if (postReadPos < dataEnd)
            data.UnreadData = reader.ReadBytes((int)(dataEnd - postReadPos));
        else if (postReadPos > dataEnd)
            throw new System.Exception("Overread");
        

        return data;
    }
}
