using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class JsonUtils
{
   public static JObject FindCentityBaseObject(JToken token)
    {
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            if (obj.ContainsKey("centity_base"))
                return obj; // Found the folder with the paper!

            foreach (var property in obj.Properties())
            {
                var found = FindCentityBaseObject(property.Value);
                if (found != null)
                    return found; // Found deeper inside!
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            foreach (var item in token.Children())
            {
                var found = FindCentityBaseObject(item);
                if (found != null)
                    return found; // Found in one of the papers!
            }
        }

        return null; // Nope, not here. Keep looking!
    }

    public static JProperty GetEntityProperty(this JToken token)
    {
        foreach (JProperty child in token.Children())
        {
            if (child.Name.StartsWith("c"))
                return child;
        }

        return null;
    }

    public static JToken FirstProperty(this JToken token)
    {
        foreach (var child in token.Children())
        {
            if (child.Type == JTokenType.Property)
                return (JToken)child;
        }

        return null;
    }

    public static JObject FirstObject(this JToken token)
    {
        foreach(var child in token.Children())
        {
            if(child.Type == JTokenType.Object)
                return (JObject)child;
        }

        return null;
    }

    public static JObject FindObjectWithKey(this JToken token, string key)
    {
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            if (obj.ContainsKey(key))
                return obj;

            foreach (var property in obj.Properties())
            {
                JObject found = FindObjectWithKey(property.Value, key);
                if (found != null)
                    return found;
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            foreach (var item in token.Children())
            {
                JObject found = FindObjectWithKey(item, key);
                if (found != null)
                    return found;
            }
        }

        return null;
    }
}

