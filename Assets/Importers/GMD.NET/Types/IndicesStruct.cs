using Yarhl.IO;
using Yarhl.IO.Serialization.Attributes;

[Serializable]
public class IndicesStruct
{
    public int IndexCount { get; set; }
    public int IndexOffset { get; set; }

    public ushort[] ReadIndices(DataReader reader, uint startPos)
    {
        ushort[] indices = new ushort[IndexCount];

        reader.Stream.RunInPosition(delegate
        {
            for (int i = 0; i < IndexCount; i++)
            {
                indices[i] = reader.ReadUInt16();
            }

        }, startPos, SeekMode.Start);

        return indices;
    }
}
