using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeStackOp : int
{
    PopPush = 0,
    Push = 1,
    Pop = 2,
    NoOp = 3,
}
public enum NodeType : int
{
    MatrixTransform = 0,
    UnskinnedMesh = 1,
    SkinnedMesh = 2,
}


public class GMDNode
{
    public int NodeID;
    public int ParentOfNodeID;
    public int SiblingOfNodeID;
    public int ObjectIndex; //-1 if not an object
    public int MatrixIndex; //Still unclear, but this is probably a matrix index. Usually == index.
    public NodeStackOp StackOp;
    public int NameIdx;
    public NodeType Type;

    public Vector4 Position;
    public Quaternion Rotation;
    public Vector4 Scale;

    public Vector4 WorldPosition;
    public Vector4 AnimAxis;
    public int[] Flags = new int[4];
}
