using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeVertex : MonoBehaviour
{
    public MeshFilter Mf;
    public float Scale = 0.01f;

    public void OnDrawGizmos()
    {
        if (Mf == null || Mf.sharedMesh == null)
            return;

        foreach (Vector3 vec in Mf.sharedMesh.vertices)
        {
            Gizmos.DrawCube(Mf.transform.position + vec, new Vector3(Scale, Scale, Scale));
        }
    }
}

