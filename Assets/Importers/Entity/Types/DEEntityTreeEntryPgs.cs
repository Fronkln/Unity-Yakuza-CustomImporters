using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DEEntityTreeEntryPgs
{
    [Serializable]
    public class Ary
    {
        [JsonProperty(".ver")]
        public int Version;
        [JsonProperty("ary")]
        public List<Ary> Array;

        [JsonProperty(".kind")]
        public string Kind;
        public int? permission_id { get; set; } = null;
        public bool? reverse { get; set; } = null;
        public int? m_scene_id { get; set; }
        public bool? m_b_reverse { get; set; }
        public bool? m_b_adv { get; set; }
        public bool? m_b_btl { get; set; }
        public bool? m_b_scn { get; set; }
        public bool? m_b_pre { get; set; }
        public string m_category_name { get; set; }
        public string m_clock_name { get; set; }
        public string m_timeline_name { get; set; }
        public int? m_category { get; set; }
        public int? m_timeline { get; set; }
        public int? m_clock { get; set; }
        public int? flags { get; set; }
        public string m_casting_name { get; set; }
        public int? m_casting { get; set; }
        public int? m_casting_puid { get; set; }
        public bool? m_b_auth_play { get; set; }
        public int? m_dip_switch_id { get; set; }
    }

    [JsonProperty(".ver")]
    public int Version;
    public List<Ary> ary = new List<Ary>();
}
