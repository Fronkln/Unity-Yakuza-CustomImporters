using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class GCTExporter : MonoBehaviour
{
    public string OutputPath = "";
    public bool IsOEGct = false;

    public int Flags;
    public uint HitFilter;
    public uint NodeDepth;

    public Bounds Bounds;

    private GCTHeader m_generatedHeader;

    List<Vector3> m_vertices = new List<Vector3>();

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

       // transform.localScale = new Vector3(1, 1, 1);

        //yield return new WaitForSecondsRealtime(0.1f);

        GCTExportData[] exportingShapes = gameObject.GetComponentsInChildren<GCTExportData>().Where(x => x.gameObject.activeInHierarchy).ToArray();
        List<GCTExportOutput> outputData = new List<GCTExportOutput>();

        int shapeIdx = 0;

        for (int i = 0; i < exportingShapes.Length; i++)
        {
            outputData.AddRange(exportingShapes[i].Export(shapeIdx));

            if (exportingShapes[i] is GCTExportDataUnityBox)
                shapeIdx += 6;
            else
                shapeIdx++;
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

       // transform.localScale = new Vector3(-1, 1, 1);


        //Clear
        m_generatedHeader = new GCTHeader();
        m_vertices = new List<Vector3>();
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

        int verticesStart = m_vertices.Count;
        m_vertices.AddRange(outputDat.Vertices);

        uint[] indices = new uint[outputDat.Vertices.Length - 1];

        for(int i = 0; i < indices.Length; i++)
            indices[i] = (uint)(verticesStart + i);

        shape.Header = outputDat.ShapeHeader;
        shape.Indices = indices;
        shape.NormalIndex = (uint)verticesStart + (uint)indices.Length;
        shape.Product = outputDat.Product;


        return shape;
    }
}
