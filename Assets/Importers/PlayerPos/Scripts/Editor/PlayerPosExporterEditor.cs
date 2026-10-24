using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerPosExporter))]
public class PlayerPosExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Export"))
            ((PlayerPosExporter)target).Export();
    }
}
