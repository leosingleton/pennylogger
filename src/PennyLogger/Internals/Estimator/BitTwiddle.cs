// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System.Diagnostics;

namespace PennyLogger.Internals.Estimator
{
    /// <summary>
    /// Helper functions for bit twiddling operations
    /// </summary>
    internal static class BitTwiddle
    {
        /// <summary>
        /// Reads a subset of bits from an 8-bit value
        /// </summary>
        /// <param name="value">8-bit value to read bits from</param>
        /// <param name="shift">Amount to right-shift the bits read</param>
        /// <param name="bitmask">Bitmask of the bits to read</param>
        /// <returns>Bits read</returns>
        public static ulong GetBits(ref byte value, int shift, ulong bitmask)
        {
            return (value & bitmask) >> shift;
        }

        /// <summary>
        /// Reads a subset of bits from a 16-bit value
        /// </summary>
        /// <param name="value">16-bit value to read bits from</param>
        /// <param name="shift">Amount to right-shift the bits read</param>
        /// <param name="bitmask">Bitmask of the bits to read</param>
        /// <returns>Bits read</returns>
        public static ulong GetBits(ref ushort value, int shift, ulong bitmask)
        {
            return (value & bitmask) >> shift;
        }

        /// <summary>
        /// Reads a subset of bits from a 64-bit value
        /// </summary>
        /// <param name="value">64-bit value to read bits from</param>
        /// <param name="shift">Amount to right-shift the bits read</param>
        /// <param name="bitmask">Bitmask of the bits to read</param>
        /// <returns>Bits read</returns>
        public static ulong GetBits(ref ulong value, int shift, ulong bitmask)
        {
            return (value & bitmask) >> shift;
        }

        /// <summary>
        /// Writes a value to a subset of bits in an 8-bit value
        /// </summary>
        /// <param name="value">8-bit value to write bits to</param>
        /// <param name="shift">Amount to left-shift the bits to write</param>
        /// <param name="bitmask">Bitmask of the bits to write</param>
        /// <param name="newBits">Value to write</param>
        public static void SetBits(ref byte value, int shift, ulong bitmask, ulong newBits)
        {
            Debug.Assert(newBits >= 0);
            Debug.Assert(newBits <= (bitmask >> shift));
            value = (byte)((value & ~bitmask) | (newBits << shift));
        }

        /// <summary>
        /// Writes a value to a subset of bits in a 16-bit value
        /// </summary>
        /// <param name="value">16-bit value to write bits to</param>
        /// <param name="shift">Amount to left-shift the bits to write</param>
        /// <param name="bitmask">Bitmask of the bits to write</param>
        /// <param name="newBits">Value to write</param>
        public static void SetBits(ref ushort value, int shift, ulong bitmask, ulong newBits)
        {
            Debug.Assert(newBits >= 0);
            Debug.Assert(newBits <= (bitmask >> shift));
            value = (ushort)((value & ~bitmask) | (newBits << shift));
        }

        /// <summary>
        /// Writes a value to a subset of bits in a 64-bit value
        /// </summary>
        /// <param name="value">64-bit value to write bits to</param>
        /// <param name="shift">Amount to left-shift the bits to write</param>
        /// <param name="bitmask">Bitmask of the bits to write</param>
        /// <param name="newBits">Value to write</param>
        public static void SetBits(ref ulong value, int shift, ulong bitmask, ulong newBits)
        {
            Debug.Assert(newBits >= 0);
            Debug.Assert(newBits <= (bitmask >> shift));
            value = (ulong)((value & ~bitmask) | (newBits << shift));
        }

        /// <summary>
        /// Finds the first non-zero bit in a 64-bit number. Behaves similar to the BitScanForward compiler intrinsic
        /// in C++, which unfortunately .NET has no equivalent for.
        /// </summary>
        /// <param name="value">Value to find the first non-zero bit</param>
        /// <returns>
        /// An integer from 0 (least significant bit) to 63 (most significant bit). If <paramref name="value"/> == 0,
        /// this method returns 0.
        /// </returns>
        public static int FindFirstSet(ulong value)
        {
            int result = 0;

            if ((value & 0xffff_ffff_0000_0000) != 0)
                result += 32;
            else
                value <<= 32;

            if ((value & 0xffff_0000_0000_0000) != 0)
                result += 16;
            else
                value <<= 16;

            if ((value & 0xff00_0000_0000_0000) != 0)
                result += 8;
            else
                value <<= 8;

            if ((value & 0xf000_0000_0000_0000) != 0)
                result += 4;
            else
                value <<= 4;

            if ((value & 0xc000_0000_0000_0000) != 0)
                result += 2;
            else
                value <<= 2;

            if ((value & 0x8000_0000_0000_0000) != 0)
                result += 1;

            return result;
        }

        /// <summary>
        /// Returns the number of non-zero bits in an 8-bit value
        /// </summary>
        /// <param name="value">Value to count the bits of</param>
        /// <returns>
        /// An integer from 0 to 8 indicating the number of non-zero bits in <paramref name="value"/>
        /// </returns>
        public static int CountBitsSet(byte value)
        {
            if (CountBitsSetLookupTable == null)
            {
                var table = new int[256];
                for (int n = 0; n < 256; n++)
                {
                    for (int b = 0; b < 8; b++)
                    {
                        if ((n & (1 << b)) != 0)
                        {
                            table[n]++;
                        }
                    }
                }
                CountBitsSetLookupTable = table;
            }

            return CountBitsSetLookupTable[value];
        }

        private static int[] CountBitsSetLookupTable;

        /// <summary>
        /// Returns the number of non-zero bits in a 64-bit value
        /// </summary>
        /// <param name="value">Value to count the bits of</param>
        /// <returns>
        /// An integer from 0 to 64 indicating the number of non-zero bits in <paramref name="value"/>
        /// </returns>
        public static int CountBitsSet(ulong value) =>
            CountBitsSet((byte)value) +
            CountBitsSet((byte)(value >> 8)) +
            CountBitsSet((byte)(value >> 16)) +
            CountBitsSet((byte)(value >> 24)) +
            CountBitsSet((byte)(value >> 32)) +
            CountBitsSet((byte)(value >> 40)) +
            CountBitsSet((byte)(value >> 48)) +
            CountBitsSet((byte)(value >> 56));

        /// <inheritdoc cref="CountBitsSet(ulong)"/>
        public static int CountBitsSet(long value) => unchecked(CountBitsSet((ulong)value));

        /// <summary>
        /// Rounds up to the next highest power of 2
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        /// <remarks>
        /// From https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
        /// </remarks>
        public static ulong RoundUpToPowerOf2(ulong value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return value + 1;
        }

        /// <inheritdoc cref="RoundUpToPowerOf2(ulong)"/>
        public static long RoundUpToPowerOf2(long value) => unchecked((long)RoundUpToPowerOf2((ulong)value));
    }
}
