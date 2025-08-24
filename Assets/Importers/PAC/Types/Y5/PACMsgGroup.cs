using System.Collections.Generic;

[System.Serializable]
public class PACMsgGroup
{
    public byte Unknown1;
    public byte Unknown2;
    public int InteractionParameters;

    public List<PACConditionChunkY5> Conditions = new List<PACConditionChunkY5>();

    public PACRef[] Refs = new PACRef[0];
}
