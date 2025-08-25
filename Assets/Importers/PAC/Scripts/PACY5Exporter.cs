using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Resolvers;
using UnityEngine;
using Yarhl.IO;

//TODO: Write ref strings, also importer only reads one ref, but there are multiple! its an array
public class PACY5Exporter : MonoBehaviour
{
    public string Path;
    public void Export()
    {
        PACComponentY5[] entities = transform.GetComponentsInChildren<PACComponentY5>();

        DataWriter writer = new DataWriter(new DataStream()) { Endianness = EndiannessMode.BigEndian };

        writer.Write((ushort)entities.Length);
        writer.WriteTimes(0, 2);
        writer.Write((int)writer.Stream.Position + 4);

        long nodesStart = writer.Stream.Position;


        Dictionary<PACComponentY5, long> headerLocations = new Dictionary<PACComponentY5, long>();
        Dictionary<PACComponentY5, long> entityDataLocations = new Dictionary<PACComponentY5, long>();
        Dictionary<PACComponentY5, int> entityDataSizes = new Dictionary<PACComponentY5, int>();
        Dictionary<PACComponentY5, long> msgLocations = new Dictionary<PACComponentY5, long>();
        Dictionary<PACComponentY5, int> msgSizes = new Dictionary<PACComponentY5, int>();

        foreach (PACComponentY5 entity in entities)
        {
            if (entity.BaseEntityData.Length > 0)
            {
                entity.BaseEntityData[0].Position = entity.transform.position;
                entity.BaseEntityData[0].Angle = (short)OERotationY.ToOERotation(entity.transform.eulerAngles.y);
            }

            headerLocations[entity] = writer.Stream.Position;

            int uid = int.Parse(entity.transform.name, System.Globalization.NumberStyles.HexNumber);
            writer.Write(uid);
            writer.Write(0);
            writer.Write(0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
        }

        foreach (PACComponentY5 entity in entities)
        {
            long msgStart = writer.Stream.Position;

            if (entity.MsgData.Groups.Count < 1)
                msgStart = 0;

            msgLocations[entity] = msgStart;

            writer.Write(entity.MsgData.Identifier);
            writer.Write((byte)entity.MsgData.Groups.Count);
            writer.Write(24);
            //Coords
            writer.Write(0);
            writer.Write((ushort)entity.MsgData.Positions.Count);
            //String Table
            writer.Write((ushort)entity.MsgData.Strings.Length);
            writer.Write(0);

            writer.Write(0);

            long groupPtrsStart = writer.Stream.Position;

            Dictionary<PACMsgGroup, long> groupHeaderLocations = new Dictionary<PACMsgGroup, long>();
            Dictionary<PACMsgGroup, Dictionary<PACRef, long>> groupRefLocations = new Dictionary<PACMsgGroup, Dictionary<PACRef, long>>();
            Dictionary<PACMsgGroup, long> groupConditionLocations = new Dictionary<PACMsgGroup, long>();
            Dictionary<PACMsgGroup, Dictionary<PACRef, long>> groupRefStringLocations = new Dictionary<PACMsgGroup, Dictionary<PACRef, long>>();

            foreach (PACMsgGroup group in entity.MsgData.Groups)
            {
                groupHeaderLocations[group] = writer.Stream.Position;

                writer.Write(0);
                writer.Write(0);
                writer.Write((byte)group.Conditions.Count);
                writer.Write((byte)group.Refs.Length);
                writer.Write(group.Unknown1);
                writer.Write(group.Unknown2);
                writer.Write(group.InteractionParameters);
            }
            foreach (PACMsgGroup group in entity.MsgData.Groups)
            {
                groupRefLocations[group] = new Dictionary<PACRef, long>();

                Dictionary<PACRef, long>  structLocations = new Dictionary<PACRef, long>();

                foreach (var refStruct in group.Refs)
                {
                    groupRefLocations[group][refStruct] = writer.Stream.Position;

                    writer.Write(refStruct.TextToggle);
                    writer.Write((byte)refStruct.MsgProperties.Count);
                    writer.Write((byte)0);

                    //string
                    writer.Write(0);
                    //ref struct address
                    writer.Write(0);
                }
                foreach (var refStruct in group.Refs)
                {
                    int refStructAddr = (int)(writer.Stream.Position - msgStart);
                    foreach (var refChunk in refStruct.MsgProperties)
                    {
                        writer.Write(refChunk.Unknown);
                        writer.Write(refChunk.Unknown2);
                        writer.Write(refChunk.Unknown3);
                        writer.Write(refChunk.Unknown4);
                    }

                    writer.Stream.RunInPosition(delegate
                    {
                        writer.Stream.Position += 8;
                        writer.Write(refStructAddr);
                    }, groupRefLocations[group][refStruct]);
                }

            }

            foreach (PACMsgGroup group in entity.MsgData.Groups)
            {
                groupConditionLocations[group] = writer.Stream.Position;

                foreach (var condition in group.Conditions)
                {
                    writer.Write(condition.Type);
                    writer.Write(condition.Unknown2);
                    writer.Write(condition.Unknown3);
                }

                if (group.Conditions.Count <= 0)
                    groupConditionLocations[group] = 0;
            }

            //TODO: write ref strings here
            foreach (PACMsgGroup group in entity.MsgData.Groups)
            {
                groupRefStringLocations[group] = new Dictionary<PACRef, long>();

                foreach (var refStruct in group.Refs)
                {
                    groupRefStringLocations[group][refStruct] = writer.Stream.Position;

                    if (!string.IsNullOrEmpty(refStruct.Text))
                        writer.Write(refStruct.Text);
                    else
                        writer.Write(0);
                }
            }


            long addittionalCoordsStart = writer.Stream.Position;

            if (entity.MsgData.Positions.Count <= 0)
                addittionalCoordsStart = 0;
            else
            {
                foreach (var pos in entity.MsgData.Positions)
                {
                    writer.Write(pos.Position.x);
                    writer.Write(pos.Position.y);
                    writer.Write(pos.Position.z);
                    writer.Write(pos.Unk);
                    writer.Write(pos.Angle);
                }
            }

            long stringTableStart = writer.Stream.Position;

            if (entity.MsgData.Strings.Length <= 0)
                stringTableStart = 0;
            else
            {
                int[] stringPositions = new int[entity.MsgData.Strings.Length];
                writer.WriteTimes(0, entity.MsgData.Strings.Length * 4);

                for (int i = 0; i < stringPositions.Length; i++)
                {
                    string str = entity.MsgData.Strings[i];
                    stringPositions[i] = (int)(writer.Stream.Position - msgStart);
                    writer.Write(str);
                }

                writer.Stream.RunInPosition(delegate
                {
                    foreach (int pos in stringPositions)
                        writer.Write(pos);
                }, stringTableStart);
            }

            //Finish up

            foreach (var kv in groupHeaderLocations)
            {

                writer.Stream.RunInPosition(delegate
                {
                    int condLocation = (int)(groupConditionLocations[kv.Key] - msgStart);
                    int refLocation = (int)(groupRefLocations[kv.Key][kv.Key.Refs[0]] - msgStart);

                    if (condLocation < 0)
                        condLocation = 0;

                    writer.Stream.Seek(kv.Value);
                    writer.Write(condLocation);
                    writer.Write(refLocation);
                }, kv.Value);
            }

            foreach(var kv in groupRefLocations)
            {
                foreach(var kv2 in groupRefLocations[kv.Key])
                {
                    writer.Stream.RunInPosition(delegate
                    {
                        int refStringPos = (int)(groupRefStringLocations[kv.Key][kv2.Key] - msgStart);
                        writer.Stream.Position += 4;
                        writer.Write(refStringPos);

                    }, kv2.Value);
                }
            }

            //Finish up header
            writer.Stream.RunInPosition(delegate
            {
                int coordsPos = (int)(addittionalCoordsStart - msgStart);
                int textPos = (int)(stringTableStart - msgStart);

                if (coordsPos <= 0)
                    coordsPos = 0;

                if(textPos <= 0)
                    textPos = 0;

                writer.Stream.Position += 8;
                writer.Write(coordsPos);
                writer.Stream.Position += 4;
                writer.Write(textPos);
            }, msgStart);


            long msgDataEnd = writer.Stream.Position;

            int msgDataSize = (int)(msgDataEnd - msgStart);

            if (msgStart <= 0)
                msgDataSize = 0;

            msgSizes[entity] = msgDataSize;

            long entityDataStart = writer.Stream.Position;
            entityDataLocations[entity] = entityDataStart;

            foreach(var entityDat in entity.BaseEntityData)
            {
                writer.Write(entityDat.Position.x);
                writer.Write(entityDat.Position.y);
                writer.Write(entityDat.Position.z);
                writer.Write(entityDat.Angle);
                writer.Write((byte)entity.BaseEntityData.Length);
                writer.Write(entityDat.Flags);
                writer.Write(entityDat.UnreadData);
            }

            long entityDataEnd = writer.Stream.Position;
            int size = (int)(entityDataEnd - entityDataStart);

            entityDataSizes[entity] = size;
            
        }


        //Finish up
        foreach (var kv in headerLocations)
        {
            writer.Stream.Seek(kv.Value);
            writer.Write(int.Parse(kv.Key.transform.name, System.Globalization.NumberStyles.HexNumber));

            if (msgLocations[kv.Key] > 0)
                writer.Write((int)msgLocations[kv.Key]);
            else
                writer.Write(0);

            writer.Write((int)entityDataLocations[kv.Key]);
            writer.Write((ushort)msgSizes[kv.Key]);
            writer.Write((ushort)entityDataSizes[kv.Key]);
            //writer.Write((ushort)(kv.Key.MsgData.Groups.Count > 0 ? 1 : 0));
        }


        writer.Stream.WriteTo(Path);
    }
}
