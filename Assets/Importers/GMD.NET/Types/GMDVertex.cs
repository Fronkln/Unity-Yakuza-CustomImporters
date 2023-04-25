using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMDVertex
{
    public Vector3 Position;
    public byte[] BoneWeights = new byte[4];
    public byte[] BoneIndices = new byte[4];

    public Vector4 Normal;
    public Vector4 Tangent;
    public Vector2 UV;
}
