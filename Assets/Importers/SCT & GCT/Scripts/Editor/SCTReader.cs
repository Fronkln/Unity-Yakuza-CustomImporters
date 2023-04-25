using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarhl.IO;

public class SCTReader
{
    private SCTHeader m_header;
    private DataReader m_reader;

    public static SCTHeader Read(DataReader reader)
    {
        SCTReader sctReader = new SCTReader();
        sctReader.m_reader = reader;
        sctReader.Read();

        return sctReader.m_header;
    }

    private void Read()
    {
        ReadHeader();
    }
    private void ReadHeader()
    {
        SCTHeader m_header = new SCTHeader();
        m_header.Magic = m_reader.ReadString(4);
        m_header.Endian = m_reader.ReadUInt32();
        m_reader.Endianness = m_header.Endian == 258 ? EndiannessMode.BigEndian : EndiannessMode.LittleEndian;

        m_header.Flags = m_reader.ReadInt32();
        m_reader.Stream.Position += 4;
    }
}
