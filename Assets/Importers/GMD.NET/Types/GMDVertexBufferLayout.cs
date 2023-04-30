using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Yarhl.IO.Serialization.Attributes;

using u8 = System.Byte;
using u32 = System.UInt32;
using u64 = System.UInt64;


[Serializable]
public class GMDVertexBufferLayout {
    public u32 Index { get; set; }
    public u32 VertexCount { get; set; }
    /// <summary>
    /// This is a bitfield that needs to be parsed with GMDVertexFormat.Deserialize
    /// </summary>
    public u64 VertexFormat { get; set; }
    public SizedPointer VertexData { get; set; }
    public u32 BytesPerVertex { get; set; }

    // Not part of the structure, parsed by reading algorithm
    public GMDVertexBuffer VertexBuffer;
}
