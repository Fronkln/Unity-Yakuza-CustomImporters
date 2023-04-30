using UnityEngine;
using Yarhl.IO;

public abstract class GCTShape
{
    public GCTShapeHeader Header;


    public virtual void ReadData(DataReader reader)
    {

    }
    public virtual void WriteData(DataWriter writer)
    {

    }

}
