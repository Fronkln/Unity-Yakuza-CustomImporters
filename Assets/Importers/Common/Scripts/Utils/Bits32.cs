using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bits32
{
    int FirstBit;
    int Length;

    public Bits32(int FirstBit, int Length)
    {
        this.FirstBit = FirstBit;
        this.Length = Length;
    }

    public uint ExtractFrom(uint value)
    {
        uint maskAfterShift = ~(uint.MaxValue << Length);
        uint valueShifted = value >> FirstBit;

        return valueShifted & maskAfterShift;
    }

    public uint WriteInto(uint originalValue, uint valueToWriteIn)
    {
        uint unshiftedMask = ~(uint.MaxValue << Length);
        valueToWriteIn = valueToWriteIn & unshiftedMask;
        originalValue = originalValue & (unshiftedMask << FirstBit);
        originalValue = originalValue | (valueToWriteIn << FirstBit);

        return originalValue;
    }
}
