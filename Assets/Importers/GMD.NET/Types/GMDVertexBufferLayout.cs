using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Yarhl.IO.Serialization.Attributes;

using u8 = System.Byte;
using u32 = System.UInt32;
using u64 = System.UInt64;


[Serializable]
public class GMDVertexBufferLayout {
    public u32 Index;
    public u32 VertexCount;
    /// <summary>
    /// This is a bitfield that needs to be parsed with GMDVertexFormat.Deserialize
    /// </summary>
    public u64 VertexFormat;
    public SizedPointer VertexData;
    public u32 BytesPerVertex;
}
