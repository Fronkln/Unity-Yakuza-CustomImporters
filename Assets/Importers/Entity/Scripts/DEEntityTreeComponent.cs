using System;
using System.Collections.Generic;
using UnityEngine;
using static DEEntityTreeEntryPgs;


[Serializable]
public class DEEntityTreeComponentPGS
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
public class DEEntityTreeComponent : MonoBehaviour
{
    public int Version;

    public DEEntityTreeComponentPGS[] PGS;

    public ulong DS;

    public float FD;
    public int FG;
}
