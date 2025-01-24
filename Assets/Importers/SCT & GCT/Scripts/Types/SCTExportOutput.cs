using UnityEngine;

public struct SCTExportOutput
{
    public GCTShapeType Type;

    public Vector3[] Vertices;
    public int[] Indices;
    public Vector3 Normal;
    public SCTSphereBounds Bounds;

    public uint Flags;
    public int Unknown;
    public int[] UnknownData;
    public int UnknownValue;
}
