[System.Serializable]
public struct GCTShapeHeader
{
    public uint Flags;
    public uint Attributes;


    //--------Attributes
    public byte GetMaterialType()
    {
        return (byte)new Bits32(8, 8).ExtractFrom(Attributes);
    }

    public byte GetControl()
    {
        return (byte)new Bits32(0, 8).ExtractFrom(Attributes);
    }

    //--------Flags
    //4 bits
    public GCTShapeType GetShapeType()
    {
        return (GCTShapeType)(Flags & 0xF);
    }

    //4 bits
    public byte GetEdgeFlags()
    {
        return (byte)new Bits32(28, 4).ExtractFrom(Flags);
    }

    //18 bits
    public uint GetShapeID()
    {
        return new Bits32(4, 18).ExtractFrom(Flags);
    }

    //18 bits
    public uint GetNodeID()
    {
        return new Bits32(22, 6).ExtractFrom(Flags);
    }
}
