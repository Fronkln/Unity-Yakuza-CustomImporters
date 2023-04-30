using UnityEngine.Rendering;

using u8 = System.Byte;
using u32 = System.UInt32;
using u64 = System.UInt64;

public class GMDVertexFormat {
    public VecStorage Pos;
    public VecStorage? Weight;
    public VecStorage? Bone;
    public VecStorage? Normal;
    public VecStorage? Tangent;
    public VecStorage? Unk;
    public VecStorage? Col0;
    public VecStorage? Col1;
    public VecStorage[] UVs;

    public GMDVertexFormat(
        VecStorage pos,
        VecStorage? weight,
        VecStorage? bone,
        VecStorage? normal,
        VecStorage? tangent,
        VecStorage? unk,
        VecStorage? col0,
        VecStorage? col1,
        VecStorage[] uvs
    ) {
        Pos = pos;
        Weight = weight;
        Bone = bone;
        Normal = normal;
        Tangent = tangent;
        Unk = unk;
        Col0 = col0;
        Col1 = col1;
        UVs = uvs;
    }

    // The bottom 32 bits are for non-UV fields, and don't have a regular structure
    private static readonly Bits64 FMT_POS_COUNT = new Bits64(0, 3);
    private static readonly Bits64 FMT_POS_PRECISION = new Bits64(3, 1);
    private static readonly Bits64 FMT_WEIGHT_EN = new Bits64(4, 3);
    private static readonly Bits64 FMT_WEIGHT_STORAGE = new Bits64(7, 2);
    private static readonly Bits64 FMT_BONES_EN = new Bits64(9, 1);
    private static readonly Bits64 FMT_NORMAL_EN = new Bits64(10, 1);
    private static readonly Bits64 FMT_NORMAL_STORAGE = new Bits64(11, 2);
    private static readonly Bits64 FMT_TANGENT_EN = new Bits64(13, 1);
    private static readonly Bits64 FMT_TANGENT_STORAGE = new Bits64(14, 2);
    private static readonly Bits64 FMT_UNK_EN = new Bits64(16, 1);
    private static readonly Bits64 FMT_UNK_STORAGE = new Bits64(17, 2);
    // We don't know what bits 19 and 20 do.
    private static readonly Bits64 FMT_COL0_EN = new Bits64(21, 1);
    private static readonly Bits64 FMT_COL0_STORAGE = new Bits64(22, 2);
    private static readonly Bits64 FMT_COL1_EN = new Bits64(24, 1);
    private static readonly Bits64 FMT_COL1_STORAGE = new Bits64(25, 2);
    private static readonly Bits64 FMT_UV_EN = new Bits64(27, 1);
    private static readonly Bits64 FMT_UV_COUNT = new Bits64(28, 4);
    // Bits 32-63 follow a regular structure: 4 bits for each UV channel
    private static readonly Bits64[] FMT_UV_BITS = new Bits64[8] {
        new Bits64(32 + (0*4), 4),
        new Bits64(32 + (1*4), 4),
        new Bits64(32 + (2*4), 4),
        new Bits64(32 + (3*4), 4),
        new Bits64(32 + (4*4), 4),
        new Bits64(32 + (5*4), 4),
        new Bits64(32 + (6*4), 4),
        new Bits64(32 + (7*4), 4),
    };

    public GMDVertexFormat Deserialize(u64 layoutBits) {
        bool hasWeights = FMT_WEIGHT_EN.ExtractFrom(layoutBits) != 0;
        bool hasBones = FMT_BONES_EN.ExtractFrom(layoutBits) != 0;
        bool hasNormal = FMT_NORMAL_EN.ExtractFrom(layoutBits) != 0;
        bool hasTangent = FMT_TANGENT_EN.ExtractFrom(layoutBits) != 0;
        bool hasUnk = FMT_UNK_EN.ExtractFrom(layoutBits) != 0;
        bool hasCol0 = FMT_COL0_EN.ExtractFrom(layoutBits) != 0;
        bool hasCol1 = FMT_COL1_EN.ExtractFrom(layoutBits) != 0;
        bool hasUVs = FMT_UV_EN.ExtractFrom(layoutBits) != 0;
        int numUVs = (int)FMT_UV_COUNT.ExtractFrom(layoutBits);

        // Parse position
        int posCount = (int)FMT_POS_COUNT.ExtractFrom(layoutBits);
        bool posIs16Bit = FMT_POS_PRECISION.ExtractFrom(layoutBits) == 1;
        var pos = new VecStorage(
            posIs16Bit ? VertexAttributeFormat.Float16 : VertexAttributeFormat.Float32,
            (posCount == 3) ? 3 : 4
        );

        VecStorage? weightsStorage =
            hasWeights ? ExtractAttributeFormat(
                layoutBits, FMT_WEIGHT_STORAGE,
                numComponentsIfFloat32: 4, formatIfByte: VertexAttributeFormat.UNorm8
            ) : null;

        VecStorage? boneStorage = hasBones ? new VecStorage(VertexAttributeFormat.UNorm8, 4) : null;

        VecStorage? normalStorage =
            hasNormal ? ExtractAttributeFormat(
                layoutBits, FMT_NORMAL_STORAGE,
                numComponentsIfFloat32: 4, formatIfByte: VertexAttributeFormat.SNorm8
            ) : null;
        VecStorage? tangentStorage = hasTangent ?
            ExtractAttributeFormat(
                layoutBits, FMT_TANGENT_STORAGE,
                numComponentsIfFloat32: 4, formatIfByte: VertexAttributeFormat.SNorm8
            ) : null;
        VecStorage? unkStorage = hasUnk ?
            ExtractAttributeFormat(
                layoutBits, FMT_UNK_STORAGE,
                numComponentsIfFloat32: 3, formatIfByte: VertexAttributeFormat.UNorm8
            ) : null;
        VecStorage? col0Storage = hasCol0 ?
            ExtractAttributeFormat(
                layoutBits, FMT_COL0_STORAGE,
                numComponentsIfFloat32: 4, formatIfByte: VertexAttributeFormat.UNorm8
            ) : null;
        VecStorage? col1Storage = hasCol1 ?
            ExtractAttributeFormat(
                layoutBits, FMT_COL1_STORAGE,
                numComponentsIfFloat32: 4, formatIfByte: VertexAttributeFormat.UNorm8
            ) : null;

        VecStorage[] uvStorages = new VecStorage[numUVs];
        if (hasUVs) {
            int currUv = 0;
            foreach (Bits64 bits in FMT_UV_BITS) {
                u64 uvBits = bits.ExtractFrom(layoutBits);
                if (uvBits == 0xF)
                    continue;

                u64 uvFormatBits = (uvBits >> 2) & 0b11;
                u64 uvNumCompBits = (uvBits >> 0) & 0b11;
                if (uvFormatBits == 2 || uvFormatBits == 3) {
                    uvStorages[currUv] = new VecStorage(VertexAttributeFormat.UNorm8, 4);
                } else {
                    // uvFormatBits == 0 or == 1
                    var uvFormat = (uvFormatBits == 1) ? VertexAttributeFormat.Float16 : VertexAttributeFormat.Float32;

                    switch (uvNumCompBits) {
                        case 0:
                            uvStorages[currUv] = new VecStorage(uvFormat, 2);
                            break;
                        case 1:
                            uvStorages[currUv] = new VecStorage(uvFormat, 3);
                            break;
                        case 2:
                            uvStorages[currUv] = new VecStorage(uvFormat, 4);
                            break;
                        case 3:
                            // This will throw an exception - we haven't had this case come up and we're not sure what to do with it
                            uvStorages[currUv] = new VecStorage(uvFormat, 1);
                            break;
                        default:
                            throw new System.InvalidOperationException("Impossible value of 2-bit value");
                    }
                }
                currUv++;
            }
        }

        return new GMDVertexFormat(
            pos,
            weight: weightsStorage,
            bone: boneStorage,
            normal: normalStorage,
            tangent: tangentStorage,
            unk: unkStorage,
            col0: col0Storage,
            col1: col1Storage,
            uvs: uvStorages
        );
    }
    private VecStorage ExtractAttributeFormat(u64 layoutBits, Bits64 bits, int numComponentsIfFloat32, VertexAttributeFormat formatIfByte) {
        switch (bits.ExtractFrom(layoutBits)) {
            case 0:
                return new VecStorage(VertexAttributeFormat.Float32, numComponentsIfFloat32);
            case 1:
                return new VecStorage(VertexAttributeFormat.Float16, 4);
            default:
                return new VecStorage(formatIfByte, 4);
        }
    }
}
