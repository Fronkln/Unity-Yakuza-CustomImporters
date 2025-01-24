using System.Collections.Generic;
using UnityEngine;

public class SCTExportData : MonoBehaviour
{
    [Header("Required")]
    public Mesh Mesh;
    public SphereCollider BoundingSphere;

    public uint Flags;
    public int Unknown;
    public int[] UnknownData = new int[4] { -1, -1, -1, -1 };
    public int UnknownValue;

    public Vector3 Normal;

    public virtual SCTExportOutput[] Export()
    {
        MeshTopology topology = Mesh.GetTopology(0);

        SCTExportOutput output = new SCTExportOutput();
        output.Flags = Flags;
        output.Unknown = Unknown;
        output.UnknownData = UnknownData;
        output.UnknownValue = UnknownValue;

        output.Vertices = Mesh.vertices;

        output.Bounds.Center = BoundingSphere.center;
        output.Bounds.Radius = BoundingSphere.radius;

        if(topology == MeshTopology.Triangles)
        {
            output.Type = GCTShapeType.Triangle;
        }
        else if(topology == MeshTopology.Quads)
        {
            output.Type = GCTShapeType.Quad;
        }

        Vector3 side1 = output.Vertices[1] - output.Vertices[0];
        Vector3 side2 = output.Vertices[2] - output.Vertices[0];
        Vector3 normal = Vector3.Cross(side2, side1).normalized;


        output.Normal = -normal;

        return new SCTExportOutput[] { output };
    }
}
