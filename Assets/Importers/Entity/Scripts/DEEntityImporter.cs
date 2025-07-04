using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using Newtonsoft.Json.Linq;

public class DEEntityImporter : MonoBehaviour
{
    [Header("Shared")]
    public TextAsset EntityKinds;
    public TextAsset Stages;

    [Header("Import")]
    public string EntityDirectory;
    public string EntityTreePath;

    [Space(10)]
    [Header("Export")]
    public string EntityTreeExportPath;
    public string EntityExportPath;
    public Formatting ExportFormatting;
    public bool ExportEntityTree = true;
    public bool ExportEntities = true;

    [Space(10)]
    [Header("Misc")]
    public bool IsDE1 = false;

    //not finished yet
    [HideInInspector]
    public bool OnlyImportTargetStageEntities = false;

    private Dictionary<uint, string> m_entityKinds = new Dictionary<uint, string>();
    private Dictionary<uint, string> m_stages = new Dictionary<uint, string>();
    private Dictionary<string, StageDat> m_entityDirs = new Dictionary<string, StageDat>();


    private string GetEntityKindName(uint kind)
    {
        return m_entityKinds[kind];
    }

    public class StageDat
    {
        public Transform Transform = null;
        public Dictionary<string, Transform> EntityKindFolders = new Dictionary<string, Transform>();
    }

    private void CacheTypes()
    {
        m_entityDirs.Clear();
        m_stages.Clear();
        m_entityKinds.Clear();

        string[] dat = EntityKinds.text.Split('\n');

        foreach (string s in dat)
        {
            string[] split = s.Split(" = ");
            m_entityKinds[uint.Parse(split[1].Trim())] = split[0].Trim();
        }

        dat = Stages.text.Split("\n");

        for (int i = 0; i < dat.Length; i++)
        {
            string s = dat[i];
            if (string.IsNullOrEmpty(s))

                continue;

            try
            {
                string[] split = s.Split(" = ");
                m_stages[uint.Parse(split[1].Trim())] = split[0].Trim();
            }
            catch
            {
                Debug.LogError("Error entity kind parsing " + s);
            }
        }

    }

    public void Import()
    {
        if (!File.Exists(EntityTreePath))
        {
            Debug.LogWarning("Entity tree file does not exist");
            return;
        }

        foreach (Transform t in transform)
            DestroyImmediate(t.gameObject);

        CacheTypes();

        DEEntityTreeEntry root = JsonConvert.DeserializeObject<DEEntityTreeEntry>(File.ReadAllText(EntityTreePath));

        if (root.Own.Stage != null)
            IsDE1 = true;
        else
            IsDE1 = false;

        EntityCreationRecursion(root).transform.parent = transform;
    }

    public void Export()
    {
        CacheTypes();

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = ExportFormatting
        };

        if (ExportEntityTree)
        {
            DEEntityTreeEntry root = EntityTreeExportRecursion(transform.GetComponentInChildren<DEEntityTreeComponent>());
            string outputPath = EntityTreeExportPath;

            File.WriteAllText(outputPath, JsonConvert.SerializeObject(root));
        }

        if (ExportEntities)
        {
            DEEntityComponent[] components = transform.GetComponentsInChildren<DEEntityComponent>();

            foreach (DEEntityComponent ent in components)
            {
                JObject outputObject = EntityExport(ent);
                JToken entity = null;
                JObject compMap = null;
                JObject basic = null;

                entity = (JObject)outputObject["centity_base"];

                if(entity == null)
                    entity = outputObject.GetEntityProperty().Value;


                if (entity != null)
                {
                    
                    compMap = (JObject)entity["m_entity_component_map"];

                    if (compMap == null)
                    {

                        JToken entityBase = JsonUtils.FindCentityBaseObject(entity);

                        if (entityBase != null)
                        {
                            JToken entityBaseVal = entityBase["centity_base"];


                            if (entityBaseVal != null)
                            {
                                compMap = (JObject)entityBaseVal["m_entity_component_map"];
                            }
                        }
                    }
                }

                if (compMap != null)
                {
                    basic = (JObject)compMap["basic"];

                    if (basic != null)
                    {
                        JObject ec = (JObject)basic["p_ec"];

                        if (ec != null)
                        {
                            ec["m_v_orient_x"] = DEEntityUtils.ParseFloatAsInt(ent.transform.localRotation.x);
                            ec["m_v_orient_y"] = DEEntityUtils.ParseFloatAsInt(ent.transform.localRotation.y);
                            ec["m_v_orient_z"] = DEEntityUtils.ParseFloatAsInt(ent.transform.localRotation.z);
                            ec["m_v_orient_w"] = DEEntityUtils.ParseFloatAsInt(ent.transform.localRotation.w);

                            ec["m_v_pos.x"] = DEEntityUtils.ParseFloatAsInt(ent.transform.localPosition.x);
                            ec["m_v_pos.y"] = DEEntityUtils.ParseFloatAsInt(ent.transform.localPosition.y);
                            ec["m_v_pos.z"] = DEEntityUtils.ParseFloatAsInt(ent.transform.localPosition.z);
                        }
                    }
                }

                string output = outputObject.ToString(ExportFormatting);

                if (!string.IsNullOrEmpty(output))
                {
                    //TODO: Perhapsnot depend on entity tree component for this part
                    DEEntityTreeComponent treeComp = ent.transform.GetComponent<DEEntityTreeComponent>();

                    ulong UID = ulong.Parse(ent.transform.name, System.Globalization.NumberStyles.HexNumber);

                    string childKind = GetEntityKindName(DEEntityUtils.ExtractEntityKindFromUID(UID));
                    string stageName = m_stages[DEEntityUtils.ExtractStageIDFromDS(treeComp.DS)];
                    byte folder = DEEntityUtils.ExtractEntityFolderFromUID(UID);

                    string filePath = Path.Combine(EntityExportPath, stageName, childKind, folder.ToString("x2"), ent.transform.name + ".txt");
                    string dir = new FileInfo(filePath).Directory.FullName;

                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(filePath, output);

                }
            }
        }
    }

    private Transform EntityCreationRecursion(DEEntityTreeEntry entry)
    {
        Dictionary<string, StageDat> entityKindFolders = new Dictionary<string, StageDat>();

        string name = entry.Own.UID.ToString("X16");
        Transform t = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        t.localScale = new Vector3(0.5f, 0.5f, 0.5f);


        t.gameObject.name = name;
        DEEntityTreeComponent comp = t.gameObject.AddComponent<DEEntityTreeComponent>();

        comp.Version = entry.Own.Version;

        comp.PGS = new DEEntityTreeComponentPGS[entry.Own.PGS.Length];

        for (int i = 0; i < comp.PGS.Length; i++)
        {
            DEEntityTreeEntryPgs pgs = entry.Own.PGS[i];
            DEEntityTreeComponentPGS newPgs = new DEEntityTreeComponentPGS();
            newPgs.Version = pgs.Version;

            foreach (DEEntityTreeEntryPgs.Ary ary in pgs.ary)
                newPgs.Ary.Add(new DEEntityTreeComponentPGS.PGSEntry() { Value = JsonConvert.SerializeObject(ary, Formatting.Indented) });

            comp.PGS[i] = newPgs;
        }

        //comp.PGS = entry.Own.PGS;
        comp.FD = entry.Own.FD;
        comp.FG = entry.Own.FG;
        comp.DS = entry.Own.DS;
        comp.SRA = entry.Own.SRA;


        if (entry.Own.UID != 0)
        {
            string ownKind = m_entityKinds[DEEntityUtils.ExtractEntityKindFromUID(entry.Own.UID)];
            string ownStageName;

            if (!IsDE1)
                ownStageName = m_stages[DEEntityUtils.ExtractStageIDFromDS(entry.Own.DS)];
            else
                ownStageName = entry.Own.Stage;

            GameObject model = null;

            switch (ownKind)
            {
                case "dispose_folder":
                case "dispose_workgroup":
                    DestroyImmediate(t.GetComponent<MeshRenderer>());
                    break;


                case "character":
                    model = Instantiate(Resources.Load<GameObject>("pac_character"));

                    model.transform.parent = t.transform;
                    model.transform.localPosition = Vector3.zero;
                    break;

                case "asset_unit":
                    t.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("casset_unit_material");
                    break;

            }

            if(model != null)
            {
                model.transform.localScale = new Vector3(1, 1, 1);
                t.transform.localScale = new Vector3(1, 1, 1);
                DestroyImmediate(t.GetComponent<MeshRenderer>());
            }


            byte folder = DEEntityUtils.ExtractEntityFolderFromUID(entry.Own.UID);

            string filePath;

            if (entry.Own.Version > 5)
                filePath = Path.Combine(EntityDirectory, ownStageName, ownKind, folder.ToString("x2"), name + ".txt");
            else
                filePath = Path.Combine(EntityDirectory, ownStageName, ownKind, name + ".txt");

            if (File.Exists(filePath))
            {
                DEEntityComponent entityComp = t.gameObject.AddComponent<DEEntityComponent>();
                entityComp.EntityData = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(File.ReadAllText(filePath)), Formatting.Indented);
            }
        }

        foreach (DEEntityTreeEntry child in entry.Children)
        {
            Transform childTransform = EntityCreationRecursion(child);
            if (childTransform != null)
            {
                string childKind = m_entityKinds[DEEntityUtils.ExtractEntityKindFromUID(child.Own.UID)];
                string stageName;

                if (!IsDE1)
                    stageName = m_stages[DEEntityUtils.ExtractStageIDFromDS(child.Own.DS)];
                else
                    stageName = child.Own.Stage;

                if (!entityKindFolders.ContainsKey(stageName))
                {
                    Transform stageTransform = new GameObject(stageName).transform;
                    stageTransform.parent = t;

                    StageDat dat = new StageDat();
                    dat.Transform = stageTransform;
                    dat.EntityKindFolders = new Dictionary<string, Transform>();

                    entityKindFolders[stageName] = dat;
                }

                StageDat stageDat = entityKindFolders[stageName];

                if (!stageDat.EntityKindFolders.ContainsKey(childKind))
                {
                    Transform entityKindFolder = new GameObject(childKind).transform;
                    entityKindFolder.transform.parent = stageDat.Transform;
                    stageDat.EntityKindFolders[childKind] = entityKindFolder;
                }


                childTransform.parent = stageDat.EntityKindFolders[childKind];
                childTransform.position = new Vector3(child.Own.Position[0], child.Own.Position[1], child.Own.Position[2]);
                childTransform.localRotation = new Quaternion(child.Own.Orient[0], child.Own.Orient[1], child.Own.Orient[2], child.Own.Orient[3]);
            }
        }

        return t;
    }

    private DEEntityTreeEntry EntityTreeExportRecursion(DEEntityTreeComponent comp)
    {
        DEEntityTreeEntry entry = new DEEntityTreeEntry();
        entry.Own.DS = comp.DS;
        entry.Own.FD = comp.FD;
        entry.Own.FG = comp.FG;
        entry.Own.PGS = new DEEntityTreeEntryPgs[comp.PGS.Length];// comp.PGS

        entry.Own.Position = new float[4];
        entry.Own.Position[0] = transform.localPosition.x;
        entry.Own.Position[1] = transform.localPosition.y;
        entry.Own.Position[2] = transform.localPosition.z;

        entry.Own.Orient = new float[4];
        entry.Own.Orient[0] = transform.localRotation.x;
        entry.Own.Orient[1] = transform.localRotation.y;
        entry.Own.Orient[2] = transform.localRotation.z;
        entry.Own.Orient[3] = transform.localRotation.w;
        entry.Own.SRA = comp.SRA;

        entry.Own.Version = comp.Version;
        entry.Own.UID = ulong.Parse(comp.name.ToString(), System.Globalization.NumberStyles.HexNumber);

        for (int i = 0; i < entry.Own.PGS.Length; i++)
        {
            DEEntityTreeComponentPGS entPgs = comp.PGS[i];

            DEEntityTreeEntryPgs pgs = new();
            pgs.Version = entPgs.Version;

            foreach (DEEntityTreeComponentPGS.PGSEntry upgs in entPgs.Ary)
            {
                string value = upgs.Value;
                DEEntityTreeEntryPgs.Ary serializedPgsAry = JsonConvert.DeserializeObject<DEEntityTreeEntryPgs.Ary>(upgs.Value);
                pgs.ary.Add(serializedPgsAry);
            }

            entry.Own.PGS[i] = pgs;
        }

        if (!OnlyImportTargetStageEntities)
        {
            for (int i = 0; i < comp.transform.childCount; i++)
            {
                Transform stageTransform = comp.transform.GetChild(i);

                for (int k = 0; k < stageTransform.childCount; k++)
                {
                    Transform entityKindTransform = stageTransform.GetChild(k);

                    for (int j = 0; j < entityKindTransform.childCount; j++)
                    {
                        Transform entityTransform = entityKindTransform.GetChild(j);
                        DEEntityTreeComponent entityComponent = entityTransform.GetComponent<DEEntityTreeComponent>();

                        if (entityComponent != null)
                        {
                            DEEntityTreeEntry treeEntry = EntityTreeExportRecursion(entityComponent);
                            entry.Children.Add(treeEntry);
                        }
                    }
                }
            }
        }

        return entry;
    }

    private JObject EntityExport(DEEntityComponent entityComponent)
    {
        return (JObject)JsonConvert.DeserializeObject(entityComponent.EntityData);
    }
}
