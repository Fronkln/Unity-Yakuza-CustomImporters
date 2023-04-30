using UnityEngine;

public class SCTHeader
{
    public string Magic; //SCTD
    public uint Endian;
    public int Flags;
    //int Padding

    public Bounds Bounds;

    public SCTShape[] Shapes;
    public Vector3[] Vertices;
}
