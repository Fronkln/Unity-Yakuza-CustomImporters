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

    public byte[] UnreadData = new byte[0];

    //Better-ish way to universally serialize data, inspired from S2U
    public PACEntityKeyValueDictionary KeyValues = new PACEntityKeyValueDictionary();

    public virtual void ReadData(DataReader reader)
    {

    }

    public virtual void WriteData(DataWriter writer)
    {

    }

    public static BasePACEntityDataY5[] Read(DataReader reader, bool isOOE, int entityUID, BasePACEntityDataY5 first, byte propertyType, int index, int size, int calcSizedPerData, int totalCount, DisposeString disposeString = null)
    {
        long start = reader.Stream.Position;
        long end = reader.Stream.Position + calcSizedPerData;

        BasePACEntityDataY5 data = null;

        switch ((PACEntityTypeY5)propertyType)
        {
            default:
                data = new BasePACEntityDataY5();
                break;

            //character
            case PACEntityTypeY5.Character:
                bool isFighter = false;
                bool isFighterFlag = false;

                if (index <= 0)
                {
                    reader.Stream.RunInPosition(delegate
                    {
                        isFighterFlag = reader.ReadByte() != 0;
                    }, 35, SeekMode.Current);
                }

                isFighter = isFighterFlag;


                BasePACEntityDataY5 ReadCharacterData()
                {
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
                    data.KeyValues.AddKeyValue("unk1", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("height", reader.ReadByte().ToString(), KeyValueType.Byte);

                    data.KeyValues.AddKeyValue("unk2", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk3", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk4", reader.ReadByte().ToString(), KeyValueType.Byte);

                    data.KeyValues.AddKeyValue("unk5", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk6", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk7", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk8", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk9", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk10", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("item", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("unk11", reader.ReadInt32().ToString(), KeyValueType.Int);

                    return data;
                }


                BasePACEntityDataY5 ReadFighterDataLessThan204Bytes()
                {
                    data = new BasePACEntityDataY5();
                    data.Position = reader.ReadPXDVector3();
                    data.Angle = reader.ReadInt16();
                    data.Data2Count = reader.ReadByte();
                    data.Flags = reader.ReadByte();

                    if (isOOE)
                    {
                        data.KeyValues.AddKeyValue("behavior", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("model", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("animation", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("stance", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("flags1", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk1", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("height", reader.ReadByte().ToString(), KeyValueType.Byte);

                        data.KeyValues.AddKeyValue("unk2", reader.ReadByte().ToString(), KeyValueType.Byte);
                        data.KeyValues.AddKeyValue("unk3", reader.ReadByte().ToString(), KeyValueType.Byte);
                        data.KeyValues.AddKeyValue("unk4", reader.ReadByte().ToString(), KeyValueType.Byte);

                        data.KeyValues.AddKeyValue("unk5", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk6", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk7", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk8", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk9", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk10", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk11", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk12", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk13", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk14", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk15", reader.ReadInt32().ToString(), KeyValueType.Int);
                    }
                    else
                    {
                        data.KeyValues.AddKeyValue("pad_1", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("pad_2", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("pad_3", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk1", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("unk2", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk3", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk4", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk5", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk6", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk7", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk8", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("npc_type", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("unk9", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("unk10", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk11", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk12", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk13", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk21", reader.ReadInt16().ToString(), KeyValueType.Short);
                        data.KeyValues.AddKeyValue("ctrltype", reader.ReadByte().ToString(), KeyValueType.Byte);
                        data.KeyValues.AddKeyValue("unk14", reader.ReadByte().ToString(), KeyValueType.Byte);
                        data.KeyValues.AddKeyValue("unk15", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk16", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("unk17", reader.ReadByte().ToString(), KeyValueType.Byte);
                        data.KeyValues.AddKeyValue("unk18", reader.ReadByte().ToString(), KeyValueType.Byte);
                        data.KeyValues.AddKeyValue("unk19", reader.ReadInt16().ToString(), KeyValueType.Short);
                        data.KeyValues.AddKeyValue("unk20", reader.ReadInt32().ToString(), KeyValueType.Int);
                        data.KeyValues.AddKeyValue("name", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("weapon_1", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("battle_intro", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                        data.KeyValues.AddKeyValue("weapon_2", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    }

                    return data;
                }

                BasePACEntityDataY5[] entityDats = new BasePACEntityDataY5[totalCount];
                entityDats[0] = ReadCharacterData();

                if(!isFighter)
                {
                    for (int i = 1; i < totalCount; i++)
                        entityDats[i] = ReadCharacterData();
                }
                else
                {
                    if (totalCount <= 1)
                        return entityDats;

                    if(calcSizedPerData <= 204)
                    {
                        entityDats[0].UnreadData = reader.ReadBytes(8);

                        for (int i = 1; i < totalCount; i++)
                        {
                            entityDats[i] = ReadFighterDataLessThan204Bytes();

                            bool lastDat = i == totalCount - 1;

                            if (reader.Stream.Position < end)
                                entityDats[i].UnreadData = reader.ReadBytes((int)(end - reader.Stream.Position));
                        }
                    }
                    else
                    {

                    }
                }

                /*
                if (isFighterFlag && totalCount == 1)
                    data.UnreadData = reader.ReadBytes(51);
                else
                {
                    if (isFighterFlag || isFighter && index != totalCount - 1)
                    {
                        data.UnreadData = reader.ReadBytes(43);
                    }
                    else
                    {
                        if (!isFighterFlag && !isFighter)
                        {
                            data.UnreadData = reader.ReadBytes(75);
                        }
                        else
                            data.UnreadData = reader.ReadBytes(79);
                    }
                }
                */

                return entityDats;

            case PACEntityTypeY5.Fighter:
                data = new BasePACEntityDataY5();
                data.Position = reader.ReadPXDVector3();
                data.Angle = reader.ReadInt16();
                data.Data2Count = reader.ReadByte();
                data.Flags = reader.ReadByte();

                if (isOOE)
                {
                    data.KeyValues.AddKeyValue("model", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("name", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("battle_intro", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("boss_node_id", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk1", reader.ReadInt16().ToString(), KeyValueType.Short);
                    data.KeyValues.AddKeyValue("ctrltype", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk2", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("character_type", reader.ReadByte().ToString(), KeyValueType.Byte);

                    data.KeyValues.AddKeyValue("unk3", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk4", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk5", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk6", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk7", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk8", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk9", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk10", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk11", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk12", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk13", reader.ReadByte().ToString(), KeyValueType.Byte);

                    data.KeyValues.AddKeyValue("unk14", reader.ReadInt32().ToString(), KeyValueType.Int);

                    data.KeyValues.AddKeyValue("fight_node_id1", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("fight_node_id2", reader.ReadInt32().ToString(), KeyValueType.Int);

                    data.KeyValues.AddKeyValue("unk15", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk16", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk17", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk18", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk19", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk20", reader.ReadInt32().ToString(), KeyValueType.Int);
                }
                else
                {
                    data.KeyValues.AddKeyValue("model", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("name", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("battle_intro", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("boss_node_id", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk2", reader.ReadInt16().ToString(), KeyValueType.Short);
                    data.KeyValues.AddKeyValue("unk_flag1", reader.ReadInt16().ToString(), KeyValueType.Short);
                    data.KeyValues.AddKeyValue("ctrltype", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk3", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("character_type", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("hp", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk4", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk5", reader.ReadInt16().ToString(), KeyValueType.Short);
                    data.KeyValues.AddKeyValue("unk_flag2", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("height", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("fight_node_id1", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("fight_node_id2", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("unk6", reader.ReadInt32().ToString(), KeyValueType.Int);
                    data.KeyValues.AddKeyValue("fighter_flag", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk_flag3", reader.ReadByte().ToString(), KeyValueType.Byte);
                    data.KeyValues.AddKeyValue("unk7", reader.ReadInt16().ToString(), KeyValueType.Short);
                    data.KeyValues.AddKeyValue("weapon_1", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("weapon_2", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                    data.KeyValues.AddKeyValue("unk8", reader.ReadInt32().ToString(), KeyValueType.Int);
                }

                return new BasePACEntityDataY5[1] { data };
            case PACEntityTypeY5.Fight:
                data = new BasePACEntityDataY5();
                data.Position = reader.ReadPXDVector3();
                data.Angle = reader.ReadInt16();
                data.Data2Count = reader.ReadByte();
                data.Flags = reader.ReadByte();

                if (isOOE)
                {
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
                }
                else
                {
                    data.KeyValues.AddKeyValue("unk_flag1", reader.ReadInt16().ToString(), KeyValueType.Short);
                }

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

                return new BasePACEntityDataY5[1] { data };
        }

        if (data == null)
            data = new BasePACEntityDataY5();

        long dataStart = reader.Stream.Position;
        long dataEnd = reader.Stream.Position + calcSizedPerData;

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


        return new BasePACEntityDataY5[1] { data };
    }
}
