using UnityEngine;
using UnityEditor;
using System.Linq;

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

                if(!string.IsNullOrEmpty(searchString))
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
                        if(!string.IsNullOrEmpty(searchString))
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
