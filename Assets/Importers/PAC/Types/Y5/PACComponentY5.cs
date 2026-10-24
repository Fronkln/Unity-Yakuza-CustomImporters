using UnityEngine;

public class PACComponentY5 : MonoBehaviour
{
    public BasePACEntityDataY5[] BaseEntityData;
    public PACEntityMsgDataY5 MsgData;

    public virtual void PostDataRead() 
    { 
    }

    public virtual void PostPACRead(PACComponentY5[] entities)
    {
    }

    public virtual void PreprocessExport(PACComponentY5[] entities, bool isOOE)
    {

    }
}
