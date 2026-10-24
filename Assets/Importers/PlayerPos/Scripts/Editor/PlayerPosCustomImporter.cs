using UnityEngine;
using Yarhl.IO;
using System.IO;
using System.Text;


#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


[ScriptedImporter(1, "ps")]
public class PlayerPosCustomImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        //var dispose = DisposeString.Read(ctx.assetPath);
        //ctx.AddObjectToAsset("dispose_string", dispose);
        //ctx.SetMainObject(dispose);
        byte[] fileBuffer = File.ReadAllBytes(ctx.assetPath);

        var readStream = DataStreamFactory.FromArray(fileBuffer, 0, fileBuffer.Length);
        var reader = new DataReader(readStream) { DefaultEncoding = Encoding.GetEncoding(932), Endianness = EndiannessMode.BigEndian };

        int posStart = reader.ReadInt32();
        int count = reader.ReadInt32();

        reader.Stream.Position = posStart;

        GameObject root = new GameObject("player_pos");

        ctx.AddObjectToAsset("root", root);

        for(int i = 0; i < count; i++)
        {
            GameObject pos = new GameObject();
            reader.Stream.RunInPosition(delegate { pos.name = reader.ReadString(); }, reader.ReadInt32());

            pos.transform.parent = root.transform;
            pos.transform.position = reader.ReadPXDVector3();
            pos.transform.eulerAngles = new Vector3(0, OERotationY.ToAngle((ushort)reader.ReadInt32()), 0);

            PlayerPosComponent posComponent = pos.gameObject.AddComponent<PlayerPosComponent>();
            posComponent.StageID = reader.ReadInt32();

            Vector3 unkPos = reader.ReadPXDVector3();
            posComponent.Daytime = reader.ReadInt32();

            reader.Stream.Position += 8;

            GameObject unknownPos = new GameObject("unknown position");
            unknownPos.transform.parent = pos.transform;
            unknownPos.transform.position = unkPos;


            var model = Instantiate(Resources.Load<GameObject>("playerpos_character"));

            model.transform.parent = pos.transform;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            ctx.AddObjectToAsset("pos_" + i, pos);
        }

        ctx.SetMainObject(root);
    }
}
