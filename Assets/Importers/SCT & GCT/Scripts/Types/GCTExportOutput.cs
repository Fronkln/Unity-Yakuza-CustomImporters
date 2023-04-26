using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GCTExportOutput
{
    public GCTShapeHeader ShapeHeader;
    public GCTShapeType Type;

    /// <summary>
    /// Vertices.Length - 1 is normal
    /// </summary>
    public Vector3[] Vertices;
    public int[] Indices;

    public float Product;

    public GCTAABox OutputAABox;
    public bool GenerateNodeAABox;
}
