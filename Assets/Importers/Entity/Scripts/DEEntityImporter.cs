using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;

public class DEEntityImporter : MonoBehaviour
{
    public string EntityDirectory;
    public string EntityTreePath;
    public TextAsset EntityKinds;
    public TextAsset Stages;

    public bool OnlyImportTargetStageEntities = false;

    private Dictionary<uint, string> m_entityKinds = new Dictionary<uint, string>();
    private Dictionary<uint, string> m_stages = new Dictionary<uint, string>();
    private Dictionary<string, StageDat> m_entityDirs = new Dictionary<string, StageDat>();

    public class StageDat
    {
        public Transform Transform = null;
        public Dictionary<string, Transform> EntityKindFolders = new Dictionary<string, Transform>();
    }


    public void Import()
    {
        if (!File.Exists(EntityTreePath))
        {
            Debug.LogWarning("File does not exist");
            return;
        }

        foreach (Transform t in transform)
            DestroyImmediate(t.gameObject);

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

        foreach (string s in dat)
        {
            string[] split = s.Split(" = ");
            m_stages[uint.Parse(split[1].Trim())] = split[0].Trim();
        }

        DEEntityTreeEntry root = JsonConvert.DeserializeObject<DEEntityTreeEntry>(File.ReadAllText(EntityTreePath));
        EntityCreationRecursion(root).transform.parent = transform;

        /*
        foreach (DEEntityTreeEntry entry in root.Children)
        {

            ushort kind = DEEntityUtils.ExtractEntityKindFromUID(entry.Own.UID);
            byte folder = DEEntityUtils.ExtractEntityFolderFromUID(entry.Own.UID);
            byte stageID = DEEntityUtils.ExtractStageIDFromDS(entry.Own.DS);
            string stageName = m_stages[stageID];
            string entityType = m_entityKinds[kind];
            string name = entry.Own.UID.ToString("x16");

            string filePath = Path.Combine(EntityDirectory, stageName, entityType, folder.ToString("x2"), name + ".txt");

            if (File.Exists(filePath))
            {
                if (!m_entityDirs.ContainsKey(stageName))
                {
                    StageDat sdat = new StageDat();
                    m_entityDirs[stageName] = new StageDat();

                    GameObject stageTransform = new GameObject(stageName);
                    stageTransform.transform.parent = transform;

                    sdat.Transform = stageTransform.transform;

                    m_entityDirs[stageName] = sdat;
                }

                StageDat stageData = m_entityDirs[stageName];

                if (!stageData.EntityKindFolders.ContainsKey(entityType))
                {
                    GameObject entityTransform = new GameObject(entityType);
                    entityTransform.transform.parent = stageData.Transform;

                    stageData.EntityKindFolders[entityType] = entityTransform.transform;
                }

                Transform workgroupTransform = new GameObject(name).transform;
                workgroupTransform.parent = stageData.EntityKindFolders[entityType].transform;

                 Dictionary<string, Transform> entityKindFolders = new Dictionary<string, Transform>();

                foreach(DEEntityTreeEntry centry in entry.Children)
                {
                    string childEntityType = m_entityKinds[DEEntityUtils.ExtractEntityKindFromUID(centry.Own.UID)];
                    if(!entityKindFolders.ContainsKey(childEntityType))
                    {
                        GameObject dir = new GameObject(childEntityType);
                        dir.transform.parent = workgroupTransform;
                        entityKindFolders[childEntityType] = dir.transform;
                    }
                }
            }
        }
        */
    }

    public void Export()
    {
        DEEntityTreeEntry root = EntityTreeExportRecursion(transform.GetComponentInChildren<DEEntityComponent>());
        string outputPath = Path.Combine(new FileInfo(EntityTreePath).Directory.FullName, "entity_tree_test.json");

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        File.WriteAllText(outputPath, JsonConvert.SerializeObject(root, Formatting.None));
    }

    private Transform EntityCreationRecursion(DEEntityTreeEntry entry)
    {
        Dictionary<string, StageDat> entityKindFolders = new Dictionary<string, StageDat>();

        string name = entry.Own.UID.ToString("X16");
        Transform t = new GameObject(name).transform;
        DEEntityComponent comp = t.gameObject.AddComponent<DEEntityComponent>();

        comp.Version = entry.Own.Version;

        comp.PGS = new DEEntityComponentPGS[entry.Own.PGS.Length];

        for (int i = 0; i < comp.PGS.Length; i++)
        {
            DEEntityTreeEntryPgs pgs = entry.Own.PGS[i];
            DEEntityComponentPGS newPgs = new DEEntityComponentPGS();
            newPgs.Version = pgs.Version;

            foreach (DEEntityTreeEntryPgs.Ary ary in pgs.ary)
                newPgs.Ary.Add(new DEEntityComponentPGS.PGSEntry() { Value = JsonConvert.SerializeObject(ary, Formatting.Indented) });

            comp.PGS[i] = newPgs;
        }

        //comp.PGS = entry.Own.PGS;
        comp.FD = entry.Own.FD;
        comp.FG = entry.Own.FG;
        comp.DS = entry.Own.DS;

        foreach (DEEntityTreeEntry child in entry.Children)
        {
            Transform childTransform = EntityCreationRecursion(child);
            if (childTransform != null)
            {
                string childKind = m_entityKinds[DEEntityUtils.ExtractEntityKindFromUID(child.Own.UID)];
                string stageName = m_stages[DEEntityUtils.ExtractStageIDFromDS(child.Own.DS)];

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

    private DEEntityTreeEntry EntityTreeExportRecursion(DEEntityComponent comp)
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
        entry.Own.SRA = new List<object>();
        
        entry.Own.Version = comp.Version;
        entry.Own.UID = ulong.Parse(comp.name.ToString(), System.Globalization.NumberStyles.HexNumber);

        for (int i = 0; i < entry.Own.PGS.Length; i++)
        {
            DEEntityComponentPGS entPgs = comp.PGS[i];

            DEEntityTreeEntryPgs pgs = new();
            pgs.Version = entPgs.Version;

            foreach (DEEntityComponentPGS.PGSEntry upgs in entPgs.Ary)
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
                        DEEntityComponent entityComponent = entityTransform.GetComponent<DEEntityComponent>();

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
}
