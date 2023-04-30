using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMDMesh
{
    public uint Index;

    public uint AttributeIndex;
    public uint VertexBufferIndex;
    public uint ObjectIndex;
    public uint NodeIndex;

    public uint MinIndex;
    public uint VertexCount;
    public uint VertexOffsetFromIndex;

    public uint MatrixListOffset;
    public uint MatrixListLength;

    public IndicesStruct TriangleListIndicesData;
    public IndicesStruct NoResetStripIndicesData;
    public IndicesStruct ResetStripIndicesData;

    // Not part of the structure, parsed by reading algorithm
    public GMDVertexBuffer VertexBuffer;
    public uint VertexStart; // This mesh uses the vertices [VertexStart, VertexEnd) from VertexBuffer. The TriangleListIndices are relative to this range.
    public uint VertexEnd;
    public ushort[] TriangleListIndices;

}
