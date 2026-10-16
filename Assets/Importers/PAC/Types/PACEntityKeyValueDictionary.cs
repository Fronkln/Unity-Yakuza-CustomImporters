using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum KeyValueType
{
    Byte,
    Short,
    Int,
    Float,
    DisposeString //int if dispose_string.bin is not provided otherwise string
}

[Serializable]
public class PACEntityKeyValueDictionary
{
    [SerializeField]
    public List<string> Keys = new List<string>();

    [SerializeField]
    public List<string> Values = new List<string>();

    [SerializeField]
    private List<KeyValueType> m_kvTypes = new List<KeyValueType>();


    public bool ContainsKey(string key)
    {
        return Keys.Any(x => x == key);
    }

    public void AddKeyValue(string key, string value, KeyValueType type)
    {
        if (this.ContainsKey(key))
            throw new Exception("Key already exists.");

        Keys.Add(key);
        Values.Add(value);
        m_kvTypes.Add(type);
    }

    public void RemoveKeyValue(string key)
    {
        if (!this.ContainsKey(key))
            return;

        int index = Keys.IndexOf(key);

        Keys.RemoveAt(index);
        Values.RemoveAt(index);
        m_kvTypes.RemoveAt(index);
    }

    public void SetKeyValue(string key, string value, KeyValueType type)
    {
        if (!this.ContainsKey(key))
        {
            AddKeyValue(key, value, type);
            return;
        }


        int idx = Keys.IndexOf(key);
        Values[idx] = value;
    }


    //can we make this linear time?
    public KeyValueType GetTypeForKey(string key)
    {
        return m_kvTypes[Keys.IndexOf(key)];
    }

    public string GetValueForKey(object key)
    {
        return Values[Keys.IndexOf((string)key)];
    }
}
