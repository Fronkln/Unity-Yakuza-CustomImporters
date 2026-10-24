using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Yarhl.IO;

public class PlayerPosExporter : MonoBehaviour
{
    public string ExportPath;


    public void Export()
    {
        if (string.IsNullOrEmpty(ExportPath))
            return;

        PlayerPosComponent[] positions = transform.GetComponentsInChildren<PlayerPosComponent>();

        DataWriter writer = new DataWriter(new DataStream()) { Endianness = EndiannessMode.BigEndian, DefaultEncoding = Encoding.GetEncoding(932) };
        writer.Write(16);
        writer.Write(positions.Length);
        writer.WriteTimes(0, 8);

        List<int> namePointers = new List<int>();

        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];

            writer.Write(0);
            writer.WritePXDVector3(pos.transform.position);
            writer.Write((int)OERotationY.ToOERotation(-pos.transform.eulerAngles.y));
            writer.Write(pos.StageID);
            writer.WritePXDVector3(pos.transform.Find("unknown position").position);
            writer.Write(pos.Daytime);
            writer.WriteTimes(0, 8);
        }
        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];

            namePointers.Add((int)writer.Stream.Position);
            writer.Write(pos.name);
        }

        writer.Align(32);

        writer.Stream.Position = 16;

        for (int i = 0; i < positions.Length; i++)
        {
            writer.Stream.Position = 16 + (i * 48);
            writer.Write(namePointers[i]);
        }

        File.WriteAllBytes(ExportPath, writer.Stream.ToArray());
    }
}
