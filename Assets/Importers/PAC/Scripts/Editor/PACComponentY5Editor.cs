
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PACComponentY5))]
public class PACComponentY5Editor : Editor
{
    SerializedProperty baseEntityDataProp;
    SerializedProperty msgProp;

    void OnEnable()
    {
        baseEntityDataProp = serializedObject.FindProperty("BaseEntityData");
        msgProp = serializedObject.FindProperty("MsgData");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("BaseEntityData"),
            true // IMPORTANT: allow Unity to draw full array UI
        );

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("MsgData"),
            true // IMPORTANT: allow Unity to draw full array UI
        );

        var header = new GUIStyle(EditorStyles.boldLabel);
        header.fontSize = 26;
        EditorGUILayout.LabelField($"Entity Keyvalues", header);

        var baseArray = baseEntityDataProp;

        for (int i = 0; i < baseArray.arraySize; i++)
        {
            SerializedProperty element =
                baseArray.GetArrayElementAtIndex(i);

            SerializedProperty keyValuesProp =
                element.FindPropertyRelative("KeyValues");

            SerializedProperty keysProp =
                keyValuesProp.FindPropertyRelative("Keys");

            SerializedProperty valuesProp =
                keyValuesProp.FindPropertyRelative("Values");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Entity {i}", EditorStyles.boldLabel);

            int count = Mathf.Min(keysProp.arraySize, valuesProp.arraySize);

            for (int j = 0; j < count; j++)
            {
                EditorGUILayout.BeginHorizontal();

                var key = keysProp.GetArrayElementAtIndex(j);
                var value = valuesProp.GetArrayElementAtIndex(j);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(key, GUIContent.none);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(value, GUIContent.none);

                EditorGUILayout.EndHorizontal();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}