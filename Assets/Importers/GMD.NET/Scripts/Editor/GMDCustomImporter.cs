using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using Yarhl.IO;
using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[ScriptedImporter(1, "gmd")]
public class GMDCustomImporter : ScriptedImporter
{
    private GMDHeader Header = new GMDHeader();

    private GMDNode[] Nodes;
    private GMDObject[] Objects;
    private GMDMesh[] Meshes;
    private GMDVertexBufferLayout[] VertexBuffers;
    private byte[] Vertices;
    private ushort[] Indices;

    private string[] NodeNames;

    private AssetImportContext m_ctx;
    private DataReader m_reader = null;
    private DataStream m_readStream = null;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        byte[] fileBuffer = File.ReadAllBytes(ctx.assetPath);

        m_ctx = ctx;
        m_readStream = DataStreamFactory.FromArray(fileBuffer, 0, fileBuffer.Length);
        m_reader = new DataReader(m_readStream) { DefaultEncoding = Encoding.GetEncoding(932) };

        ReadHeader();
        ReadNodeNames();
        ReadBones();
        ReadObjects();
        ReadVertexBuffers();
        ReadVertices();
        ReadIndices();
        ReadMeshes();

        m_readStream.Dispose();

        ProcessAsset();
    }

    private void ReadHeader()
    {
        Header.Magic = m_reader.ReadString(4);

        if (Header.Magic != "GSGM")
            throw new Exception($"{Path.GetFileName(assetPath)} is not a valid GMD file.");

        Header.FileEndian = m_reader.ReadByte();
        Header.VertexEndian = m_reader.ReadByte();
        m_reader.Stream.Position += 2;
        m_reader.Endianness = DetermineFileEndian();
        Header.Version = m_reader.ReadInt32();
        Header.FileSize = m_reader.ReadInt32();
        Header.ModelName = m_reader.Read<PXDHash>();

        Header.DetectedVersion = DetermineVersion();

        Debug.Log("Modelname: " + Header.ModelName.Text);
        Debug.Log("Version: " + Header.DetectedVersion);

        Header.NodesChunk = m_reader.Read<SizedPointer>();
        Header.ObjectChunk = m_reader.Read<SizedPointer>();
        Header.MeshChunk = m_reader.Read<SizedPointer>();
        Header.MaterialChunk = m_reader.Read<SizedPointer>();
        Header.MaterialParamsChunk = m_reader.Read<SizedPointer>();
        Header.MatrixListChunk = m_reader.Read<SizedPointer>();
        Header.VertexBufferChunk = m_reader.Read<SizedPointer>();
        Header.VertexBytesChunk = m_reader.Read<SizedPointer>();
        Header.MaterialNameChunk = m_reader.Read<SizedPointer>();
        Header.ShaderNameChunk = m_reader.Read<SizedPointer>();
        Header.NodeNameChunk = m_reader.Read<SizedPointer>();
        Header.IndicesChunk = m_reader.Read<SizedPointer>();
    }

    private void ReadNodeNames()
    {
        m_reader.Stream.Seek(Header.NodeNameChunk.Pointer, SeekMode.Start);
        NodeNames = new string[Header.NodeNameChunk.Count];

        //Read node names
        for (int i = 0; i < Header.NodeNameChunk.Count; i++)
        {
            NodeNames[i] = m_reader.Read<PXDHash>().ToString();
        }
    }

    private void ReadBones()
    {
        m_reader.Stream.Seek(Header.NodesChunk.Pointer, SeekMode.Start);

        Nodes = new GMDNode[Header.NodesChunk.Count];

        for (int i = 0; i < Header.NodesChunk.Count; i++)
        {
            GMDNode bone = new GMDNode();

            bone.BoneID = m_reader.ReadInt32();
            bone.ChildBoneID = m_reader.ReadInt32();
            bone.ParentBoneID = m_reader.ReadInt32();
            bone.ObjectIndex = m_reader.ReadInt32();
            bone.MatrixIndex = m_reader.ReadInt32();

            bone.StackOp = (NodeStackOp)m_reader.ReadInt32();

            bone.NameIdx = m_reader.ReadInt32();

            bone.Type = (NodeType)m_reader.ReadInt32();

            bone.Position = new Vector4(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
            bone.Rotation = new Quaternion(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
            bone.Scale = new Vector4(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());

            bone.WorldPosition = new Vector4(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
            bone.AnimAxis = new Vector4(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());

            for (int k = 0; k < 4; k++)
                bone.Flags[k] = m_reader.ReadInt32();

            Nodes[i] = bone;
        }
    }

    private void ReadObjects()
    {
        m_reader.Stream.Seek(Header.ObjectChunk.Pointer, SeekMode.Start);

        Objects = new GMDObject[Header.ObjectChunk.Count];

        for (int i = 0; i < Header.ObjectChunk.Count; i++)
        {
            GMDObject obj = m_reader.Read<GMDObject>();
            m_reader.Stream.Position += 48; //bbox
            
            Objects[i] = obj;
        }
    }

    private void ReadMeshes()
    {
        m_reader.Stream.Seek(Header.MeshChunk.Pointer, SeekMode.Start);

        Meshes = new GMDMesh[Header.MeshChunk.Count];

        for (int i = 0; i < Header.MeshChunk.Count; i++)
        {
            GMDMesh mesh = new GMDMesh();

            mesh.Index = m_reader.ReadUInt32();
            mesh.AttributeIndex = m_reader.ReadUInt32();
           
            mesh.VertexBufferIndex = m_reader.ReadUInt32();
            mesh.VertexCount = m_reader.ReadUInt32();

            mesh.TriangleListIndicesData = m_reader.Read<IndicesStruct>();
            mesh.NoResetStripIndicesData = m_reader.Read<IndicesStruct>();
            mesh.ResetStripIndicesData = m_reader.Read<IndicesStruct>();

            mesh.MatrixListOffset = m_reader.ReadUInt32();
            mesh.MatrixListLength = m_reader.ReadUInt32();

            mesh.NodeIndex = m_reader.ReadUInt32();
            mesh.ObjectIndex = m_reader.ReadUInt32();
           
            mesh.VertexOffsetFromIndex = m_reader.ReadUInt32();
            mesh.MinIndex = m_reader.ReadUInt32();

            int index_ptr_min = mesh.TriangleListIndicesData.IndexOffset;
            int index_ptr_max = index_ptr_min + mesh.TriangleListIndicesData.IndexCount;

            List<ushort> range = Indices.ToList().GetRange(index_ptr_min, index_ptr_max - index_ptr_min);
            uint smallestIndex = range.Min();

            uint indexOffset;

            if (FileUsesRelativeIndices())
                indexOffset = 0;
            else
            {
                if (FileUsesMinIndex())
                {
                    indexOffset = mesh.MinIndex;

                    if (mesh.MinIndex > smallestIndex) {
                        // TODO the blender addon throws an error here
                        indexOffset = smallestIndex;
                    }
                }
                else
                    indexOffset = smallestIndex;
            }

            List<ushort> indices = new List<ushort>();

            int indexMin = 0x1_0000;
            int indexMax = -1;

            for (int k = index_ptr_min; k < index_ptr_max; k++)
            {
                ushort index = Indices[k];


                if (index != 0xFFFF)
                {
                    indexMin = Math.Min(indexMin, index);
                    indexMax = Math.Max(indexMax, index);

                    index = (ushort)(index - indexOffset);
                }

                indices.Add(index);
            }

            uint actualMinIndex = Math.Min(mesh.MinIndex, (uint)indexMin); // if indices_offset_by_min_index else indexMin. indices_offset_by_min_index is False in Kenzan

            mesh.VertexBuffer = VertexBuffers[mesh.VertexBufferIndex].VertexBuffer;
            mesh.VertexStart = actualMinIndex + mesh.VertexOffsetFromIndex;
            mesh.VertexEnd = mesh.VertexStart + mesh.VertexCount;
            mesh.TriangleListIndices = indices.ToArray();
            Meshes[i] = mesh;
        }
    }

    private void ReadVertexBuffers()
    {
        m_reader.Stream.Seek(Header.VertexBufferChunk.Pointer, SeekMode.Start);

        VertexBuffers = new GMDVertexBufferLayout[Header.VertexBufferChunk.Count];

        for (int i = 0; i < Header.VertexBufferChunk.Count; i++)
        {
            GMDVertexBufferLayout buffer = m_reader.Read<GMDVertexBufferLayout>();
            // 4 bytes padding DONT USE SkipPadding HERE
            m_reader.SkipAhead(4);

            // Read vertices, vertex data pointer is relative only in kenzan (which i dont care about rn)
            uint vertexDataStartPos = (uint)(Header.VertexBytesChunk.Pointer + buffer.VertexData.Pointer);

            GMDVertexFormat layout = GMDVertexFormat.Deserialize(buffer.VertexFormat);

            m_reader.Stream.PushToPosition(vertexDataStartPos, SeekMode.Start);
            var oldEndian = m_reader.Endianness;
            m_reader.Endianness = Header.VertexEndianness;
            buffer.VertexBuffer = layout.ExtractVertexBuffer(m_reader, (int)buffer.VertexCount, (int)buffer.BytesPerVertex);
            m_reader.Stream.PopPosition();
            m_reader.Endianness = oldEndian;

            VertexBuffers[i] = buffer;
        }
    }

    private void ReadVertices()
    {
        m_reader.Stream.Seek(Header.VertexBytesChunk.Pointer, SeekMode.Start);
        Vertices = m_reader.ReadBytes(Header.VertexBytesChunk.Count);
    }

    private void ReadIndices()
    {
        m_reader.Stream.Seek(Header.IndicesChunk.Pointer);
        Indices = new ushort[Header.IndicesChunk.Count];

        for(int i = 0; i < Indices.Length; i++)
        {
            Indices[i] = m_reader.ReadUInt16();
        }
    }

    private GMDVersion DetermineVersion()
    {
        switch (Header.Version)
        {
            default:
                return GMDVersion.Invalid;

            case 131080:
                return GMDVersion.OOE;
            case 196619:
                return GMDVersion.OE;
            case 262146:
                return GMDVersion.DE;
        }
    }

    private EndiannessMode DetermineFileEndian()
    {
        if (Header.FileEndian == 2)
            return EndiannessMode.BigEndian;
        else
            return EndiannessMode.LittleEndian;
    }

    private bool FileUsesMinIndex()
    {
        //only kenzan gmd doesnt

        return true;
    }

    private bool FileUsesRelativeIndices()
    {
        //only kenzan gmd does

        return false;
    }


    //Create the model based on data we have read.
    private void ProcessAsset()
    {
        GameObject model = new GameObject("Model");
        model.name = Header.ModelName.Text;

        m_ctx.AddObjectToAsset("Model", model);
        m_ctx.SetMainObject(model);

        Dictionary<uint, GameObject> nodeMap = new Dictionary<uint, GameObject>();

        //Create the nodes
        foreach(GMDNode node in Nodes)
        {
            GameObject bone = new GameObject("Node");
            bone.transform.parent = model.transform;

            //Bone visualization, remove these 3 lines if you want
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            cube.transform.parent = bone.transform;

            if(node.NameIdx > -1)
                bone.name = NodeNames[node.NameIdx];

            bone.transform.position = node.WorldPosition;
            bone.transform.rotation = node.Rotation;

            nodeMap[(uint)node.BoneID] = bone;
        }

        //TODO: TheTurboTurnip help me fix this preasu
        foreach(var kv in nodeMap)
        {
            GMDNode nodeInfo = Nodes[kv.Key];
            GameObject nodeObject = kv.Value;

            if (nodeInfo.ParentBoneID > -1)
                nodeMap[(uint)nodeInfo.ParentBoneID].transform.parent = nodeObject.transform;

            if (nodeInfo.ChildBoneID > -1)
                nodeMap[(uint)nodeInfo.ChildBoneID].transform.parent = nodeObject.transform;
        }

        foreach (GMDMesh mesh in Meshes) {
            //Create the mesh based on the data we read.
            Mesh meshInst = mesh.VertexBuffer.GenerateMesh(mesh.TriangleListIndices, mesh.VertexStart, mesh.VertexEnd);
            meshInst.name = mesh.Index.ToString() + "_mesh";

            //A basic mesh filter and renderer for now.
            GameObject meshObj = new GameObject();
            MeshFilter filter = meshObj.AddComponent<MeshFilter>();
            meshObj.gameObject.AddComponent<MeshRenderer>();
            meshObj.transform.parent = nodeMap[mesh.NodeIndex].transform;
            meshObj.name = mesh.Index.ToString();
            filter.mesh = meshInst;

            //Add created meshes to the imported asset
            m_ctx.AddObjectToAsset(meshInst.name, meshInst);
            m_ctx.AddObjectToAsset(meshInst.name, meshObj);
        }
    }
}
