using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GCTExporter))]
public class GCTExporterEditor :  Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Export"))
            (target as GCTExporter).Export();
    }
}
