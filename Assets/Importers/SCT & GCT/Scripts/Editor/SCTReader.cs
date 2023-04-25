using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarhl.IO;

public class SCTReader
{
    public static SCTHeader Read(DataReader reader)
    {
        SCTHeader header = new SCTHeader();
        header.Magic = reader.ReadString(4);
        header.Endian = reader.ReadUInt32();
        reader.Endianness = header.Endian == 258 ? EndiannessMode.BigEndian : EndiannessMode.LittleEndian;

        header.Flags = reader.ReadInt32();
        reader.Stream.Position += 4;

        return header;
    }
}
