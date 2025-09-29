using System.Collections.Generic;

[System.Serializable]
public class PACRef
{
    public short TextLength;
    public string Text;

    public List<MsgProperty> MsgProperties = new List<MsgProperty>();
}
