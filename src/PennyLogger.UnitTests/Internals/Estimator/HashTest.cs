// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace PennyLogger.Internals.Estimator.UnitTests
{
    /// <summary>
    /// Tests the <see cref="HashTest"/> class
    /// </summary>
    public class HashTest
    {
        private void Test(Func<int, Hash> createHashLambda, int count = 100000)
        {
            // Ensure hash collisions are rare. There should be none in 100,000 hashes.
            int duplicates = CountDuplicates(createHashLambda, count);
            Assert.Equal(0, duplicates);

            // The average hash should be approximately zero. Allow +/- 0.1% after 100,000 hashes.
            var avg = ComputeAverage(createHashLambda, count);
            Assert.InRange(avg.Item1, long.MinValue / 1000, long.MaxValue / 1000);
            Assert.InRange(avg.Item2, long.MinValue / 1000, long.MaxValue / 1000);
        }

        private int CountDuplicates(Func<int, Hash> createHashLambda, int count)
        {
            var values = new HashSet<long>();
            int duplicates = 0;

            for (int n = 0; n < count; n++)
            {
                var hash = createHashLambda(n);
                
                if (values.Contains(hash.Hash1))
                {
                    duplicates++;
                }
                values.Add(hash.Hash1);

                if (values.Contains(hash.Hash2))
                {
                    duplicates++;
                }
                values.Add(hash.Hash2);
            }

            return duplicates;
        }

        private (long, long) ComputeAverage(Func<int, Hash> createHashLambda, int count)
        {
            long avg1 = 0, avg2 = 0;

            for (int n = 0; n < count; n++)
            {
                var hash = createHashLambda(n);

                avg1 += hash.Hash1 / count;
                avg2 += hash.Hash2 / count;
            }

            return (avg1, avg2);
        }

        /// <summary>
        /// Test <see cref="Hash.Create(byte[])"/> with varying length
        /// </summary>
        [Fact]
        public void CreateByteArrayVaryingLength()
        {
            // This test is a bit slower, so we test only 10,000 values instead of the usual 100,000
            Test(n => Hash.Create(new byte[n]), 10000);
        }

        /// <summary>
        /// Test <see cref="Hash.Create(byte[])"/> with varying values
        /// </summary>
        [Fact]
        public void CreateByteArrayVaryingValues()
        {
            Test(n => Hash.Create(new byte[8] { (byte)n, (byte)n, (byte)n, (byte)n,
                (byte)n, (byte)(n / 256), (byte)(n / (256 * 256)), (byte)(n / (256 * 256 * 256)) }));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(string)"/>
        /// </summary>
        [Fact]
        public void CreateString()
        {
            Test(n => Hash.Create(n.ToString()));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(int)"/>
        /// </summary>
        [Fact]
        public void CreateInt32()
        {
            Test(n => Hash.Create(n));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(long)"/>
        /// </summary>
        [Fact]
        public void CreateInt64()
        {
            Test(n => Hash.Create((long)n));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(uint)"/>
        /// </summary>
        [Fact]
        public void CreateUInt32()
        {
            Test(n => Hash.Create((uint)n));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(ulong)"/>
        /// </summary>
        [Fact]
        public void CreateUInt64()
        {
            Test(n => Hash.Create((ulong)n));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(float)"/>
        /// </summary>
        [Fact]
        public void CreateSingle()
        {
            Test(n => Hash.Create((float)n));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(double)"/>
        /// </summary>
        [Fact]
        public void CreateDouble()
        {
            Test(n => Hash.Create((double)n));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(Guid)"/>
        /// </summary>
        [Fact]
        public void CreateGuid()
        {
            Test(n => Hash.Create(new Guid(n, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)));
        }

        /// <summary>
        /// Test <see cref="Hash.Create(decimal)"/>
        /// </summary>
        [Fact]
        public void CreateDecimal()
        {
            Test(n => Hash.Create((decimal)n));
        }

        /// <summary>
        /// Test <see cref="Hash.CalculateDoubleHashes(int)"/>
        /// </summary>
        [Fact]
        public void TestDoubleHashes()
        {
            var values = new HashSet<long>();
            int duplicates = 0;

            var hash = Hash.Create(1);
            foreach (long value in hash.CalculateDoubleHashes(100000))
            {
                if (values.Contains(value))
                {
                    duplicates++;
                }
                values.Add(value);
            }

            Assert.Equal(0, duplicates);
        }

        /// <summary>
        /// Test <see cref="Hash.CalculateDoubleHashes(int, ulong)"/>
        /// </summary>
        [Fact]
        public void TestDoubleHashesULong()
        {
            var values = new HashSet<ulong>();
            int duplicates = 0;

            var hash = Hash.Create(1);
            foreach (ulong value in hash.CalculateDoubleHashes(10000, uint.MaxValue))
            {
                if (values.Contains(value))
                {
                    duplicates++;
                }
                values.Add(value);
            }

            Assert.Equal(0, duplicates);
        }
    }
}
