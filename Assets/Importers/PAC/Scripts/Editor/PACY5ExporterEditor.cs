using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PACY5Exporter))]
public class PACY5ExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Export"))
            (target as PACY5Exporter).Export();
    }
}
