using UnityEngine;

public class SCTHeader
{
    public string Magic; //SCTD
    public uint Endian;
    public int Flags;
    //int Padding

    public uint HitFilter;
    public uint NodeDepth;
    public Bounds Bounds;

    public Vector3[] Vertices;
}
