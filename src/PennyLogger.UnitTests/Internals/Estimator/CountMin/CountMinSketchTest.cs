// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.UnitTests;
using Xunit;

namespace PennyLogger.Internals.Estimator.CountMin.UnitTests
{
    /// <summary>
    /// Test <see cref="CountMinSketch"/>
    /// </summary>
    public class CountMinSketchTest : FrequencyEstimatorTestBase<CountMinSketch>
    {
        /// <inheritdoc/>
        protected override CountMinSketch Create() => new CountMinSketch(0.001, 0.001);

        // CountMinSketch never returns NoCapacity, as there is no limit to the number of collisions. Disable the
        // capacity test.

        /// <inheritdoc/>
        protected override int MinValuesBeforeCapacity => 0;

        /// <inheritdoc/>
        protected override int MaxValuesBeforeCapacity => 0;

        /// <summary>
        /// Test <see cref="CountMinSketch.Add(CountMinSketch)"/>
        /// </summary>
        [Fact]
        public void Add()
        {
            var hashA = Hash.Create(1);
            var hashB = Hash.Create(2);

            var cms1 = new CountMinSketch(0.001, 0.001);
            var result1 = cms1.TryIncrementAndEstimate(hashA, out long estimate1);
            Assert.Equal(IncrementResult.Success, result1);
            Assert.Equal(1, estimate1);

            var cms2 = new CountMinSketch(0.001, 0.001);
            var result2 = cms2.TryIncrementAndEstimate(hashA, out long estimate2);
            Assert.Equal(IncrementResult.Success, result2);
            Assert.Equal(1, estimate2);

            cms1.Add(cms2);
            Assert.Equal(2, cms1.Estimate(hashA));
            Assert.Equal(0, cms1.Estimate(hashB));
        }
    }
}
