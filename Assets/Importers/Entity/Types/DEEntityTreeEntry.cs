using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DEEntityTreeEntry
{
    [JsonProperty("own")]
    public DEEntityTreeEntryOwn Own;
    [JsonProperty("childs")]
    public List<DEEntityTreeEntry> Children = new List<DEEntityTreeEntry>();
}