using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using Yarhl.IO;
using System;

public class DisposeString : ScriptableObject
{
    public string[] Strings;

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

        dString.Strings = strings;

        return dString;
    }

    //Static function to use on keyvalues stuff on custom importers, a bit jank but oh well
    public static string Get(DisposeString dispose, int index)
    {
        if (dispose == null)
            return index.ToString();

        if (index < 0 || index >= dispose.Strings.Length)
            return index.ToString();

        return dispose.Strings[index];
    }
}
