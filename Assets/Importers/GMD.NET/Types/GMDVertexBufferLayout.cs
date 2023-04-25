using Yarhl.IO.Serialization.Attributes;

[Serializable]
public class GMDVertexBufferLayout
{
    public uint Index;
    public uint VertexCount;
    public int Flags;
    public int Format;
    public SizedPointer VertexData;
    public int BytesPerVertex;

    public GMDVertex[] Vertices;
}
