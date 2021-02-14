// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Text;

namespace PennyLogger.Internals.Estimator
{
    /// <summary>
    /// The built-in <see cref="object.GetHashCode"/> isn't suitable for the frequency estimators because:
    /// <list type="number">
    /// <item>It only provides 32 bits of output. Some algorithms need up to 128 bits for optimal efficiency.</item>
    /// <item><see cref="int.GetHashCode"/> simply returns the input integer</item>
    /// <item>
    /// <see cref="string.GetHashCode()"/> returns a different result for the same string on each process execution.
    /// This makes unit tests non-deterministic.
    /// </item>
    /// </list>
    /// This struct provides a replacement and holds a 128-bit hash code (exposed as two 64-bit longs). It also provides
    /// static creation methods to construct hashes from common .NET data types.
    /// </summary>
    public struct Hash
    {
        /// <summary>
        /// First 64-bit hash value
        /// </summary>
        public long Hash1 { get; set; }

        /// <summary>
        /// Second 64-bit hash value
        /// </summary>
        public long Hash2 { get; set; }

        /// <summary>
        /// Creates a 128-bit hash from a UInt64
        /// </summary>
        /// <param name="value">UInt64 value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(ulong value)
        {
            unchecked
            {
                // Two large prime numbers from asecuritysite.com
                const ulong prime1 = 12_972_119_692_533_089_989UL;
                const ulong prime2 = 11_687_066_356_047_830_333UL;

                // Two large random numbers from random.org
                const ulong random1 = 0x40af_387b_041b_b86a;
                const ulong random2 = 0x6ea1_8141_765d_96f6;

                // Use Knuth's multiplicative hash method of multiplying by a large prime.
                // We first add a big random offset so that Hash(0) != 0.
                ulong hash1 = (value + random1) * prime1;
                ulong hash2 = (value + random2) * prime2;

                return new Hash { Hash1 = (long)hash1, Hash2 = (long)hash2 };
            }
        }

        /// <summary>
        /// Creates a 128-bit hash from an array of bytes
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(byte[] bytes)
        {
            Hash result = new Hash();

            // Break the input bytes into chunks of 8 bytes and run them through the ulong hash implementation.
            // However, we first hash the array length to prevent arrays with the same repeating value but varying
            // length from generating the same hashes.
            ulong value = (ulong)bytes.LongLength;
            for (int n = 0; n < bytes.Length; n++)
            {
                if (n % 8 == 0)
                {
                    Hash hash = Create(value);
                    result.Hash1 ^= hash.Hash1;
                    result.Hash2 ^= hash.Hash2;
                }

                value = (value << 8) | bytes[n];
            }

            // Run any remaining bytes through the ulong hash implementation
            {
                Hash hash = Create(value);
                result.Hash1 ^= hash.Hash1;
                result.Hash2 ^= hash.Hash2;
            }

            return result;
        }

        /// <summary>
        /// Creates a 128-bit hash from a string
        /// </summary>
        /// <param name="s">String</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(string s) => Create(Encoding.UTF8.GetBytes(s));

        /// <summary>
        /// Creates a 128-bit hash from a Int32
        /// </summary>
        /// <param name="value">Int32 value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(int value) => Create(unchecked((ulong)value));

        /// <summary>
        /// Creates a 128-bit hash from a Int64
        /// </summary>
        /// <param name="value">Int64 value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(long value) => Create(unchecked((ulong)value));

        /// <summary>
        /// Creates a 128-bit hash from a UInt32
        /// </summary>
        /// <param name="value">UInt32 value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(uint value) => Create(unchecked((ulong)value));

        /// <summary>
        /// Creates a 128-bit hash from a Single
        /// </summary>
        /// <param name="value">Single value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(float value) => Create(unchecked((ulong)value));

        /// <summary>
        /// Creates a 128-bit hash from a Double
        /// </summary>
        /// <param name="value">Double value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(double value) => Create(unchecked((ulong)value));

        /// <summary>
        /// Creates a 128-bit hash from a GUID
        /// </summary>
        /// <param name="value">GUID value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(Guid value) => Create(value.ToByteArray());

        /// <summary>
        /// Creates a 128-bit hash from a Decimal
        /// </summary>
        /// <param name="value">Decimal value</param>
        /// <returns>128-bit hash</returns>
        public static Hash Create(decimal value)
        {
            // Convert the decimal to four 32-bit integers
            var ints = decimal.GetBits(value);
            
            // Convert the 32-bit integers to bytes
            var intBytes = new byte[4][]
            {
                BitConverter.GetBytes(ints[0]),
                BitConverter.GetBytes(ints[1]),
                BitConverter.GetBytes(ints[2]),
                BitConverter.GetBytes(ints[3])
            };

            // Flatten the values into a single byte array
            var bytes = new byte[16]
            {
                intBytes[0][0],
                intBytes[0][1],
                intBytes[0][2],
                intBytes[0][3],
                intBytes[1][0],
                intBytes[1][1],
                intBytes[1][2],
                intBytes[1][3],
                intBytes[2][0],
                intBytes[2][1],
                intBytes[2][2],
                intBytes[2][3],
                intBytes[3][0],
                intBytes[3][1],
                intBytes[3][2],
                intBytes[3][3]
            };

            return Create(bytes);
        }

        /// <summary>
        /// Calculates an arbitrary number of 64-bit hashes using double hashing
        /// </summary>
        /// <param name="count">Number of hash values to return</param>
        /// <returns>Array of hash values</returns>
        public long[] CalculateDoubleHashes(int count)
        {
            unchecked
            {
                // The algorithm here comes from a paper by Dillinger and Manolios and can also be found on Wikipedia
                // to minimize hash collisions.
                // For details, see: https://en.wikipedia.org/wiki/Double_hashing#Enhanced_double_hashing
                var hashes = new long[count];
                long a = Hash1;
                long b = Hash2;

                for (int i = 0; i < count; i++)
                {
                    hashes[i] = a;
                    a += b;
                    b += i;
                }

                return hashes;
            }
        }

        /// <summary>
        /// Calculates an arbitrary number of 64-bit hashes using double hashing
        /// </summary>
        /// <param name="count">Number of hash values to return</param>
        /// <param name="max">Maximum value to return</param>
        /// <returns>Array of hash values, in the range [0, <paramref name="max"/> - 1]</returns>
        public ulong[] CalculateDoubleHashes(int count, ulong max)
        {
            unchecked
            {
                // The algorithm here comes from a paper by Dillinger and Manolios and can also be found on Wikipedia
                // to minimize hash collisions.
                // For details, see: https://en.wikipedia.org/wiki/Double_hashing#Enhanced_double_hashing
                var hashes = new ulong[count];
                ulong a = (ulong)Hash1;
                ulong b = (ulong)Hash2;

                for (uint i = 0; i < count; i++)
                {
                    hashes[i] = a % max;
                    a += b;
                    b += i;
                }

                return hashes;
            }
        }
    }
}
