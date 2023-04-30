using UnityEngine.Rendering;

using u8 = System.Byte;
using u32 = System.UInt32;
using u64 = System.UInt64;
using Yarhl.IO;

#nullable enable

public struct VecStorage {
    public VertexAttributeFormat Component;
    public int NumComponents;

    public VecStorage(VertexAttributeFormat component, int numComponents) {
        if (numComponents < 2 || numComponents > 4) {
            throw new System.ArgumentOutOfRangeException("numComponents " + numComponents + " not between 2 and 4");
        }

        Component = component;
        NumComponents = numComponents;
    }

    public int ComponentWidthBytes {
        get {
            switch (Component) {
                case VertexAttributeFormat.Float32:
                case VertexAttributeFormat.UInt32:
                case VertexAttributeFormat.SInt32:
                    return 4;
                case VertexAttributeFormat.Float16:
                case VertexAttributeFormat.UInt16:
                case VertexAttributeFormat.SInt16:
                case VertexAttributeFormat.UNorm16:
                case VertexAttributeFormat.SNorm16:
                    return 2;
                case VertexAttributeFormat.UInt8:
                case VertexAttributeFormat.SInt8:
                case VertexAttributeFormat.UNorm8:
                case VertexAttributeFormat.SNorm8:
                    return 1;
                default:
                    throw new System.ArgumentException("VecStorage with unknown VertexAttributeFormat " + Component);
            }
        }
    }

    public int WidthBytes {
        get {
            return ComponentWidthBytes * NumComponents;
        }
    }

    /// <summary>
    /// Extract byte-sized data from the given vertex buffer bytes and return it as a multidimensional array
    /// </summary>
    /// <returns></returns>
    public u8[,]? ExtractDataAsBytes(DataReader reader, int numVertices, int vertexSize) {
        if (ComponentWidthBytes != 1) {
            return null;
        }

        int bytesBetweenVertexEndAndNextVertexStart = vertexSize - WidthBytes;

        u8[,] data = new u8[numVertices, NumComponents];
        for (int i = 0; i < numVertices; i++) {
            for (int j = 0; j < NumComponents; j++) {
                data[i, j] = reader.ReadByte();
            }
            reader.SkipAhead(bytesBetweenVertexEndAndNextVertexStart);
        }
        return data;
    }
    /// <summary>
    /// Extract data from the given vertex buffer bytes, convert it to float32, and return it as a multidimensional array.
    /// </summary>
    /// <returns></returns>
    public float[,] ExtractDataAsFloats(DataReader reader, int numVertices, int vertexSize) {
        int bytesBetweenVertexEndAndNextVertexStart = vertexSize - WidthBytes;

        float[,] data = new float[numVertices, NumComponents];

        // Make a copy of Component inside the loop to help the compiler realize it can lift the switch-case out of it
        var component = Component;
        for (int i = 0; i < numVertices; i++) {
            for (int j = 0; j < NumComponents; j++) {
                switch (component) {
                    case VertexAttributeFormat.Float32:
                        data[i, j] = reader.ReadSingle();
                        break;
                    case VertexAttributeFormat.Float16:
                        data[i, j] = System.HalfHelper.HalfBitsToSingle(reader.ReadUInt16());
                        break;
                    case VertexAttributeFormat.UNorm8:
                        // (0, 255) -> (0, 1) by dividing by 255
                        data[i, j] = reader.ReadByte() / 255.0f;
                        break;
                    case VertexAttributeFormat.SNorm8:
                        // (0, 255) -> (0, 1) by dividing by 255
                        // (0, 1) -> (-1, 1) by multiplying by 2, subtracting 1
                        data[i, j] = ((reader.ReadByte() / 255.0f) * 2.0f) - 1.0f;
                        break;
                    case VertexAttributeFormat.UInt8:
                        data[i, j] = (float)reader.ReadByte();
                        break;
                    default:
                        throw new System.ArgumentException("Don't know how to unpack " + component);
                }
            }
            reader.SkipAhead(bytesBetweenVertexEndAndNextVertexStart);
        }

        return data;
    }
}