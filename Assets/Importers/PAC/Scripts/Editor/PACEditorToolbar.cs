using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CustomTools
{
    [MenuItem("Tools/PAC/Find by PAC Ref String")]
    public static void ShowFindByStringWindow()
    {
        FindByPACRefStringWindow.ShowWindow();
    }

    [MenuItem("Tools/PAC/Find by PAC String Table")]
    public static void DoSomethingElse()
    {
        FindByPACStringTableWindow.ShowWindow();
    }
}

public class FindByPACStringTableWindow : EditorWindow
{
    private string searchString = "";

    public static void ShowWindow()
    {
        GetWindow<FindByPACStringTableWindow>("Find by PAC String Table");
    }

    public void OnGUI()
    {
        GUILayout.Label("Search", EditorStyles.boldLabel);

        searchString = EditorGUILayout.TextField("Search String", searchString);

        GUILayout.Space(10);

        if (GUILayout.Button("Search"))
        {
            GameObject selected = Selection.activeGameObject;

            if (selected == null)
                return;

            foreach (var entity in selected.GetComponentsInChildren<PACComponentY5>())
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    if (entity.MsgData.Strings.Any(x => x.Contains(searchString)))
                    {
                        Selection.activeGameObject = entity.gameObject;
                        Debug.Log("Found at " + entity.transform.name);
                        return;
                    }
                }
                else
                {
                    if (entity.MsgData.Strings.Any(x => !string.IsNullOrEmpty(x)))
                    {
                        Selection.activeGameObject = entity.gameObject;
                        Debug.Log("Found at " + entity.transform.name);
                        return;
                    }
                }
            }

            Debug.Log("Could not find in pac entities ref string data: " + searchString);
        }
    }
}

public class FindByPACRefStringWindow : EditorWindow
{
    private string searchString = "";

    public static void ShowWindow()
    {
        GetWindow<FindByPACRefStringWindow>("Find by PAC Ref String");
    }

    public void OnGUI()
    {
        GUILayout.Label("Search", EditorStyles.boldLabel);

        searchString = EditorGUILayout.TextField("Search String", searchString);

        GUILayout.Space(10);

        if (GUILayout.Button("Search"))
        {
            GameObject selected = Selection.activeGameObject;

            if (selected == null)
                return;

            foreach (var entity in selected.GetComponentsInChildren<PACComponentY5>())
            {
                for (int i = 0; i < entity.MsgData.Groups.Count; i++)
                {
                    var group = entity.MsgData.Groups[i];
                    foreach (var refData in group.Refs)
                    {
                        if (!string.IsNullOrEmpty(searchString))
                        {
                            if (refData.Text.Contains(searchString))
                            {
                                Selection.activeGameObject = entity.gameObject;
                                Debug.Log("Found at " + entity.transform.name + " Group ID: " + i);
                                return;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(refData.Text))
                            {
                                Selection.activeGameObject = entity.gameObject;
                                Debug.Log("Found at " + entity.transform.name + " Group ID: " + i);
                                return;
                            }
                        }
                    }
                }
            }

            Debug.Log("Could not find in pac entities ref string data: " + searchString);
        }
    }
}

public class PACComponentY5KVFinder : EditorWindow
{
    private string keyName = "";
    private string searchValue = "";
    private string prompt = "";

    private List<GameObject> results = new();

    private Vector2 scrollPosition;

    [MenuItem("Tools/PAC/Find PAC Entity By Key Value")]
    private static void OpenWindow()
    {
        GetWindow<PACComponentY5KVFinder>("PAC Finder");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("PAC Entity KV Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        keyName = EditorGUILayout.TextField("Key Name", keyName);

        searchValue = EditorGUILayout.TextField("Value", searchValue);

        EditorGUILayout.Space();

        if (GUILayout.Button("Find Components"))
            Search();

        EditorGUILayout.LabelField(prompt);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Results ({results.Count})", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var result in results)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(result.name, EditorStyles.boldLabel);

            if (GUILayout.Button("Select"))
            {
                Selection.activeGameObject = result;
                EditorGUIUtility.PingObject(result);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void Search()
    {
        results.Clear();

        if (string.IsNullOrWhiteSpace(keyName))
        {
            prompt = "Enter a key name.";
            return;
        }

        var components = Resources.FindObjectsOfTypeAll<Component>();

        foreach (var component in components)
        {
            if (component == null)
                continue;

            if (!(component is PACComponentY5))
                continue;

            if (!component.gameObject.scene.IsValid())
                continue;

            var componentPac = component as PACComponentY5;

            foreach (var entDat in componentPac.BaseEntityData)
            {
                if (!entDat.KeyValues.ContainsKey(keyName))
                    continue;

                string valueString = entDat.KeyValues.GetValueForKey(keyName);

                if (!valueString.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                    continue;

                results.Add(component.gameObject);
                break;
            }
        }

        prompt = $"Found {results.Count} matching component(s).";
    }
}
