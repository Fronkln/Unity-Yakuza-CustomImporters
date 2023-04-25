using PlasticPipe.PlasticProtocol.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarhl.IO;

public class GCTReader
{   

    public static GCTHeader Read(DataReader reader)
    {
        GCTHeader header = new GCTHeader();
        header.Magic = reader.ReadString(4);
        header.Endian = reader.ReadUInt32();
        reader.Endianness = header.Endian == 511 ? EndiannessMode.BigEndian : EndiannessMode.LittleEndian;

        header.Flags = reader.ReadInt32();
        header.FileSize = reader.ReadUInt32();

        return header;
    }
}
