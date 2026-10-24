using Yarhl.IO;

[System.Serializable]
public class PACConditionChunkY5 
{
    public int Type;
    public byte[] UnreadData = new byte[0];
    public KeyValueDictionary ConditionParams = new KeyValueDictionary();

    public void ReadConditionParams(DataReader reader)
    {
        switch(Type)
        {
            case 4:
                ConditionParams.AddKeyValue("state_category", reader.ReadInt16().ToString(), KeyValueType.Short);
                ConditionParams.AddKeyValue("state_id", reader.ReadInt16().ToString(), KeyValueType.Short);
                ConditionParams.AddKeyValue("complete", reader.ReadInt32().ToString(), KeyValueType.Int);
                break;

        }
    }
}
