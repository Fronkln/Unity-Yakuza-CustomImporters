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

[ScriptedImporter(1, "sct")]
public class SCTCustomImporter : ScriptedImporter
{
    public bool GenerateExportData = true;

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
        if(magic == "GCTD")
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
            ctx.AddObjectToAsset(Path.GetFileNameWithoutExtension(ctx.assetPath), createdCollisionObject);
            ctx.SetMainObject(createdCollisionObject);
        }
    }

    //Create the stage collision object
    public GameObject Process(SCTHeader sctData)
    {
        GameObject stageColl = new GameObject();

        for(int i = 0; i < sctData.Shapes.Length; i++)
        {
            GenerateShape(sctData.Shapes[i], i).transform.parent = stageColl.transform;
        }

        return stageColl;
    }

    private GameObject GenerateShape(SCTShape sctShape, int index)
    {
        GameObject shape = new GameObject("Shape_" + index);

        Mesh shapeMesh = new Mesh();
        shapeMesh.name = "Shape_" + index + "_Mesh";

        List<Vector3> meshVertices = new List<Vector3>();

        foreach (ushort idx in sctShape.Indices)
            meshVertices.Add(m_header.Vertices[idx]);

        shapeMesh.SetVertices(meshVertices);
        shapeMesh.SetIndices(new int[] { 0, 2, 1, 3}, MeshTopology.Quads, 0);
        shapeMesh.RecalculateBounds();

        MeshCollider coll = shape.AddComponent<MeshCollider>();
        coll.sharedMesh = shapeMesh;

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
