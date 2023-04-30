using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarhl.IO;

public struct GMDHeader
{
    public string Magic;
    public byte FileEndian;
    public byte VertexEndian;
    public int Version;
    public int FileSize;

    //Not part of the header, included for easy access
    public GMDVersion DetectedVersion;

    public PXDHash ModelName;

    public SizedPointer NodesChunk;
    public SizedPointer ObjectChunk;
    public SizedPointer MeshChunk;
    public SizedPointer MaterialChunk;
    public SizedPointer MaterialParamsChunk;
    public SizedPointer MatrixListChunk;
    public SizedPointer VertexBufferChunk;
    public SizedPointer VertexBytesChunk;
    public SizedPointer MaterialNameChunk;
    public SizedPointer ShaderNameChunk;
    public SizedPointer NodeNameChunk;
    public SizedPointer IndicesChunk;

    public EndiannessMode VertexEndianness {
        get {
            switch (VertexEndian) {
                case 1:
                case 2:
                case 3:
                case 6:
                    return EndiannessMode.BigEndian;
                default:
                    return EndiannessMode.LittleEndian;
            }
        }
    }
}
