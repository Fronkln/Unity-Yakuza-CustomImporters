using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class GMDVertexBuffer {
    GMDVertexFormat Format;

    /// <summary>
    /// Stored in Unity Mesh vertex position attribute
    /// </summary>
    public float[,] Pos;
    /// <summary>
    /// Stored in a UV channel (assuming unskinned)
    /// </summary>
    public float[,]? Weight;
    /// <summary>
    /// Stored in a UV channel (assuming unskinned)
    /// </summary>
    public float[,]? Bone;
    /// <summary>
    /// Stored in Unity Mesh vertex normal attribute (may allocate a UV channel for normal.W)
    /// </summary>
    public float[,]? Normal;
    /// <summary>
    /// Stored in Unity Mesh vertex tangent attribute (may allocate a UV channel for tangent.W)
    /// </summary>
    public float[,]? Tangent;
    /// <summary>
    /// Stored in Unity Mesh vertex color attribute
    /// </summary>
    public float[,]? Col0;
    /// <summary>
    /// Stored in a UV channel (Unity doesn't allow multiple vertex colors)
    /// </summary>
    public float[,]? Col1;
    /// <summary>
    /// Stored in UV channels. Takes top priority
    /// </summary>
    public List<float[,]> UVs;

    public GMDVertexBuffer(GMDVertexFormat format, float[,] pos, float[,]? weight, float[,]? bone, float[,]? normal, float[,]? tangent, float[,]? col0, float[,]? col1, List<float[,]> uvs) {
        Format = format;
        Pos = pos;
        Weight = weight;
        Bone = bone;
        Normal = normal;
        Tangent = tangent;
        Col0 = col0;
        Col1 = col1;
        UVs = uvs;
    }

    public Mesh GenerateMesh(ushort[] triangleListIndices) {
        // TODO only write the vertices we use into the buffer
        Mesh mesh = new Mesh();

        mesh.SetVertices(bufToVec3(Pos));
        if (Normal is not null)
            mesh.SetNormals(bufToVec3(Normal));
        if (Tangent is not null)
            mesh.SetTangents(bufToVec4(Tangent));
        if (Col0 is not null)
            mesh.SetColors(bufToColor(Col0));

        int uvChannel = 0;
        // First, push all the uvs in - starting with the primary index if there is one
        if (Format.PrimaryUVIndex is int nonNullPrimaryUV) {
            CoerceBufIntoUV(mesh, uvChannel, UVs[nonNullPrimaryUV]);
            uvChannel++;
        }
        // Then push in the rest of the UVs that aren't the primary index
        for (int i = 0; i < UVs.Count; i++) {
            if (i == Format.PrimaryUVIndex)
                continue;
            CoerceBufIntoUV(mesh, uvChannel, UVs[i]);
            uvChannel++;
        }
        // Push Col1
        if (Col1 is not null) {
            CoerceBufIntoUV(mesh, uvChannel, Col1);
            uvChannel++;
        }
        // Push normal.w if present
        if (TryCoerceBufWIntoUV(mesh, uvChannel, Normal)) {
            uvChannel++;
        }
        // Push tangent.w if present
        if (TryCoerceBufWIntoUV(mesh, uvChannel, Tangent)) {
            uvChannel++;
        }
        // Push weights
        if (Weight is not null) {
            CoerceBufIntoUV(mesh, uvChannel, Weight);
            uvChannel++;
        }
        // Push bones
        if (Bone is not null) {
            CoerceBufIntoUV(mesh, uvChannel, Bone);
            uvChannel++;
        }

        mesh.SetTriangles(triangleListIndices, 0);

        mesh.UploadMeshData(markNoLongerReadable: false);

        return mesh;
    }

    private static void CoerceBufIntoUV(Mesh mesh, int uvChannel, float[,] buf) {
        switch (buf.GetLength(1)) {
            case 2:
                mesh.SetUVs(uvChannel, bufToVec2(buf));
                break;
            case 3:
                mesh.SetUVs(uvChannel, bufToVec3(buf));
                break;
            case 4:
                mesh.SetUVs(uvChannel, bufToVec4(buf));
                break;
            default:
                throw new System.ArgumentException("buf has invalid second length " + buf.GetLength(1));
        }
    }
    private static bool TryCoerceBufWIntoUV(Mesh mesh, int uvChannel, float[,]? buf) {
        if (buf is null) {
            return false;
        }
        if (buf.GetLength(1) < 4) {
            return false;
        }
        mesh.SetUVs(uvChannel, bufWToVec2(buf));
        return true;
    }
    private static Vector2[] bufToVec2(float[,] buf) {
        if (buf.GetLength(1) < 2) {
            throw new System.ArgumentOutOfRangeException("Tried to convert buffer to Vec2 when second length was " + buf.GetLength(1));
        }

        var data = new Vector2[buf.GetLength(0)];
        for (int i = 0; i < buf.GetLength(0); i++) {
            data[i] = new Vector2(buf[i, 0], buf[i, 1]);
        }
        return data;
    }
    private static Vector3[] bufToVec3(float[,] buf) {
        if (buf.GetLength(1) < 3) {
            throw new System.ArgumentOutOfRangeException("Tried to convert buffer to Vec3 when second length was " + buf.GetLength(1));
        }

        var data = new Vector3[buf.GetLength(0)];
        for (int i = 0; i < buf.GetLength(0); i++) {
            data[i] = new Vector3(buf[i, 0], buf[i, 1], buf[i, 2]);
        }
        return data;
    }
    private static Vector4[] bufToVec4(float[,] buf) {
        if (buf.GetLength(1) < 4) {
            throw new System.ArgumentOutOfRangeException("Tried to convert buffer to Vec4 when second length was " + buf.GetLength(1));
        }

        var data = new Vector4[buf.GetLength(0)];
        for (int i = 0; i < buf.GetLength(0); i++) {
            data[i] = new Vector4(buf[i, 0], buf[i, 1], buf[i, 2], buf[i, 3]);
        }
        return data;
    }

    private static Vector2[] bufWToVec2(float[,] buf) {
        if (buf.GetLength(1) < 4) {
            throw new System.ArgumentOutOfRangeException("Tried to convert buffer W to Vec2 when second length was " + buf.GetLength(1));
        }

        var data = new Vector2[buf.GetLength(0)];
        for (int i = 0; i < buf.GetLength(0); i++) {
            data[i] = new Vector2(buf[i, 3], 0);
        }
        return data;
    }
    private static Color[] bufToColor(float[,] buf) {
        if (buf.GetLength(1) < 4) {
            throw new System.ArgumentOutOfRangeException("Tried to convert buffer to Vec4 when second length was " + buf.GetLength(1));
        }

        var data = new Color[buf.GetLength(0)];
        for (int i = 0; i < buf.GetLength(0); i++) {
            data[i] = new Color(buf[i, 0], buf[i, 1], buf[i, 2], buf[i, 3]);
        }
        return data;
    }
}
