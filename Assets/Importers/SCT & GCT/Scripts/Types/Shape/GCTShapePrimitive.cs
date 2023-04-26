using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GCTShapePrimitive : GCTShape
{
    public uint[] Indices;
    public uint NormalIndex = 0;
    public float Product;
}
