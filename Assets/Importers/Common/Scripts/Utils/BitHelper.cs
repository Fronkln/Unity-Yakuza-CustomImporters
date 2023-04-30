using System.Collections;
using UnityEngine;

public static class BitHelper
{
    public static uint SetBits(uint value, byte set, int start, int maxLength = -1)
    {
        BitArray b = new BitArray(new byte[] { set });
        bool[] bits = new bool[b.Count];
        b.CopyTo(bits, 0);

        int bitsCount = bits.Length;

        if(maxLength > 0)
        {
                bitsCount = maxLength;
        }

        for (int i = 0; i < bitsCount; i++)
        {
            bool bit = bits[i];

            if (bit)
            {
                value = (uint)(value | (1 << i + start));
            }
            else
                value = (uint)(value & ~(1 << i + start));
        }

        return value;
    }

    public static uint SetBits(uint value, int set, int start, int maxLength = -1)
    {
        BitArray b = new BitArray(new int[] { set });
        bool[] bits = new bool[b.Count];
        b.CopyTo(bits, 0);

        int bitsCount = bits.Length;

        if (maxLength > 0)
        {
            bitsCount = maxLength;
        }

        for (int i = 0; i < bitsCount; i++)
        {
            bool bit = bits[i];

            if (bit)
            {
                value = (uint)(value | (1 << i + start));
            }
            else
                value = (uint)(value & ~(1 << i + start));
        }

        return value;
    }
}
