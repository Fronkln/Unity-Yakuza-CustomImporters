using u8 = System.Byte;
using u64 = System.UInt64;

struct Bits64 {
    u8 FirstBit;
    u8 Length;

    public Bits64(u8 firstBit, u8 length) {
        FirstBit = firstBit;
        Length = length;
    }

    public u64 ExtractFrom(u64 value) {
        // Get a mask that's Length bits long
        u64 maskAfterShift = ~(u64.MaxValue << Length);
        // Shift value down so bit #FirstBit is moved to bit 0
        u64 valueShifted = value >> FirstBit;
        // Mask out the first Length bits
        return valueShifted & maskAfterShift;
    }
    public u64 WriteInto(u64 originalValue, u64 valueToWriteIn) {
        // Get a mask that's Length bits long
        u64 unshiftedMask = ~(u64.MaxValue << Length);
        // Ensure valueToWriteIn only has Length bits
        valueToWriteIn = valueToWriteIn & unshiftedMask;
        // Mask out whatever was there before
        originalValue = originalValue & ~(unshiftedMask << FirstBit);
        // Write in new stuff
        originalValue = originalValue | (valueToWriteIn << FirstBit);
        return originalValue;
    }
}

