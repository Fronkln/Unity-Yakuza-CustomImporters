using System.Collections.Generic;

[System.Serializable]
public class PACRef
{
    public short TextToggle;
    public string Text = "";

    public List<PACRefChunk> RefChunks = new List<PACRefChunk>();
}
