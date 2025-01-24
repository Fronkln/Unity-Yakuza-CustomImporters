using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DEEntityImporter))]
public class DEEntityImporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Import"))
            (target as DEEntityImporter).Import();

        if (GUILayout.Button("Export"))
            (target as DEEntityImporter).Export();
    }

}
