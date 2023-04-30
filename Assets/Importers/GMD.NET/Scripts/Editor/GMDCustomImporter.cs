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
        Header.VertexBufferPoolChunk = m_reader.Read<SizedPointer>();
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

            //Vertex
            uint vertexStart = mesh.MinIndex + mesh.VertexOffsetFromIndex;
            uint vertexEnd = vertexStart + mesh.VertexCount;

            //mesh.VertexBuffer = VertexBuffers[mesh.VertexBufferIndex];
            //mesh.VerticesData = new GMDVertex[mesh.VertexCount];
            //Array.Copy(mesh.VertexBuffer.Vertices, (int)vertexStart, mesh.VerticesData, 0, (int)mesh.VertexCount);

            int index_ptr_min = mesh.TriangleListIndicesData.IndexOffset;
            int index_ptr_max = index_ptr_min + mesh.TriangleListIndicesData.IndexCount;

            int index_offset = 0;

            List<ushort> range = Indices.ToList().GetRange(index_ptr_min, index_ptr_max - index_ptr_min);
            int smallestIndex = range.IndexOf(range.Min());

            if (FileUsesRelativeIndices())
                index_offset = 0;
            else
            {
                if (FileUsesMinIndex())
                {
                    index_offset = (int)mesh.MinIndex;

                    if (mesh.MinIndex > smallestIndex)
                        index_offset = smallestIndex;
                }
                else
                    index_offset = smallestIndex;
            }

            List<ushort> indices = new List<ushort>();


            for (int k = index_ptr_min; k < index_ptr_max; k++)
            {
                ushort index = Indices[k];
                int indexMin = index;
                int indexMax = index;

                if (index != 0xFFFF)
                {
                    indexMin = Mathf.Min(indexMin, index);
                    indexMax = Mathf.Max(indexMax, index);

                    index = (ushort)(index - mesh.MinIndex);
                }

                if(mesh.Index == 0)
                    Debug.Log(index);

                indices.Add(index);
            }


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
            var buffer = m_reader.Read<GMDVertexBufferLayout>();
            // 4 bytes padding
            m_reader.Stream.Position += 4;

            ////Read vertices, vertex data pointer is relative only in kenzan (which i dont care about rn)
            //uint vertexDataStartPos = (uint)(Header.VertexBufferPoolChunk.Pointer + buffer.VertexData.Pointer);

            //m_reader.Stream.RunInPosition(delegate
            //{
            //    buffer.Vertices = ReadBufferVertices(buffer.Format, buffer.Flags, buffer.VertexData.Count, buffer.BytesPerVertex);
            //}, vertexDataStartPos, SeekMode.Start);
            

            VertexBuffers[i] = buffer;
        }
    }

    private void ReadVertices()
    {
        m_reader.Stream.Seek(Header.VertexBufferPoolChunk.Pointer, SeekMode.Start);
        Vertices = m_reader.ReadBytes(Header.VertexBufferPoolChunk.Count);
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

        //foreach(GMDMesh mesh in Meshes)
        //{
        //    //Create the mesh based on the data we read.
        //    Mesh meshInst = new Mesh();
        //    meshInst.name = mesh.Index.ToString() + "_mesh";
        //    meshInst.SetVertices(mesh.VerticesData.Select(x => new Vector3(x.Position.x, x.Position.y, x.Position.z)).ToArray());
        //    meshInst.SetNormals(mesh.VerticesData.Select(x => (Vector3)x.Normal).ToArray());
        //    meshInst.SetUVs(0, (mesh.VerticesData.Select(x => x.UV)).ToArray());

        //    //Convert the ushort indices to int since that's Unity's format.
        //    int[] intIndices = mesh.TriangleListIndices.Select(x => (int)x).ToArray();
        //    //Meshes are triangles
        //    meshInst.SetIndices(intIndices, MeshTopology.Triangles, 0);
        //    //Finalize the mesh
        //    meshInst.RecalculateNormals();

        //    //A basic mesh filter and renderer for now.
        //    GameObject meshObj = new GameObject();
        //    MeshFilter filter = meshObj.AddComponent<MeshFilter>();
        //    meshObj.gameObject.AddComponent<MeshRenderer>();
        //    meshObj.transform.parent = nodeMap[mesh.NodeIndex].transform;
        //    meshObj.name = mesh.Index.ToString();
        //    filter.mesh = meshInst;

        //    //Add created meshes to the imported asset
        //    m_ctx.AddObjectToAsset(meshInst.name, meshInst);
        //}
    }
}
