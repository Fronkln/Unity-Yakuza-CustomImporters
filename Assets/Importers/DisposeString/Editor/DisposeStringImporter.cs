using UnityEngine;
using Yarhl.IO;
using System.IO;
using System.Text;


#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


[ScriptedImporter(1, "ds")]
public class DisposeStringImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var dispose = DisposeString.Read(ctx.assetPath);
        ctx.AddObjectToAsset("dispose_string", dispose);
        ctx.SetMainObject(dispose);
    }
}
