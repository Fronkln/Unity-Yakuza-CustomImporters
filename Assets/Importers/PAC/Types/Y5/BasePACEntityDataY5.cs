using System;
using UnityEngine;
using Yarhl.IO;

[Serializable]
public class BasePACEntityDataY5
{
    public Vector3 Position;
    public short Angle;
    public byte Data2Count;
    public byte Flags;

    public byte[] UnreadData;

    //Better-ish way to universally serialize data, inspired from S2U
    public PACEntityKeyValueDictionary KeyValues = new PACEntityKeyValueDictionary();

    public virtual void ReadData(DataReader reader)
    {

    }

    public virtual void WriteData(DataWriter writer)
    {

    }

    public static BasePACEntityDataY5 Read(DataReader reader, bool isOOE, int entityUID, BasePACEntityDataY5 first, byte propertyType, int index, int size, int totalCount, DisposeString disposeString = null)
    {
        BasePACEntityDataY5 data = null;

        switch (propertyType)
        {
            default:
                data = new BasePACEntityDataY5();
                break;

            //character
            case 0:
                bool isFighter = false;
                bool isFighterFlag = false;

                if (index <= 0)
                {
                    reader.Stream.RunInPosition(delegate
                    {
                        isFighterFlag = reader.ReadByte() != 0;
                    }, 35, SeekMode.Current);
                }

                data = new BasePACEntityDataY5();
                data.Position = reader.ReadPXDVector3();
                data.Angle = reader.ReadInt16();
                data.Data2Count = reader.ReadByte();
                data.Flags = reader.ReadByte();

                data.KeyValues.AddKeyValue("behavior", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.AddKeyValue("model", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.AddKeyValue("animation", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.AddKeyValue("stance", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.AddKeyValue("flags1", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("height", reader.ReadByte().ToString(), KeyValueType.Byte);

                isFighter = isFighterFlag;



                if (isFighterFlag && totalCount == 1)
                    data.UnreadData = reader.ReadBytes(51);
                else
                {
                    if (isFighterFlag || isFighter && index != totalCount - 1)
                    {
                        data.UnreadData = reader.ReadBytes(47);
                    }
                    else
                    {
                        if (!isFighterFlag && !isFighter)
                            data.UnreadData = reader.ReadBytes(39);
                        else
                            data.UnreadData = reader.ReadBytes(79);
                    }
                }

                return data;

            case 5:
                data = new BasePACEntityDataY5();
                data.Position = reader.ReadPXDVector3();
                data.Angle = reader.ReadInt16();
                data.Data2Count = reader.ReadByte();
                data.Flags = reader.ReadByte();

                data.KeyValues.AddKeyValue("model", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.AddKeyValue("name", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.AddKeyValue("battle_intro", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.AddKeyValue("boss_node_id", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk1", reader.ReadInt16().ToString(), KeyValueType.Short);
                data.KeyValues.AddKeyValue("ctrltype", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("unk2", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("character_type", reader.ReadByte().ToString(), KeyValueType.Byte);
                
                data.KeyValues.AddKeyValue("unk3", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk4", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk7", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("unk8", reader.ReadInt16().ToString(), KeyValueType.Short);
                
                data.KeyValues.AddKeyValue("fight_node_id1", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("fight_node_id2", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk9", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk10", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk11", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk12", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk13", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk14", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("unk15", reader.ReadInt32().ToString(), KeyValueType.Int);

                return data;
            case 0x2A:
                data = new BasePACEntityDataY5();
                data.Position = reader.ReadPXDVector3();
                data.Angle = reader.ReadInt16();
                data.Data2Count = reader.ReadByte();
                data.Flags = reader.ReadByte();

                data.KeyValues.AddKeyValue("ai_param1", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param2", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param3", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param4", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param5", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param6", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param7", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param8", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param9", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.AddKeyValue("ai_param10", reader.ReadByte().ToString(), KeyValueType.Byte);

                short enemyCount = reader.ReadInt16();
                data.KeyValues.AddKeyValue("enemy_count", enemyCount.ToString(), KeyValueType.Short);

                data.KeyValues.AddKeyValue("boss_ptr", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("pad1", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("pad2", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.AddKeyValue("pad3", reader.ReadInt32().ToString(), KeyValueType.Int);


                for (int i = 0; i < enemyCount; i++)
                    data.KeyValues.AddKeyValue("enemy" + i + "_uid", reader.ReadInt32().ToString(), KeyValueType.Int);

                for (int i = 0; i < enemyCount; i++)
                    data.KeyValues.AddKeyValue("enemy" + i + "_idx", reader.ReadInt16().ToString(), KeyValueType.Short);

                return data;
        }

        if (data == null)
            data = new BasePACEntityDataY5();

        long dataStart = reader.Stream.Position;
        long dataEnd = reader.Stream.Position + size;

        data.Position = reader.ReadPXDVector3();
        data.Angle = reader.ReadInt16();
        data.Data2Count = reader.ReadByte();
        data.Flags = reader.ReadByte();

        data.ReadData(reader);

        long postReadPos = reader.Stream.Position;

        if (postReadPos < dataEnd)
            data.UnreadData = reader.ReadBytes((int)(dataEnd - postReadPos));
        else if (postReadPos > dataEnd)
            throw new System.Exception("Overread");


        return data;
    }
}
