using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
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
    private uint m_triangleShapeCount;
    private uint m_quadShapeCount;
    private uint m_aaboxPtr;
    private uint m_unkRegion2Ptr;
    private uint m_unkRegion3Ptr;

    private SCTShape[] AllShapes
    {
        get
        {
            List<SCTShape> shapes = new List<SCTShape>();
            shapes.AddRange(m_header.TriangleShapes);
            shapes.AddRange(m_header.QuadShapes);

            return shapes.ToArray();
        }
    }

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
        ReadSphereBounds();
        ReadUnknownRegion2();
        ReadUnknownRegion3();
    }
    private void ReadHeader()
    {
        m_header = new SCTHeader();
        m_header.Magic = m_reader.ReadString(4);
        m_header.Endian = m_reader.ReadUInt32();
        m_reader.Endianness = m_header.Endian == 258 ? EndiannessMode.BigEndian : EndiannessMode.LittleEndian;

        m_header.Flags = m_reader.ReadInt32();
        m_reader.Stream.Position += 4;

        m_header.Unknown = m_reader.ReadInt32();
        m_triangleShapeCount = m_reader.ReadUInt32();
        m_vertexCount = m_reader.ReadInt32();
        m_header.Unknown2 = m_reader.ReadInt32();

        m_shapePtr = m_reader.ReadUInt32();
        m_vertexPtr = m_reader.ReadUInt32();
        m_quadShapeCount = m_reader.ReadUInt32();

        m_reader.Stream.Position += 8;
        m_aaboxPtr = m_reader.ReadUInt32();
        m_unkRegion2Ptr = m_reader.ReadUInt32();
        m_unkRegion3Ptr = m_reader.ReadUInt32();

        m_header.Bounds = new SCTSphereBounds() { Center = m_reader.ReadVector3(), Radius = m_reader.ReadSingle() };
        m_header.UnknownRegion = m_reader.ReadBytes(48);

        //REAL shape count?
        //m_shapeCount = (aaboxPtr - m_shapePtr) / 32;
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

        //m_shapeCount = (m_)

        m_header.TriangleShapes = new SCTShape[m_triangleShapeCount];

        for (int i = 0; i < m_header.TriangleShapes.Length; i++)
        {
            SCTShape shape = new SCTShape();
            shape.Type = GCTShapeType.Triangle;
            shape.Normal = new Vector3(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
            shape.Unknown = m_reader.ReadInt32();

            shape.Indices = new uint[3];

            for(int k = 0; k < shape.Indices.Length; k++)
                shape.Indices[k] = m_reader.ReadUInt32();

            shape.Flags = m_reader.ReadUInt32();

            m_header.TriangleShapes[i] = shape;
        }

        m_header.QuadShapes = new SCTShape[m_quadShapeCount];

        for (int i = 0; i < m_header.QuadShapes.Length; i++)
        {
            SCTShape shape = new SCTShape();
            shape.Type = GCTShapeType.Quad;
            shape.Normal = new Vector3(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
            shape.Unknown = m_reader.ReadInt32();
            /*
            shape.Indices = new uint[4];
            shape.Indices[0] = m_reader.ReadUInt32();
            shape.Indices[1] = m_reader.ReadUInt16();
            shape.Indices[2] = m_reader.ReadUInt16();
            //super weird value: at a first glance, seems to point to a triangle shape?
            ushort triangleShapePtr = m_reader.ReadUInt16();
            shape.Indices[3] = m_reader.ReadUInt16();
            */

            shape.Indices = new uint[4];

            //RGG moment
            //m_reader.Stream.Position += 2;
            ushort flags = m_reader.ReadUInt16();
            shape.Indices[0] = m_reader.ReadUInt16();

            ushort flags2 = m_reader.ReadUInt16();


            if (flags2 == 768)
            {
                m_reader.Stream.Position += 2;
                shape.Indices[1] = shape.Indices[0] + 1;
            }
            else
            {
                shape.Indices[1] = m_reader.ReadUInt16();
            }

            //m_reader.Stream.Position += 2;
            //shape.Indices[1] = m_reader.ReadUInt16();

            m_reader.Endianness = m_reader.Endianness = EndiannessMode.LittleEndian;
            shape.Indices[2] = m_reader.ReadUInt16();
            m_reader.Endianness = m_reader.Endianness = EndiannessMode.BigEndian;

            if (flags2 == 768)
            {
                m_reader.Stream.Position += 2;
                shape.Indices[3] = shape.Indices[2] + 1;
            }
            else
            {
                shape.Indices[3] = m_reader.ReadUInt16();
            }

            if (flags2 == 1024)
                shape.Indices[2] = shape.Indices[3] + 1;

            if (flags2 == 256)
            {
                shape.Indices[2] = shape.Indices[0] - 1;
            }


            shape.Flags = m_reader.ReadUInt32();
            shape.IndiceFlags = flags2;

            m_header.QuadShapes[i] = shape;
        }
    }

    //Unlike GCT, SCT uses spheres for its collision
    private void ReadSphereBounds()
    {
        m_reader.Stream.Seek(m_aaboxPtr);

        SCTShape[] allShapes = AllShapes;

        for(int i = 0; i < AllShapes.Length; i++)
        {
            allShapes[i].SphereBounds.Center = m_reader.ReadVector3();
            allShapes[i].SphereBounds.Radius = m_reader.ReadSingle();
        }
    }

    private void ReadUnknownRegion2()
    {
        m_reader.Stream.Seek(m_unkRegion2Ptr);

        SCTShape[] allShapes = AllShapes;

        for (int i = 0; i < AllShapes.Length; i++)
        {
            allShapes[i].UnknownData = new int[4] {m_reader.ReadInt32(), m_reader.ReadInt32(), m_reader.ReadInt32(), m_reader.ReadInt32() };
        }
    }

    private void ReadUnknownRegion3()
    {
        m_reader.Stream.Seek(m_unkRegion3Ptr);

        SCTShape[] allShapes = AllShapes;

        for (int i = 0; i < AllShapes.Length; i++)
        {
            allShapes[i].UnknownValue = m_reader.ReadInt32();
        }
    }
}