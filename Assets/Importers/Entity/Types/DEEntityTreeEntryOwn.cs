using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DEEntityTreeEntryOwn
{
    [JsonProperty(".ver")]
    public int Version;
    [JsonProperty("ds")]
    public ulong DS;
    [JsonProperty("uid")]
    public ulong UID;

    [JsonProperty("pos")]
    public float[] Position;
    [JsonProperty("ori")]
    public float[] Orient;
    [JsonProperty("pgs")]
    public DEEntityTreeEntryPgs[] PGS;
    [JsonProperty("sra")]
    public List<int> SRA{ get; set; } //i dont think this is ever used 26.02.2025 update: it was used noob
    [JsonProperty("fd")]
    public float FD { get; set; }
    [JsonProperty("fg")]
    public int FG { get; set; }
}
