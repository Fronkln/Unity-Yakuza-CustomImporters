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

                data.KeyValues.Add("behavior", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.Add("model", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.Add("animation", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.Add("stance", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.Add("flags1", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("height", reader.ReadByte().ToString(), KeyValueType.Byte);

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

                data.KeyValues.Add("model", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.Add("name", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.Add("battle_intro", DisposeString.Get(disposeString, reader.ReadInt32()), KeyValueType.DisposeString);
                data.KeyValues.Add("boss_node_id", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk1", reader.ReadInt16().ToString(), KeyValueType.Short);
                data.KeyValues.Add("ctrltype", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.Add("unk2", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.Add("character_type", reader.ReadByte().ToString(), KeyValueType.Byte);
                
                data.KeyValues.Add("unk3", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk4", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk7", reader.ReadByte().ToString(), KeyValueType.Byte);
                data.KeyValues.Add("unk8", reader.ReadInt16().ToString(), KeyValueType.Short);
                
                data.KeyValues.Add("fight_node_id1", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("fight_node_id2", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk9", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk10", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk11", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk12", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk13", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk14", reader.ReadInt32().ToString(), KeyValueType.Int);
                data.KeyValues.Add("unk15", reader.ReadInt32().ToString(), KeyValueType.Int);

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
