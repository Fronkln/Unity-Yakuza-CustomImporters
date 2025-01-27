using Newtonsoft.Json.Linq;
using UnityEngine;

public unsafe static class DEEntityUtils
{
    public static float ParseIntAsFloat(int value)
    {
        return *(float*)&value;
    }

    public static int ParseFloatAsInt(float value)
    {
        return *(int*)&value;
    }
    public static ushort ExtractEntityKindFromUID(ulong uid)
    {
        ulong shiftedValue = uid >> 32;
        ulong extractedValue = shiftedValue & 0xFFF;

        return (ushort)extractedValue;
    }

    public static byte ExtractEntityFolderFromUID(ulong uid)
    {
        string uidText = $"{uid:X16}";
        string extract = uidText.Substring(uidText.Length - 2, 2);
        return byte.Parse(extract, System.Globalization.NumberStyles.HexNumber);
    }

    public static byte ExtractStageIDFromDS(ulong ds)
    {
        string dsText = $"{ds:X8}";
        string extract = dsText.Substring(0, 2);
        return byte.Parse(extract, System.Globalization.NumberStyles.HexNumber);
    }
}
