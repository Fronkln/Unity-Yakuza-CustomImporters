using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.Collections.Generic;
using System.IO;
using Yarhl.IO;
using System.Text;
using System.Xml.Linq;

[ScriptedImporter(1, "sct")]
public class SCTCustomImporter : ScriptedImporter
{
    public bool GenerateExportData = true;
    public bool DebugVertex;
    public bool DebugShapeVertex;
    public bool ImportQuad;
    public bool ImportTriangle;

    private AssetImportContext m_ctx;
    private SCTHeader m_header = null;
    private DataReader m_reader = null;
    private DataStream m_readStream = null;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        byte[] fileBuffer = File.ReadAllBytes(ctx.assetPath);

        m_ctx = ctx;
        m_readStream = DataStreamFactory.FromArray(fileBuffer, 0, fileBuffer.Length);
        m_reader = new DataReader(m_readStream) { DefaultEncoding = Encoding.GetEncoding(932) };

        string magic = m_reader.ReadString(4);
        m_reader.Stream.Position -= 4;

        GameObject createdCollisionObject = null;

        //Modern SCT, Yakuza 5 and above
        if (magic == "GCTD")
        {
            Debug.Log("OE/DE SCT");

            GCTHeader gctData = GCTReader.Read(m_reader);
            createdCollisionObject = GCTCustomImporter.Process(gctData, m_ctx, GenerateExportData);
        }
        else //True SCT, Yakuza 3 & 4
        {
            Debug.Log("OOE SCT");

            m_header = SCTReader.Read(m_reader);
            createdCollisionObject = Process(m_header);
        }

        if (createdCollisionObject != null)
        {
            if (GenerateExportData)
            {
                SCTExporter exporter = createdCollisionObject.AddComponent<SCTExporter>();
                exporter.Header = m_header;
                exporter.OutputPath = ctx.assetPath;
            }

            ctx.AddObjectToAsset(Path.GetFileNameWithoutExtension(ctx.assetPath), createdCollisionObject);
            ctx.SetMainObject(createdCollisionObject);
        }


    }

    //Create the stage collision object
    public GameObject Process(SCTHeader sctData)
    {
        GameObject stageColl = new GameObject();

        for (int i = 0; i < sctData.TriangleShapes.Length; i++)
        {
            GenerateShape(sctData.TriangleShapes[i], i).transform.parent = stageColl.transform;
        }

        for (int i = 0; i < sctData.QuadShapes.Length; i++)
        {
            GenerateShape(sctData.QuadShapes[i], i).transform.parent = stageColl.transform;
        }

        if (DebugVertex)
        {
            GameObject holder = new GameObject("Vertices");
            holder.transform.parent = stageColl.transform;

            for(int i = 0; i < sctData.Vertices.Length; i++)
            {
                Vector3 vtx = sctData.Vertices[i];
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.name = i.ToString();
                obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                obj.transform.parent = holder.transform;
                obj.transform.position = vtx;
            }

        }

        return stageColl;
    }

    public static bool debugVtx = true;

    private GameObject GenerateShape(SCTShape sctShape, int index)
    {
        GameObject shape = new GameObject();

        Mesh shapeMesh = new Mesh();

        List<Vector3> meshVertices = new List<Vector3>();

        //Triangle
        if (sctShape.Type == GCTShapeType.Triangle)
        {
            shape.name = "Triangle_" + index;
            shapeMesh.name = "Triangle_" + index + "_Mesh";

            meshVertices.Add(m_header.Vertices[sctShape.Indices[0]]);
            meshVertices.Add(m_header.Vertices[sctShape.Indices[1]]);
            meshVertices.Add(m_header.Vertices[sctShape.Indices[2]]);

            shapeMesh.SetVertices(meshVertices.ToArray());
            shapeMesh.SetIndices(new int[] { 1, 2, 0 }, MeshTopology.Triangles, 0);
            shapeMesh.RecalculateBounds();
        }
        else
        {
            shape.name = "Quad_" + index;
            shapeMesh.name = "Quad_" + index + "_Mesh";

            foreach (ushort idx in sctShape.Indices)
                meshVertices.Add(m_header.Vertices[idx]);

            shapeMesh.SetVertices(meshVertices);
            shapeMesh.SetIndices(new int[] { 1, 3, 2, 0 }, MeshTopology.Quads, 0);
            shapeMesh.RecalculateBounds();
        }

        MeshCollider coll = shape.AddComponent<MeshCollider>();
        coll.sharedMesh = shapeMesh;

        GameObject sphereBound = new GameObject("Sphere Bound");
        sphereBound.transform.parent = shape.transform;
        sphereBound.transform.localRotation = Quaternion.identity;
        
        SphereCollider sphereColl = sphereBound.AddComponent<SphereCollider>();
        //sphereColl.center = sctShape.SphereBounds.Center;
        sphereColl.radius = sctShape.SphereBounds.Radius;
        sphereBound.transform.position = sctShape.SphereBounds.Center;

        if (DebugShapeVertex)
        {
            foreach(Vector3 vtx in meshVertices)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                obj.transform.parent = shape.transform;
                obj.transform.position = vtx;
            }
        }

        if(GenerateExportData)
        {
            SCTExportData data = shape.gameObject.AddComponent<SCTExportData>();
            data.Flags = sctShape.Flags;
            data.Unknown = sctShape.Unknown;
            data.UnknownData = sctShape.UnknownData;
            data.UnknownValue = sctShape.UnknownValue;
            data.BoundingSphere = sphereColl;
            data.Mesh = shapeMesh;
            data.Normal = sctShape.Normal;
        }

        m_ctx.AddObjectToAsset(shapeMesh.name, shapeMesh);

        return shape;
    }

    //Creates a mesh that only holds vertex information (only for debugging pruposes)
    private static Mesh DebugCreateVerticesMesh(SCTHeader data)
    {
        Mesh mesh = new Mesh();
        mesh.name = "debug_vertices_mesh";
        mesh.vertices = data.Vertices;
        mesh.RecalculateNormals();

        return mesh;
    }
}
