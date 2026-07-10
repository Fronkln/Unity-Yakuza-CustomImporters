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
public class PACEntityKeyValueDictionary : SerializableDictionary<string, string>
{
    [SerializeField]
    private List<KeyValueType> m_kvTypes = new List<KeyValueType>();

    public void Add(string key, string value, KeyValueType type)
    {
        if (this.ContainsKey(key))
            throw new System.Exception("Key already exists.");

        this[key] = value;
        m_kvTypes.Add(type);
    }

    //can we make this linear time?
    public KeyValueType GetTypeForKey(string key)
    {
        return m_kvTypes[keys.IndexOf(key)];
    }
}
