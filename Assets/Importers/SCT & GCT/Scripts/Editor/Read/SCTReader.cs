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
    private int m_vertexCount;
    private int m_unknown4;
    private uint m_shapePtr;
    private uint m_vertexPtr;
    private uint m_shapeCount;


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
        ReadShapes();
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
        m_vertexCount = m_reader.ReadInt32();
        m_unknown4 = m_reader.ReadInt32();

        m_shapePtr = m_reader.ReadUInt32();
        m_vertexPtr = m_reader.ReadUInt32();
        m_shapeCount = m_reader.ReadUInt32();
    }

    private void ReadVertices()
    {
        m_reader.Stream.Seek(m_vertexPtr, SeekMode.Start);

        m_header.Vertices = new Vector3[m_vertexCount];

        for (int i = 0; i < m_vertexCount; i++)
            m_header.Vertices[i] = new Vector3(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
    }

    private void ReadShapes()
    {
        m_reader.Stream.Seek(m_shapePtr, SeekMode.Start);

        m_header.Shapes = new SCTShape[m_shapeCount];

        for(int i = 0; i < m_header.Shapes.Length; i++)
        {
            SCTShape shape = new SCTShape();
            shape.Normal = new Vector3(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
            shape.Unknown = m_reader.ReadInt32();

            shape.Indices = new ushort[4];

            //RGG moment
            m_reader.Stream.Position += 2;
            shape.Indices[0] = m_reader.ReadUInt16();

            m_reader.Stream.Position += 2;
            shape.Indices[1] = m_reader.ReadUInt16();

            m_reader.Endianness = m_reader.Endianness = EndiannessMode.LittleEndian;
            shape.Indices[2] = m_reader.ReadUInt16();
            m_reader.Endianness = m_reader.Endianness = EndiannessMode.BigEndian;

            shape.Indices[3] = m_reader.ReadUInt16();

            shape.Flags = m_reader.ReadUInt32();

            m_header.Shapes[i] = shape;
        }
    }
}
