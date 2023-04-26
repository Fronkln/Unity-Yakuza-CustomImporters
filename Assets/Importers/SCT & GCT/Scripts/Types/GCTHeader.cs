using UnityEngine;

public class GCTHeader
{
    public string Magic; //GCTD
    public uint Endian;
    public int Flags;
    public uint FileSize;

    public uint HitFilter;
    public uint NodeDepth;
    public Bounds Bounds;

    public PXDHash Name = new PXDHash();

    public GCTShape[] Shapes;
    public GCTAABox[] NodeAABoxes;
    public GCTAABox[] ShapeAABoxes;
    public Vector3[] Vertices;
    public int[] Indices;
}
