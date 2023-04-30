using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GCTExportData : MonoBehaviour
{
    [Header("Required")]
    public Mesh Mesh;
    public BoxCollider AABox;
    public int AABoxHitFilter = -2147479553;


    [Header("Shape")]
    public GCTShapeType Type = GCTShapeType.Quad;
    public ShapeEdgeFlags EdgeFlags = ShapeEdgeFlags.Unknown2;

    [Header("Attributes")]
    public byte Material = 65;
    public byte Control = 2;

    [Header("Dont Change If You Have No Clue")]
    public bool GenerateNodeAABox;
    public GCTAABox NodeAABox;
    public uint NodeID;

    private void Awake()
    {
        Mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
    }

    public virtual GCTExportOutput[] Export(int shapeID)
    {
        if (!Validate())
            return new GCTExportOutput[0];

        GCTExportOutput output = new GCTExportOutput();

        List<Vector3> actualVertices = new List<Vector3>(Mesh.vertices);
        int normalIdx = actualVertices.Count - 1;

        for (int i = 0; i < (!IsUnityShape() ? normalIdx : actualVertices.Count); i++)
        {
            Matrix4x4 localToWorld = transform.localToWorldMatrix;
            actualVertices[i] = localToWorld.MultiplyPoint3x4(Mesh.vertices[i]);
        }

        if(IsUnityShape())
            actualVertices.Add(GenerateNormal());
        else
        {
            Vector3 side1 = actualVertices[1] - actualVertices[0]; 
            Vector3 side2 = actualVertices[2] - actualVertices[0]; 
            Vector3 normal = Vector3.Cross(side2, side1).normalized;

            print(normal);

         //   actualVertices[actualVertices.Count - 1] =
        }

        output.OutputAABox.Center = AABox.bounds.center;
        output.OutputAABox.Center.w = AABox.bounds.center.magnitude;
        output.OutputAABox.Extents = AABox.bounds.extents;
        output.OutputAABox.HitFilter = AABoxHitFilter;

        GCTShapeHeader genHeader = new GCTShapeHeader();
        genHeader.Flags = GenerateFlagsBitfield(shapeID);
        genHeader.Attributes = GenerateAttributesBitfield();

        output.ShapeHeader = genHeader;
        output.Type = Type;
        output.Vertices = actualVertices.ToArray();
        output.Indices = Mesh.GetIndices(0);
        output.Product = Vector3.Dot(actualVertices[normalIdx], actualVertices[0]);
        output.GenerateNodeAABox = GenerateNodeAABox;
        output.OutputNodeAABox = NodeAABox;
        output.OutputNodeAABox.Center.w = output.OutputNodeAABox.Center.magnitude;

        return new GCTExportOutput[] { output };
    }

    /// <summary>
    /// We use this when we determine the mesh is from Unity. Because it doesn't have an extra vertex which
    /// is the normal in GCT read shapes.
    /// </summary>
    protected Vector3 GenerateNormal()
    {
        return transform.forward;
    }
    
    protected bool IsUnityShape()
    {
        switch(Type)
        {
            case GCTShapeType.Quad:
                return Mesh.vertices.Length == 4;
            case GCTShapeType.Triangle:
                return Mesh.vertices.Length == 3;
        }

        return false;
    }

    protected uint GenerateAttributesBitfield()
    {
        uint result = 0xFF800000;
        result = BitHelper.SetBits(result, Material, 8);
        result = BitHelper.SetBits(result, Control, 0);

        return result;
    }

    protected uint GenerateFlagsBitfield(int shapeID)
    {
        Bits32 shapeTypeBits = new Bits32(0, 4);
        Bits32 edgeBits = new Bits32(28, 4);
        Bits32 shapeIDBits = new Bits32(4, 18);
        Bits32 nodeIDBits = new Bits32(22, 6);

        uint flag = 0;
        flag |= shapeTypeBits.WriteInto(flag, (uint)Type);
        flag |= shapeIDBits.WriteInto(flag, (uint)shapeID);
        flag |= nodeIDBits.WriteInto(flag, (uint)NodeID);
        flag |= edgeBits.WriteInto(flag, (byte)EdgeFlags);

        return flag;
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

        switch(Type)
        {
            case GCTShapeType.Quad:
                if(Mesh.vertices.Length > 5)
                {
                    Debug.LogError("More vertices than expected. " + " GameObject: " + transform.name);
                    failed = true;
                }
                break;
            case GCTShapeType.Triangle:
                if (Mesh.vertices.Length > 4)
                {
                    Debug.LogError("More vertices than expected. " + " GameObject: " + transform.name);
                    failed = true;
                }
                break;
        }

        return !failed;
    }
}
