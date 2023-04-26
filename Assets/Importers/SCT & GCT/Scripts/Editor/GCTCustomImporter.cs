using UnityEngine;
using System.Collections.Generic;
using System.Text;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using Yarhl.IO;
using System.IO;
using System.Linq;

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

        for (int i = 0; i < gctData.Shapes.Length; i++)
        {
            GCTShape shape = gctData.Shapes[i];

            if (shape is GCTShapePrimitive)
            {
                GameObject createdPrimitive = GenerateGCTPrimitive(gctData, shape as GCTShapePrimitive, i, ctx);

                if(createdPrimitive != null)
                    createdPrimitive.transform.parent = stageColl.transform;
            }
        }

        for(int i = 0; i < gctData.NodeAABoxes.Length; i++)
        {
            GCTAABox aaBox = gctData.NodeAABoxes[i];
            GameObject generatedAABox = GenerateAABoxTest(aaBox);

            if (generatedAABox != null)
                generatedAABox.transform.parent = stageColl.transform;
        }

        for (int i = 0; i < gctData.ShapeAABoxes.Length; i++)
        {
            GCTAABox aaBox = gctData.ShapeAABoxes[i];
            GameObject generatedAABox = GenerateAABoxTest(aaBox);

            if (generatedAABox != null)
                generatedAABox.transform.parent = stageColl.transform;
        }

        return stageColl;
    }


    private static GameObject GenerateAABoxTest(GCTAABox aaBoxData)
    {
        GameObject collider = new GameObject("AABox Collider");
        BoxCollider boxColl = collider.gameObject.AddComponent<BoxCollider>();
        boxColl.center = aaBoxData.Center;
        boxColl.size = aaBoxData.Extents * 2;


        return collider;
    }

    //Creates a mesh that only holds vertex information (only for debugging pruposes)
    private static GameObject GenerateGCTPrimitive(GCTHeader header, GCTShapePrimitive primitive, int index, AssetImportContext ctx)
    {
        string name = primitive.Header.GetShapeType().ToString() + "_" + index; //Quad_1

        GameObject primitiveObj = new GameObject(name + "_G");

        List<Vector3> vertices = new List<Vector3>();

        int[] indices = new int[primitive.Indices.Length];
        uint smallestIndex = primitive.Indices.Min();

        for (int i = 0; i < indices.Length; i++)
        {
            try
            {
                vertices.Add(header.Vertices[primitive.Indices[i]]);
            }
            catch
            {
                Debug.LogError("Indice error at " + name);
                DestroyImmediate(primitiveObj);
                return null;
            }

           //vertices.Add(header.Vertices[header.Indices[i]]);
            indices[i] = i;        
        }

        //Let's not forget to include the normal here.
        vertices.Add(header.Vertices[primitive.NormalIndex]);

        MeshTopology topologyType;

        if (primitive.Header.GetShapeType() == GCTShapeType.Quad)
            topologyType = MeshTopology.Quads;
        else
            topologyType = MeshTopology.Triangles;

        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.SetVertices(vertices.ToArray());
        mesh.SetIndices(indices, topologyType, 0);
        mesh.RecalculateNormals();

        MeshCollider coll = primitiveObj.AddComponent<MeshCollider>();
        coll.sharedMesh = mesh;

        ctx.AddObjectToAsset(name, mesh);

        return primitiveObj;
    }
}
