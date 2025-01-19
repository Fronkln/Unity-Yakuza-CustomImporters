using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GCTExportDataCustom : GCTExportData
{
    public override GCTExportOutput[] Export(int shapeID)
    {
        MeshFilter filter = transform.GetComponent<MeshFilter>();

        if (filter != null)
            Mesh = filter.sharedMesh;

        if(Mesh == null)
        {
            Debug.LogError("Mesh is null for " + transform.name + " will not export GCT shape");
            return new GCTExportOutput[0];
        }

        MeshTopology meshTopology = Mesh.GetTopology(0);

        if(meshTopology != MeshTopology.Quads && meshTopology != MeshTopology.Triangles)
        {
            Debug.LogError("Mesh topology for " + transform.name + " is neither quads or triangles. Will not export GCT shape, Topology was: " + meshTopology);
            return new GCTExportOutput[0];
        }

        Debug.Log("Exporting custom mesh with topology: " + meshTopology);

        GCTExportOutput[] result;

        if (meshTopology == MeshTopology.Triangles)
            result = ExportTriangleTopologyMesh(shapeID);
        else
            result = ExportQuadTopologyMesh(shapeID);

        return result;
    }

    public void Update()
    {
        if (transform.parent != null)
        {
            if (transform.parent.localScale.x < 0)
            {
                Vector3 newScale = new Vector3(Mathf.Abs(transform.localScale.x) * -1f, transform.localScale.y, transform.localScale.z);
                transform.localScale = newScale;
            }
        }
    }

    private GCTExportOutput[] ExportTriangleTopologyMesh(int shapeID)
    {
        Vector3 CalculateNormal(Vector3[] vertices)
        {
            Vector3 v0 = vertices[0];
            Vector3 v1 = vertices[1];
            Vector3 v2 = vertices[2];

            // Calculate the edges of the triangle
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            // Compute the normal using the cross product
            Vector3 normal = Vector3.Cross(edge1, edge2);

            // Normalize the normal
            normal.Normalize();

            return normal;
        }

        Type = GCTShapeType.Triangle;

        Mesh[] meshData = GetTrianglesMeshes();
        List<GCTExportOutput> outputDatas = new List<GCTExportOutput>();

        foreach(Mesh mesh in meshData)
        {
            GCTShapeHeader genHeader = new GCTShapeHeader();
            genHeader.Flags = GenerateFlagsBitfield(shapeID);
            genHeader.Attributes = GenerateAttributesBitfield();

            GCTExportOutput output = new GCTExportOutput();
            output.ShapeHeader = genHeader;
            output.Type = GCTShapeType.Triangle;
            output.GenerateNodeAABox = false;

            if (AABox == null)
            {
                Bounds localBounds = mesh.bounds;

                // Transform the local bounds to world space using the object’s Transform
                Bounds worldBounds = new Bounds(
                    transform.TransformPoint(localBounds.center), // Transform the center to world space
                    transform.TransformVector(localBounds.size)   // Transform the size (extents) to world space
                );

                Vector3 extents = worldBounds.extents;

                if (extents.x == 0)
                    extents.x = 0.01f;

                if (extents.y == 0)
                    extents.y = 0.01f;

                if (extents.z == 0)
                    extents.z = 0.01f;

                output.OutputAABox.Center = worldBounds.center;
                output.OutputAABox.Center.w = worldBounds.center.magnitude;
                output.OutputAABox.Extents = worldBounds.extents;
                output.OutputAABox.HitFilter = AABoxHitFilter;

                
            }
            else
            {
                output.OutputAABox.Center = AABox.bounds.center;
                output.OutputAABox.Center.w = AABox.bounds.center.magnitude;
                output.OutputAABox.Extents = AABox.bounds.extents;
                output.OutputAABox.HitFilter = AABoxHitFilter;
            }

            Vector3[] newVertices = mesh.vertices;
            Matrix4x4 localToWorld = transform.localToWorldMatrix;

            for (int i = 0; i < newVertices.Length; i++)
            {
                newVertices[i] = localToWorld.MultiplyPoint3x4(newVertices[i]);
            }

            output.Vertices = new Vector3[4]
            {
                newVertices[0],
                newVertices[1], 
                newVertices[2],
                CalculateNormal(newVertices)
            };

            output.Product = Vector3.Dot(output.Vertices[3], output.Vertices[0]);

            output.Indices = mesh.triangles;

            outputDatas.Add(output);
        }

        //Clean after processing them, these meshes were temporary. otherwise memory leaks could happen
        foreach(Mesh mesh in meshData)
            DestroyImmediate(mesh);

        return outputDatas.ToArray();
    }

    private GCTExportOutput[] ExportQuadTopologyMesh(int shapeID)
    {
        Type = GCTShapeType.Quad;

        return new GCTExportOutput[0];
    }

    //Seperates each triangle into its own mesh
    private Mesh[] GetTrianglesMeshes()
    {
        // Get the vertices, triangles, and other mesh data
        Vector3[] vertices = Mesh.vertices;
        int[] triangles = Mesh.triangles;
        Vector3[] normals = Mesh.normals;
        Vector2[] uvs = Mesh.uv;

        List<Mesh> meshes = new List<Mesh>();

        // Iterate through each triangle
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Define indices for the three vertices of the triangle
            int index0 = triangles[i];
            int index1 = triangles[i + 1];
            int index2 = triangles[i + 2];

            // Extract the three vertices of the triangle
            Vector3[] triangleVertices = new Vector3[3] { vertices[index0], vertices[index1], vertices[index2] };

            // Extract the corresponding normals and UVs for each vertex
            Vector3[] triangleNormals = new Vector3[3] { normals[index0], normals[index1], normals[index2] };
            Vector2[] triangleUVs = new Vector2[3] { uvs[index0], uvs[index1], uvs[index2] };

            // Create a new mesh for the triangle
            Mesh triangleMesh = new Mesh
            {
                vertices = triangleVertices,
                triangles = new int[] { 0, 1, 2 }, // Each mesh has a single triangle (3 indices)
                normals = triangleNormals,
                uv = triangleUVs
            };

            triangleMesh.name = "Triangle " + i;

            meshes.Add(triangleMesh);
        }

        return meshes.ToArray();
    }
}
