using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SCTExporter : MonoBehaviour
{
    public SCTHeader Header;
    public string OutputPath = "";

    public bool Optimize = true;

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

        SCTExportData[] exportingShapes = gameObject.GetComponentsInChildren<SCTExportData>().Where(x => x.gameObject.activeInHierarchy).ToArray();
        List<SCTExportOutput> outputData = new List<SCTExportOutput>();

        for (int i = 0; i < exportingShapes.Length; i++)
        {
            SCTExportOutput[] outputShapes = exportingShapes[i].Export();
            outputData.AddRange(outputShapes);
        }

        //Vertices equal to zero = assume didnt export successfully and filter out.
        outputData = outputData.Where(x => x.Vertices.Length > 0).ToList();

        SCTShape[] shapes = outputData.Select(x => ConvertToShape(x)).Where(x => x != null).ToArray();
        SCTShape[] quadShapes = shapes.Where(x => x.Type == GCTShapeType.Quad).ToArray();
        SCTShape[] triangleShapes = shapes.Where(x => x.Type == GCTShapeType.Triangle).ToArray();

        Header.TriangleShapes = triangleShapes;
        Header.QuadShapes = quadShapes;
        Header.Vertices = m_vertices.ToArray();

        SCTWriter.Write(Header, OutputPath);

        //Cleanup
        Header.TriangleShapes = null;
        Header.QuadShapes = null;
        m_vertices = new List<Vector3>();
        m_vertices_indexes = new Dictionary<Vector3, int>();

        yield return null;
    }

    public SCTShape ConvertToShape(SCTExportOutput outputDat)
    {
        SCTShape shape = new SCTShape();
        shape.Type = outputDat.Type;
        shape.Flags = outputDat.Flags;
        shape.IndiceFlags = 0;
        shape.UnknownValue = outputDat.UnknownValue;
        shape.UnknownData = outputDat.UnknownData;
        shape.Unknown = outputDat.Unknown;
        shape.SphereBounds = outputDat.Bounds;
        shape.Normal = outputDat.Normal;

        if (shape.Type == GCTShapeType.Quad)
        {
            Vector3 vertex3 = outputDat.Vertices[2];
            Vector3 vertex4 = outputDat.Vertices[3];

            outputDat.Vertices[2] = vertex4;
            outputDat.Vertices[3] = vertex3;
        }

        uint[] indices = new uint[outputDat.Vertices.Length];

        if (Optimize)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                Vector3 vec = outputDat.Vertices[i];

                if (!m_vertices_indexes.ContainsKey(vec))
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
        }

        if (shape.Type == GCTShapeType.Quad)
        {
            uint indice3 = indices[2];
            uint indice4 = indices[3];

            indices[2] = indice4;
            indices[3] = indice3;
        }

        shape.IndiceFlags = 0;
        shape.Indices = indices;

        return shape;
    }
}
