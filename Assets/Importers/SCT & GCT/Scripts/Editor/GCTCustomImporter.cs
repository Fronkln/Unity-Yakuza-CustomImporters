using UnityEngine;
using System.Text;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using Yarhl.IO;
using System.IO;

[ScriptedImporter(1, "gct")]
public class GCTCustomImporter : ScriptedImporter
{
    private AssetImportContext m_ctx;
    private DataReader m_reader = null;
    private DataStream m_readStream = null;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        Debug.Log("GCT");

        byte[] fileBuffer = File.ReadAllBytes(ctx.assetPath);

        m_ctx = ctx;
        m_readStream = DataStreamFactory.FromArray(fileBuffer, 0, fileBuffer.Length);
        m_reader = new DataReader(m_readStream) { DefaultEncoding = Encoding.GetEncoding(932) };

        GCTHeader gctData = GCTReader.Read(m_reader);
        GameObject createdStageObj = Process(gctData, m_ctx);

        if (createdStageObj != null)
        {
            m_ctx.AddObjectToAsset(gctData.Name.Text, createdStageObj);
            m_ctx.SetMainObject(createdStageObj);
        }
    }


    //Create the stage collision object
    //ctx is an argument because SCTReader will call this if it realizes its not a true SCT (Y5 and Above)
    public static GameObject Process(GCTHeader gctData, AssetImportContext ctx)
    {
        GameObject stageColl = new GameObject();

        //Visualize vertices (Debug)
        GameObject debugMeshObj = new GameObject();
        MeshFilter debugMeshFilter = debugMeshObj.AddComponent<MeshFilter>();
        debugMeshFilter.sharedMesh = DebugCreateVerticesMesh(gctData);

        VisualizeVertex visualizer = debugMeshObj.AddComponent<VisualizeVertex>();
        visualizer.Mf = debugMeshFilter;
        visualizer.Scale = 0.1f;

        debugMeshObj.transform.parent = stageColl.transform;

        ctx.AddObjectToAsset("debug_test_vertices_mesh", debugMeshFilter.sharedMesh);

        return stageColl;
    }

    //Creates a mesh that only holds vertex information (only for debugging pruposes)
    private static Mesh DebugCreateVerticesMesh(GCTHeader data)
    {
        Mesh mesh = new Mesh();
        mesh.name = "debug_vertices_mesh";
        mesh.vertices = data.Vertices;
        mesh.RecalculateNormals();

        return mesh;
    }
}
