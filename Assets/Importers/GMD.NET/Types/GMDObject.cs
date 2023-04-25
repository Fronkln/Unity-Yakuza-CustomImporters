using Yarhl.IO.Serialization.Attributes;

[Serializable]
public class GMDObject
{
    public uint Index { get; set; }
    public uint NodeIndex1 { get; set; }
    public uint NodeIndex2 { get; set; }
    public uint DrawListRelativePointer { get; set; }
}
