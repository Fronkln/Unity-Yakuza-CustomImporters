using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SCTExporter))]
public class SCTExporterEditor :  Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Export"))
            (target as SCTExporter).Export();
    }
}
