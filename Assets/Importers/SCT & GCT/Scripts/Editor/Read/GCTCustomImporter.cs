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
    public bool GenerateExportData = true;

    [Header("Flips X scale of GCT. Enable in OOE/OE, Disable in DE")]
    public bool FlipGCT = false;

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
        GameObject createdStageObj = Process(gctData, m_ctx, GenerateExportData);

        if (createdStageObj != null)
        {
            m_ctx.AddObjectToAsset(gctData.Name.Text, createdStageObj);
            m_ctx.SetMainObject(createdStageObj);

            if (FlipGCT)
            {
                //flip it so the view aligns with the game, it will become 1 during conversion, then -1 again
                createdStageObj.transform.localScale = new Vector3(-1, 1, 1);
            }
        }

    }


    //Create the stage collision object
    //ctx and generateExportData is an argument because SCTReader will call this if it realizes its not a true SCT (Y5 and Above)
    public static GameObject Process(GCTHeader gctData, AssetImportContext ctx, bool generateExportData)
    {
        GameObject stageColl = new GameObject();

        for (int i = 0; i < gctData.Shapes.Length; i++)
        {
            GCTShape shape = gctData.Shapes[i];

            if (shape is GCTShapePrimitive)
            {
                GameObject createdPrimitive = GenerateGCTPrimitive(gctData, shape as GCTShapePrimitive, i, ctx, generateExportData);

                if(createdPrimitive != null)
                    createdPrimitive.transform.parent = stageColl.transform;
            }
        }

        if (generateExportData)
        {
            GCTExporter exporter = stageColl.gameObject.AddComponent<GCTExporter>();
            exporter.Flags = gctData.Flags;
            exporter.HitFilter = gctData.HitFilter;
            exporter.NodeDepth = gctData.NodeDepth;
            exporter.Bounds = gctData.Bounds;
            exporter.OutputPath = ctx.assetPath;
        }

        /*
        for(int i = 0; i < gctData.NodeAABoxes.Length; i++)
        {
            GCTAABox aaBox = gctData.NodeAABoxes[i];
            GameObject generatedAABox = GenerateAABoxTest(aaBox);

            if (generatedAABox != null)
                generatedAABox.transform.parent = stageColl.transform;
        }
        */
        //Absolutely useless
        /*
        for (int i = 0; i < gctData.ShapeAABoxes.Length; i++)
        {
            GCTAABox aaBox = gctData.ShapeAABoxes[i];
            GameObject generatedAABox = GenerateAABoxTest(aaBox);

            if (generatedAABox != null)
                generatedAABox.transform.parent = stageColl.transform;
        }
        */

        return stageColl;
    }


    private static BoxCollider GenerateAABoxTest(GCTAABox aaBoxData)
    {
        GameObject collider = new GameObject("AABox Collider");
        BoxCollider boxColl = collider.gameObject.AddComponent<BoxCollider>();
        boxColl.center = aaBoxData.Center;
        boxColl.size = aaBoxData.Extents * 2;


        return boxColl;
    }

    //Creates a mesh that only holds vertex information (only for debugging pruposes)
    private static GameObject GenerateGCTPrimitive(GCTHeader header, GCTShapePrimitive primitive, int index, AssetImportContext ctx, bool generateExportData)
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

        GCTAABox boundingBox = header.ShapeAABoxes[index];

        BoxCollider generatedAABox = GenerateAABoxTest(boundingBox);
        generatedAABox.transform.parent = primitiveObj.transform;
        generatedAABox.name = "AA Box " + index;


        if(generateExportData)
        {
            GCTExportData exportComponent = primitiveObj.AddComponent<GCTExportData>();
            exportComponent.AABox = generatedAABox;
            exportComponent.Mesh = mesh;

            exportComponent.Type = primitive.Header.GetShapeType();
            exportComponent.EdgeFlags = (ShapeEdgeFlags)primitive.Header.GetEdgeFlags();
            exportComponent.NodeID = primitive.Header.GetNodeID();

            exportComponent.Material = primitive.Header.GetMaterialType();
            exportComponent.Control = primitive.Header.GetControl();

            exportComponent.AABoxHitFilter = boundingBox.HitFilter;

            if (index < header.NodeAABoxes.Length)
            {
                exportComponent.NodeAABox = header.NodeAABoxes[index];
                exportComponent.GenerateNodeAABox = true;
            }
        }

        ctx.AddObjectToAsset(name, mesh);

        return primitiveObj;
    }
}
