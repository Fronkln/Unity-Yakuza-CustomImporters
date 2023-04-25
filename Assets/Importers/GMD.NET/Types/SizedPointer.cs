using Yarhl.IO.Serialization.Attributes;


[Serializable]
public class SizedPointer
{
    public int Pointer { get; set; }
    public int Count { get; set; }
}
