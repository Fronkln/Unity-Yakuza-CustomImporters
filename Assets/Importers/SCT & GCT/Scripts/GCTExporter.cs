using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

//To match with GMD, GCT should be negative scale
[ExecuteInEditMode]
public class GCTExporter : MonoBehaviour
{
    public string OutputPath = "";
    [Header("Write In Big Endian")]
    public bool IsOEGct = false;

    public int Flags;
    public uint HitFilter;
    public uint NodeDepth;

    [Header("Don't export duplicate vertices")]
    public bool Optimize = true;

    [Space(20)]
    public GCTAABox Bounds;

    private GCTHeader m_generatedHeader;

    List<Vector3> m_vertices = new List<Vector3>();
    Dictionary<Vector3, int> m_vertices_indexes = new Dictionary<Vector3, int>();

    public void Export()
    {
        StartCoroutine(ExportRoutine());
    }

    private IEnumerator ExportRoutine()
    {
        if (string.IsNullOrEmpty(OutputPath))
            yield return null;

        m_generatedHeader = new GCTHeader();
        m_vertices = new List<Vector3>();
        m_vertices_indexes = new Dictionary<Vector3, int>();

       // transform.localScale = new Vector3(1, 1, 1);

        //yield return new WaitForSecondsRealtime(0.1f);

        GCTExportData[] exportingShapes = gameObject.GetComponentsInChildren<GCTExportData>().Where(x => x.gameObject.activeInHierarchy).ToArray();
        List<GCTExportOutput> outputData = new List<GCTExportOutput>();

        int shapeIdx = 0;

        for (int i = 0; i < exportingShapes.Length; i++)
        {
            GCTExportOutput[] outputShapes = exportingShapes[i].Export(shapeIdx);
            outputData.AddRange(outputShapes);
            shapeIdx += outputShapes.Length;
        }

        //Vertices equal to zero = assume didnt export successfully and filter out.
        outputData = outputData.Where(x => x.Vertices.Length > 0).ToList();

        GCTShape[] shapes = outputData.Select(x => ConvertToShape(x)).Where(x => x != null).ToArray();

        m_generatedHeader.Vertices = m_vertices.ToArray();
        m_generatedHeader.Shapes = shapes.ToArray();
        m_generatedHeader.NodeAABoxes = outputData.Where(x => x.GenerateNodeAABox == true).Select(x => x.OutputNodeAABox).ToArray();
        m_generatedHeader.ShapeAABoxes = outputData.Select(x => x.OutputAABox).ToArray();
        m_generatedHeader.Flags = Flags;
        m_generatedHeader.HitFilter = HitFilter;
        m_generatedHeader.NodeDepth = NodeDepth;
        m_generatedHeader.Bounds = Bounds;
        m_generatedHeader.Name.Set(transform.name);

        GCTWriter.Write(m_generatedHeader, IsOEGct, OutputPath);

        //Clear
        m_generatedHeader = new GCTHeader();
        m_vertices = new List<Vector3>();
        m_vertices_indexes = new Dictionary<Vector3, int>();
    }

    private GCTShape ConvertToShape(GCTExportOutput outputDat)
    {
        GCTShapePrimitive shape = null;

        switch(outputDat.Type)
        {
            case GCTShapeType.Triangle:
                shape = new GCTShapeTriangle();
                break;
            case GCTShapeType.Quad:
                shape = new GCTShapeQuad();
                break;
        }

        uint[] indices = new uint[outputDat.Vertices.Length - 1];

        if (Optimize)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                Vector3 vec = outputDat.Vertices[i];
                
                if(!m_vertices_indexes.ContainsKey(vec))
                {
                    m_vertices.Add(vec);
                    m_vertices_indexes[vec] = m_vertices.Count - 1;
                    indices[i] = (uint)(m_vertices.Count - 1);
                }
                else
                {
                    indices[i] = (uint)m_vertices_indexes[vec];
                }
            }
      
            Vector3 normal = outputDat.Vertices[outputDat.Vertices.Length - 1];

            if (!m_vertices_indexes.ContainsKey(normal))
            {
                m_vertices.Add(normal);
                m_vertices_indexes[normal] = m_vertices.Count - 1;
                shape.NormalIndex = (uint)(m_vertices.Count - 1);
            }
            else
            {
                shape.NormalIndex = (uint)m_vertices_indexes[normal];
            }
        }
        else
        {
            int verticesStart = m_vertices.Count;
            m_vertices.AddRange(outputDat.Vertices);

            for (int i = 0; i < indices.Length; i++)
                indices[i] = (uint)(verticesStart + i);

            shape.NormalIndex = (uint)verticesStart + (uint)indices.Length;
        }

        shape.Header = outputDat.ShapeHeader;
        shape.Indices = indices;
        shape.Product = outputDat.Product;


        return shape;
    }
}
