using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


//https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
/// <summary>
/// SerializableDictionary - christophfranke123
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    protected List<TKey> keys = new List<TKey>();

    [SerializeField]
    protected List<TValue> values = new List<TValue>();


    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            Debug.LogError(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", keys.Count, values.Count));

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}
