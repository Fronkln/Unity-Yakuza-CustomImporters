using System;
using UnityEngine;

[Serializable]
public class SCTHeader
{
    public string Magic; //SCTD
    public uint Endian;
    public int Flags;
    //int Padding

    public int Unknown;
    public int Unknown2;

    public SCTSphereBounds Bounds;
    public byte[] UnknownRegion = new byte[48];

    [NonSerialized]
    public SCTShape[] TriangleShapes = new SCTShape[0];
    [NonSerialized]
    public SCTShape[] QuadShapes = new SCTShape[0];
    [NonSerialized]
    public Vector3[] Vertices = new Vector3[0];
}
