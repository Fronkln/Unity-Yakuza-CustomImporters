using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.Collections.Generic;
using System.IO;
using Yarhl.IO;
using System.Text;
using System.Xml;

[ScriptedImporter(1, "pac")]
public class PACCustomImporter : ScriptedImporter
{
    private AssetImportContext m_ctx;
    private DataReader m_reader = null;
    private DataStream m_readStream = null;

    private ushort m_entityCount = 0;
    private ushort m_stringCount = 0;
    private int m_dataStartOffset = 0;
    private int m_stringPtrTableOffset = 0;

    private string[] m_stringTable = null;
    private BasePACEntity[] m_entities = null;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        byte[] fileBuffer = File.ReadAllBytes(ctx.assetPath);

        m_ctx = ctx;
        m_readStream = DataStreamFactory.FromArray(fileBuffer, 0, fileBuffer.Length);
        m_reader = new DataReader(m_readStream) { DefaultEncoding = Encoding.GetEncoding(932), Endianness = EndiannessMode.BigEndian };

        m_entityCount = m_reader.ReadUInt16();
        m_stringCount = m_reader.ReadUInt16();
        m_dataStartOffset = m_reader.ReadInt32();
        m_stringPtrTableOffset = m_reader.ReadInt32();

        ReadStringTable();
        ReadEntities();


        Transform root = new GameObject("pac").transform;
        m_ctx.AddObjectToAsset("pac_root", root.gameObject);
        m_ctx.SetMainObject(root.gameObject);

        for(int i = 0; i < m_entities.Length; i++)
        {
            BasePACEntity entity = m_entities[i];
            
            Transform pacEntObj = new GameObject($"Entity {i} (Type {entity.Type})").transform;
            pacEntObj.transform.position = entity.Position;
            pacEntObj.transform.parent = root;
            pacEntObj.transform.eulerAngles = new Vector3(0, OERotationY.ToAngle(entity.RotY), 0);


            if (entity.Type == 2)
            {
                GameObject charaAsset = Instantiate(Resources.Load<GameObject>("pac_character"));
                charaAsset.transform.name = "preview";
                charaAsset.transform.parent = pacEntObj;
                charaAsset.transform.localPosition = Vector3.zero;
                charaAsset.transform.localRotation = Quaternion.identity;
                m_ctx.AddObjectToAsset($"entity_{i}_preview", charaAsset);
            }
            else
            {
                GameObject charaAsset = GameObject.CreatePrimitive(PrimitiveType.Cube);
                charaAsset.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                charaAsset.transform.name = "preview";
                charaAsset.transform.parent = pacEntObj;
                charaAsset.transform.localPosition = Vector3.zero;
                charaAsset.transform.localRotation = Quaternion.identity;
                m_ctx.AddObjectToAsset($"entity_{i}_preview", charaAsset);
            }

            m_ctx.AddObjectToAsset($"entity_{i}", pacEntObj);
        }
    }

    private void ReadStringTable()
    {
        m_reader.Stream.Seek(m_stringPtrTableOffset);

        //Process string table
        int[] stringPtrs = new int[m_stringCount];
        m_stringTable = new string[m_stringCount];

        for (int i = 0; i < stringPtrs.Length; i++)
            stringPtrs[i] = m_reader.ReadInt32();

        for (int i = 0; i < stringPtrs.Length; i++)
        {
            m_reader.Stream.Seek(stringPtrs[i]);
            m_stringTable[i] = m_reader.ReadString();
        }
    }

    private void ReadEntities()
    {
        m_reader.Stream.Seek(m_dataStartOffset);

        m_entities = new BasePACEntity[m_entityCount];

        for(int i = 0; i < m_entities.Length; i++)
        {
            PACEntityTypeY3 type = (PACEntityTypeY3)m_reader.ReadUInt16();
            ushort id = m_reader.ReadUInt16();
            
            int cccDataPtr = m_reader.ReadInt32();
            int entityDataPtr = m_reader.ReadInt32();
            
            ushort cccDataSize = m_reader.ReadUInt16();
            ushort entityDataSize = m_reader.ReadUInt16();

            BasePACEntity entity = CreatePACEntity(type);
            entity.Type = (ushort)type;
            entity.ID = id;
            entity.CCC = new PACEntityCCC();

            long entityEndPos = m_reader.Stream.Position;

            m_reader.Stream.Seek(cccDataPtr);
            long expectedCCCPos = m_reader.Stream.Position + cccDataSize;

            entity.CCC.Read(m_reader, m_stringTable);

            if (m_reader.Stream.Position < expectedCCCPos)
                entity.CCC.UnknownData = m_reader.ReadBytes((int)(expectedCCCPos - m_reader.Stream.Position));

            m_reader.Stream.Seek(entityDataPtr);
            long expectedEntityDataPos = m_reader.Stream.Position + entityDataSize;

            entity.ProcessEntityData(m_reader, m_stringTable);

            if (m_reader.Stream.Position < expectedEntityDataPos)
                entity.UnknownData = m_reader.ReadBytes((int)(expectedEntityDataPos - m_reader.Stream.Position));

            m_entities[i] = entity;
            m_reader.Stream.Seek(entityEndPos);
        }
    }

    private BasePACEntity CreatePACEntity(PACEntityTypeY3 type)
    {
        switch(type)
        {
            default:
                return new BasePACEntity();
            case PACEntityTypeY3.Character:
                return new PACEntityY3Character();
        }
    }
}
