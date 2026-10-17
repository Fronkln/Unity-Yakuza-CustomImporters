
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PACComponentFightY5))]
public class PACComponentFightY5Editor : Editor
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
        PACComponentY5 targetComp = (PACComponentY5)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("BaseEntityData"),
            true // IMPORTANT: allow Unity to draw full array UI
        );

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("MsgData"),
            true // IMPORTANT: allow Unity to draw full array UI
        );


        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("Fighters"),
            true // IMPORTANT: allow Unity to draw full array UI
        );

        if(GUILayout.Button("Assign fighters to fight 1"))
        {
            foreach(var fighter in ((PACComponentFightY5)target).Fighters)
            {
                if (fighter == null)
                    continue;

                foreach(var entDat in fighter.BaseEntityData)
                    entDat.KeyValues.SetKeyValue("fight_node_id1", int.Parse(targetComp.transform.name, System.Globalization.NumberStyles.HexNumber).ToString(), KeyValueType.Int);
            }
        }

        if (GUILayout.Button("Assign fighters to fight 2"))
        {
            foreach (var fighter in ((PACComponentFightY5)target).Fighters)
            {
                if (fighter == null)
                    continue;

                foreach (var entDat in fighter.BaseEntityData)
                    entDat.KeyValues.SetKeyValue("fight_node_id2", int.Parse(targetComp.transform.name, System.Globalization.NumberStyles.HexNumber).ToString(), KeyValueType.Int);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}