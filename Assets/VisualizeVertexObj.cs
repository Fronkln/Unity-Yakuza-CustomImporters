
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(VisualizeVertexObj))]
public class VisualizeVertexObjEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
            (target as VisualizeVertexObj).Create();
    }
}

public class VisualizeVertexObj : MonoBehaviour
{
    public Mesh mesh;

    public void Create()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        foreach(Vector3 vert in mesh.vertices)
        {
            GameObject verty = GameObject.CreatePrimitive(PrimitiveType.Cube);
            verty.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            verty.transform.parent = transform;
            verty.transform.localPosition = vert;
        }
    }

}
#endif