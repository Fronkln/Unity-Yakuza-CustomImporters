using System.Collections.Generic;
using UnityEngine;

public class GCTExportData : MonoBehaviour
{
    [Header("Required")]
    public Mesh Mesh;
    public BoxCollider AABox;
    public int AABoxHitFilter;

    [Header("Dont Change If You Have No Clue")]
    public GCTShapeHeader NodeHeader;
    public GCTShapeType Type;
    public bool GenerateNodeAABox;
    public GCTAABox NodeAABox;

    public GCTExportOutput Export()
    {
        if (!Validate())
            return new GCTExportOutput();

        GCTExportOutput output = new GCTExportOutput();

        List<Vector3> actualVertices = new List<Vector3>(Mesh.vertices);
        int normalIdx = actualVertices.Count - 1;



        for (int i = 0; i < normalIdx; i++)
        {
            Matrix4x4 localToWorld = transform.localToWorldMatrix;

            if(i < actualVertices.Count - 1)
                actualVertices[i] = localToWorld.MultiplyPoint3x4(Mesh.vertices[i]);
        }

        switch(Type)
        {
            case GCTShapeType.Triangle:
                if (actualVertices.Count == 3) //Unity triangle or something
                    actualVertices.Add(GenerateNormal());
                break;
            case GCTShapeType.Quad:
                if (actualVertices.Count == 4)//Unity Quad
                    actualVertices.Add(GenerateNormal());
                break;
        }

        //normalize normal
        actualVertices[actualVertices.Count - 1] = actualVertices[actualVertices.Count - 1].normalized;

        output.OutputAABox.Center = AABox.bounds.center;
        output.OutputAABox.Center.w = AABox.bounds.center.magnitude;
        output.OutputAABox.Extents = AABox.bounds.extents;
        output.OutputAABox.HitFilter = AABoxHitFilter;

        output.ShapeHeader = NodeHeader;
        output.Type = Type;
        output.Vertices = actualVertices.ToArray();
        output.Indices = Mesh.GetIndices(0);
        output.Product = Vector3.Dot(actualVertices[normalIdx], actualVertices[0]);
        output.GenerateNodeAABox = GenerateNodeAABox;
        output.OutputNodeAABox = NodeAABox;
        output.OutputNodeAABox.Center.w = output.OutputNodeAABox.Center.magnitude;

        return output;
    }

    /// <summary>
    /// We use this when we determine the mesh is from Unity. Because it doesn't have an extra vertex which
    /// is the normal in GCT read shapes.
    /// </summary>
    private Vector3 GenerateNormal()
    {
        return transform.forward;
    }

    private bool Validate()
    {
        bool failed = false;

        if (Mesh == null)
        {
            Debug.LogError("You need a triangle or quad mesh to export a GCT shape. GameObject: " + transform.name);
            failed = true;
        }
        if(AABox == null)
        {
            Debug.LogError("You need an AA Box to export a GCT shape. GameObject: " + transform.name);
            failed = true;
        }

        if(Type != GCTShapeType.Quad && Type != GCTShapeType.Triangle)
        {
            Debug.LogError("Unsupported shape type for export: " + Type + " GameObject: " + transform.name);
            failed = true;
        }

        return !failed;
    }
}
