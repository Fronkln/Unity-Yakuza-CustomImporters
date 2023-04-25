using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarhl.IO;

public class SCTReader
{
    private SCTHeader m_header;
    private DataReader m_reader;

    private int m_unknown1;
    private int m_unknown2;
    private int m_unknown3;
    private int m_unknown4;

    private SizedPointer m_vertexChunk;


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
        ReadVertices();
    }
    private void ReadHeader()
    {
        m_header = new SCTHeader();
        m_header.Magic = m_reader.ReadString(4);
        m_header.Endian = m_reader.ReadUInt32();
        m_reader.Endianness = m_header.Endian == 258 ? EndiannessMode.BigEndian : EndiannessMode.LittleEndian;

        m_header.Flags = m_reader.ReadInt32();
        m_reader.Stream.Position += 4;

        m_unknown1 = m_reader.ReadInt32();
        m_unknown2 = m_reader.ReadInt32();
        m_unknown3 = m_reader.ReadInt32();
        m_unknown4 = m_reader.ReadInt32();

        m_header.HitFilter = m_reader.ReadUInt32();

        m_vertexChunk = m_reader.Read<SizedPointer>();
    }

    private void ReadVertices()
    {
        m_reader.Stream.Seek(m_vertexChunk.Pointer, SeekMode.Start);

        long vertexStart = m_reader.Stream.Position;
        long vertexEnd = m_reader.Stream.Length;

        int numVertices = (int)((vertexEnd - vertexStart) / 12);

        m_header.Vertices = new Vector3[numVertices];

        for (int i = 0; i < numVertices; i++)
        {
            m_header.Vertices[i] = new Vector3(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
        }
    }
}
