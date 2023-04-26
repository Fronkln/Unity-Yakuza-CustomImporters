using System.Collections.Generic;
using UnityEngine;

public class GCTExportData : MonoBehaviour
{
    [Header("Required")]
    public Mesh Mesh;
    public BoxCollider AABox;
    public int AABoxHitFilter;

    [Header("Temp, calculate later")]
    public float Product;

    [Header("Dont Change If You Have No Clue")]
    public GCTShapeHeader NodeHeader;
    public GCTShapeType Type;
    public bool GenerateNodeAABox;


    public GCTExportOutput Export()
    {
        if (!Validate())
            return new GCTExportOutput();

        GCTExportOutput output = new GCTExportOutput();

        List<Vector3> actualVertices = new List<Vector3>(Mesh.vertices);

        for(int i = 0; i < actualVertices.Count; i++)
        {
            //Doesnt make a difference for GCT primitives
            //Is important for Unity ones however.
            actualVertices[i] += transform.position;
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

        output.OutputAABox.Center = transform.position + AABox.center;
        output.OutputAABox.Extents = AABox.size / 2;
        output.OutputAABox.HitFilter = AABoxHitFilter;

        output.ShapeHeader = NodeHeader;
        output.Type = Type;
        output.Vertices = actualVertices.ToArray();
        output.Indices = Mesh.GetIndices(0);
        output.Product = Product;
        output.GenerateNodeAABox = GenerateNodeAABox;

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

        switch(Type)
        {
            default:
                Debug.LogError("Unsupported shape type for export: " + Type + " GameObject: " + transform.name);
               // failed = true;
                break;
        }

        return !failed;
    }
}
