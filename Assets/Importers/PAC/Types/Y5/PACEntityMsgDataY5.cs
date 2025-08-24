using System;
using System.Collections.Generic;

[Serializable]
public struct PACEntityMsgDataY5
{
    public byte[] Identifier;
    public List<MsgPosition> Positions;
    public string[] Strings;

    public List<PACMsgGroup> Groups;
}
