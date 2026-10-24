using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PACY5Exporter))]
public class PACY5ExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("IsOOE"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Path"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DisposeString"));

        var disposeStringProp = serializedObject.FindProperty("DisposeString");

        if (disposeStringProp.objectReferenceValue != null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExportDisposeString"));

            var exportDisposeStringProp = serializedObject.FindProperty("ExportDisposeString");

            if(exportDisposeStringProp.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExportDisposeStringPath"));
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        if (GUILayout.Button("Export"))
            ((PACY5Exporter)target).Export();
    }
}