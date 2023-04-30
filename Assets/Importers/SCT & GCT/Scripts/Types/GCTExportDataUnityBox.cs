using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class UnityBoxTest : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif


public class GCTExportDataUnityBox : GCTExportData
{
    private enum Side
    {
        Down,
        Up,
        Left,
        Right,
        Forward,
        Backward
    }

    private struct FaceGenInfo
    {
        public Vector3[] Vertices;
        public int[] Indices;
    }

    public override GCTExportOutput[] Export(int shapeID)
    {
        GCTExportOutput output = GenerateFaceExport(Side.Left, shapeID);

        return new GCTExportOutput[] 
        {
            GenerateFaceExport(Side.Left, shapeID),
            GenerateFaceExport(Side.Right, shapeID + 1),
            GenerateFaceExport(Side.Forward, shapeID + 2),
            GenerateFaceExport(Side.Backward, shapeID + 3),
            GenerateFaceExport(Side.Up, shapeID + 4),
            GenerateFaceExport(Side.Down, shapeID + 5),
        };
    }


    private GCTExportOutput GenerateFaceExport(Side side, int idx)
    {
        FaceGenInfo generated = GenerateFace(side);

        GCTShapeHeader genHeader = new GCTShapeHeader();
        genHeader.Flags = GenerateFlagsBitfield(idx);
        genHeader.Attributes = GenerateAttributesBitfield();

        GCTExportOutput output = new GCTExportOutput();
        output.ShapeHeader = genHeader;
        output.Type = GCTShapeType.Quad;


        switch (side)
        {
            case Side.Up:
                output.OutputAABox.Extents = new Vector3(AABox.bounds.extents.x, 0.001f, AABox.bounds.extents.z);
                output.OutputAABox.Center = AABox.bounds.center;
                output.OutputAABox.Center.y += AABox.bounds.extents.y;
                break;
            case Side.Down:
                output.OutputAABox.Extents = new Vector3(AABox.bounds.extents.x, 0.001f, AABox.bounds.extents.z);
                output.OutputAABox.Center = AABox.bounds.center;
                output.OutputAABox.Center.y -= AABox.bounds.extents.y;
                break;
            case Side.Backward:
                output.OutputAABox.Extents = new Vector3(AABox.bounds.extents.x, AABox.bounds.extents.y, 0.001f);
                output.OutputAABox.Center = AABox.bounds.center;
                output.OutputAABox.Center.z -= AABox.bounds.extents.z;
                break;
            case Side.Forward:
                output.OutputAABox.Extents = new Vector3(AABox.bounds.extents.x, AABox.bounds.extents.y, 0.001f);
                output.OutputAABox.Center = AABox.bounds.center;
                output.OutputAABox.Center.z += AABox.bounds.extents.z;
                break;

            case Side.Left:
                output.OutputAABox.Extents = new Vector3(0.001f, AABox.bounds.extents.y, AABox.bounds.extents.z);
                output.OutputAABox.Center = AABox.bounds.center;
                output.OutputAABox.Center.x += AABox.bounds.extents.x;
                break;

            case Side.Right:
                output.OutputAABox.Extents = new Vector3(0.001f, AABox.bounds.extents.y, AABox.bounds.extents.z);
                output.OutputAABox.Center = AABox.bounds.center;
                output.OutputAABox.Center.x -= AABox.bounds.extents.x;
                break;
        }

        output.OutputAABox.Center.w = output.OutputAABox.Center.magnitude;
        output.OutputAABox.HitFilter = AABoxHitFilter;

        output.Product = Vector3.Dot(generated.Vertices[4], generated.Vertices[0]);
        output.GenerateNodeAABox = false;
        //output.OutputNodeAABox = NodeAABox;
        //output.OutputNodeAABox.Center.w = output.OutputNodeAABox.Center.magnitude;

        int normalIdx = generated.Vertices.Length - 1;
        Matrix4x4 localToWorld = transform.localToWorldMatrix;

        for (int i = 0; i < normalIdx; i++)
           generated.Vertices[i] = localToWorld.MultiplyPoint3x4(generated.Vertices[i]);

        output.Vertices = generated.Vertices;
        output.Indices = generated.Indices;

        return output;
    }

    private FaceGenInfo GenerateFace(Side side)
    {
        Bounds localBounds = new Bounds(AABox.center, AABox.size);

        FaceGenInfo output = new FaceGenInfo();


        switch (side)
        {
            case Side.Left:
                output.Vertices = new Vector3[]
                {
                    new Vector3(localBounds.max.x, localBounds.max.y, localBounds.max.z),
                    new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z),
                    new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z),
                    new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z),
                    -transform.right
                };

                break;
            case Side.Right:
                output.Vertices = new Vector3[]
                {
                    new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z),
                    new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z),
                    new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z),
                    new Vector3(localBounds.min.x, localBounds.min.y, localBounds.min.z),
                    transform.right
                };
                break;
            case Side.Up:
                output.Vertices = new Vector3[]
                {
                    new Vector3(localBounds.max.x, localBounds.max.y, localBounds.max.z),
                    new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z),
                    new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z),
                    new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z),
                    transform.up
                };
                break;
            case Side.Down:
                output.Vertices = new Vector3[]
                {
                    new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z),
                    new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z),
                    new Vector3(localBounds.min.x, localBounds.min.y, localBounds.min.z),
                    new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z),
                    -transform.up
                };
                break;

            case Side.Forward:
                output.Vertices = new Vector3[]
                {
                    new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z),
                    new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z),
                    new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z),
                    new Vector3(localBounds.max.x, localBounds.max.y, localBounds.max.z),
                    transform.forward
                };
                break;

            case Side.Backward:
                output.Vertices = new Vector3[]
                {
                    new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z),
                    new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z),
                    new Vector3(localBounds.min.x, localBounds.min.y, localBounds.min.z),
                    new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z),
                    -transform.forward
                };
                break;
        }

        output.Indices = new int[] { 0, 1, 2, 3 };

        return output;
    }
}
