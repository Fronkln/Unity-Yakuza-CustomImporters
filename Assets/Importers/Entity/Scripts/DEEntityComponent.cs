using System;
using System.Collections.Generic;
using UnityEngine;
using static DEEntityTreeEntryPgs;


[Serializable]
public class DEEntityComponentPGS
{
    [Serializable]
    public class PGSEntry
    {
        [Multiline]
        public string Value;
    }

    public int Version = 0;
    public List<PGSEntry> Ary = new List<PGSEntry>();
}
public class DEEntityComponent : MonoBehaviour
{
    public int Version;

    public DEEntityComponentPGS[] PGS;

    public ulong DS;

    public float FD;
    public int FG;
}
