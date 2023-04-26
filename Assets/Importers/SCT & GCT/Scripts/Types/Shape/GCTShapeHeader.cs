public struct GCTShapeHeader
{
    public int Flags;
    public int Attributes;

    public GCTShapeType GetShapeType()
    {
        return (GCTShapeType)(Flags & 0xF);
    }
}
