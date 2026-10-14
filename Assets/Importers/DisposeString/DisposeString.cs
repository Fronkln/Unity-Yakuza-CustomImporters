using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using Yarhl.IO;
using System;

public class DisposeString : ScriptableObject
{
    public List<string> Strings;

    public static DisposeString Read(string filePath)
    {
        DisposeString dString = (DisposeString)ScriptableObject.CreateInstance("DisposeString");

        byte[] fileBuffer = File.ReadAllBytes(filePath);

        var readStream = DataStreamFactory.FromArray(fileBuffer, 0, fileBuffer.Length);
        var reader = new DataReader(readStream) { DefaultEncoding = Encoding.GetEncoding(932), Endianness = EndiannessMode.BigEndian };

        short count = reader.ReadInt16();
        reader.Stream.Position += 2;
        int startPtr = reader.ReadInt32();

        reader.Stream.Position = startPtr;

        int[] pointers = new int[count];
        string[] strings = new string[count];

        for (int i = 0; i < count; i++)
            pointers[i] = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            int ptr = pointers[i];
            reader.Stream.Position = ptr;

            strings[i] = reader.ReadString();
        }

        dString.Strings = strings.ToList();

        return dString;
    }

    public static void Write(string outputPath, IEnumerable<string> strings)
    {
        int stringsCount = strings.Count();

        DataWriter writer = new DataWriter(new DataStream()) { Endianness = EndiannessMode.BigEndian, DefaultEncoding = Encoding.GetEncoding(932)};
        writer.Write((ushort)stringsCount);
        writer.Write((ushort)0);
        writer.Write(8);

        writer.WriteTimes(0, stringsCount * 4);

        int[] pointers = new int[stringsCount];

        for (int i = 0; i < stringsCount; i++)
        {
            pointers[i] = (int)writer.Stream.Position;
            writer.Write(strings.ElementAt(i));
        }

        writer.Stream.Position = 8;

        for(int i = 0; i < stringsCount; i++)
            writer.Write(pointers[i]);

        writer.Align(32);

        File.WriteAllBytes(outputPath, writer.Stream.ToArray());
    }

    //Static function to use on keyvalues stuff on custom importers, a bit jank but oh well
    public static string Get(DisposeString dispose, int index)
    {
        if (dispose == null)
            return index.ToString();

        if (index < 0 || index >= dispose.Strings.Count)
            return index.ToString();

        return dispose.Strings[index];
    }
}
