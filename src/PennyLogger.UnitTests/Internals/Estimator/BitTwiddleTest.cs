// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Xunit;

namespace PennyLogger.Internals.Estimator.UnitTests
{
    /// <summary>
    /// Tests the <see cref="BitTwiddle"/> class
    /// </summary>
    public class BitTwiddleTest
    {
        /// <summary>
        /// Test <see cref="BitTwiddle.FindFirstSet(ulong)"/>
        /// </summary>
        [Fact]
        public void FindFirstSet()
        {
            Assert.Equal(0, BitTwiddle.FindFirstSet(0));
            Assert.Equal(0, BitTwiddle.FindFirstSet(1));
            Assert.Equal(1, BitTwiddle.FindFirstSet(2));
            Assert.Equal(1, BitTwiddle.FindFirstSet(3));
            Assert.Equal(2, BitTwiddle.FindFirstSet(4));
            Assert.Equal(32, BitTwiddle.FindFirstSet(0x0000_0001_0000_0000));
            Assert.Equal(63, BitTwiddle.FindFirstSet(0xffff_ffff_ffff_ffff));
        }

        /// <summary>
        /// Test <see cref="BitTwiddle.CountBitsSet(long)"/>
        /// </summary>
        [Fact]
        public void CountBitsSet()
        {
            Assert.Equal(0, BitTwiddle.CountBitsSet(0L));
            Assert.Equal(4, BitTwiddle.CountBitsSet(0xfL));
            Assert.Equal(16, BitTwiddle.CountBitsSet(0xffffL));
            Assert.Equal(64, BitTwiddle.CountBitsSet(0xffff_ffff_ffff_ffffL));
        }

        /// <summary>
        /// Test <see cref="BitTwiddle.RoundUpToPowerOf2(long)"/>
        /// </summary>
        [Fact]
        public void RoundUpToPowerOf2()
        {
            Assert.Equal(1, BitTwiddle.RoundUpToPowerOf2(1));
            Assert.Equal(2, BitTwiddle.RoundUpToPowerOf2(2));
            Assert.Equal(4, BitTwiddle.RoundUpToPowerOf2(3));
            Assert.Equal(4, BitTwiddle.RoundUpToPowerOf2(4));
            Assert.Equal(1024, BitTwiddle.RoundUpToPowerOf2(1023));
            Assert.Equal(1024, BitTwiddle.RoundUpToPowerOf2(1024));
            Assert.Equal(2048, BitTwiddle.RoundUpToPowerOf2(1025));
        }
    }
}
