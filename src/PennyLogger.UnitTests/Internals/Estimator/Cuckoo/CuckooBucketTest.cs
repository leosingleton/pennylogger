// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Xunit;

namespace PennyLogger.Internals.Estimator.Cuckoo.UnitTests
{
    /// <summary>
    /// Tests the various classes derived from <see cref="ICuckooBucket"/>
    /// </summary>
    public class CuckooBucketTest
    {
        private static void Test(ICuckooBucket bucket, int expectedSize, bool counting)
        {
            // The default value should be empty
            Assert.True(bucket.IsEmpty);

            // The size is the expected value
            Assert.Equal(expectedSize, bucket.SizeOf);

            // Read and write max values
            bucket.Fingerprint = bucket.MaxFingerprint;
            bucket.Count = bucket.MaxCount;
            Assert.Equal(bucket.MaxFingerprint, bucket.Fingerprint);
            Assert.Equal(bucket.MaxCount, bucket.Count);

            // Read and write zeroes
            bucket.Fingerprint = 0UL;
            Assert.Equal(0UL, bucket.Fingerprint);
            Assert.Equal(bucket.MaxCount, bucket.Count);

            if (counting)
            {
                bucket.Count = 0UL;
                Assert.Equal(0UL, bucket.Fingerprint);
                Assert.Equal(0UL, bucket.Count);
            }
        }

        /// <summary>
        /// Test <see cref="CuckooBucket8"/>
        /// </summary>
        [Fact]
        public void CuckooBucket8()
        {
            Test(new CuckooBucket8(), 1, false);
        }

        /// <summary>
        /// Test <see cref="CuckooBucket16Counting"/>
        /// </summary>
        [Fact]
        public void CuckooBucket16Counting()
        {
            Test(new CuckooBucket16Counting(), 2, true);
        }

        /// <summary>
        /// Test <see cref="CuckooBucket64Counting"/>
        /// </summary>
        [Fact]
        public void CuckooBucket64Counting()
        {
            Test(new CuckooBucket64Counting(), 8, true);
        }
    }
}
