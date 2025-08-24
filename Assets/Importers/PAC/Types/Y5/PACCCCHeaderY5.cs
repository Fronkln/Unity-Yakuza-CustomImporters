using UnityEngine;

//Literally msg header
public struct PACCCCHeaderY5
{
    public byte[] Identifier;
    public byte GroupsCount;
    public int GroupsPtr;
    public int PositionsPtr;
    public ushort PositionsCount;
    public ushort StringCount;
    public int StringTablePtr;
    public int Unk1Ptr;
}
