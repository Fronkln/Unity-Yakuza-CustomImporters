using System.Collections.Generic;

[System.Serializable]
public class PACRef
{
    public short TextToggle;
    public string Text = "";

    public List<MsgProperty> MsgProperties = new List<MsgProperty>();
}
