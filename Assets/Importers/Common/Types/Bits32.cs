using u8 = System.Byte;
using u32 = System.UInt32;

struct Bits32 {
    u8 FirstBit;
    u8 Length;

    public Bits32(u8 firstBit, u8 length) {
        FirstBit = firstBit;
        Length = length;
    }

    public u32 ExtractFrom(u32 value) {
        // Get a mask that's Length bits long
        u32 maskAfterShift = ~(u32.MaxValue << Length);
        // Shift value down so bit #FirstBit is moved to bit 0
        u32 valueShifted = value >> FirstBit;
        // Mask out the first Length bits
        return valueShifted & maskAfterShift;
    }
    public u32 WriteInto(u32 originalValue, u32 valueToWriteIn) {
        // Get a mask that's Length bits long
        u32 unshiftedMask = ~(u32.MaxValue << Length);
        // Ensure valueToWriteIn only has Length bits
        valueToWriteIn = valueToWriteIn & unshiftedMask;
        // Mask out whatever was there before
        originalValue = originalValue & ~(unshiftedMask << FirstBit);
        // Write in new stuff
        originalValue = originalValue | (valueToWriteIn << FirstBit);
        return originalValue;
    }
}
