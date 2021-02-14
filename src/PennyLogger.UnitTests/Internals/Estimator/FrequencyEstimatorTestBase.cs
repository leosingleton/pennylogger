// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.EstimatorTest;
using Xunit;

namespace PennyLogger.Internals.Estimator.UnitTests
{
    /// <summary>
    /// Base class for unit tests for frequency estimators. Provides a set of generic test cases that apply regardless
    /// of implementation.
    /// </summary>
    /// <typeparam name="T">Frequency estimator to test</typeparam>
    public abstract class FrequencyEstimatorTestBase<T>
        where T : IFrequencyEstimator
    {
        /// <summary>
        /// Derived classes must override this method to instantiate a frequency estimator
        /// </summary>
        /// <returns>Frequency estimator of type <typeparamref name="T"/></returns>
        protected abstract T Create();

        /// <summary>
        /// Derived classes must override this property to return the expected number of one-off values the estimator
        /// can hold. This property is the minimum, and the actual amount must be within the min-max range to pass.
        /// </summary>
        protected abstract int MinValuesBeforeCapacity { get; }

        /// <summary>
        /// Derived classes must override this property to return the expected number of one-off values the estimator
        /// can hold. This property is the maximum, and the actual amount must be within the min-max range to pass.
        /// </summary>
        protected abstract int MaxValuesBeforeCapacity { get; }

        /// <summary>
        /// A value's estimate is initialized to zero
        /// </summary>
        [Fact]
        public void InitializedToZero()
        {
            T est = Create();
            var hash = Hash.Create("Test");
            Assert.Equal(0, est.Estimate(hash));
        }

        /// <summary>
        /// Increment one value until it overflows
        /// </summary>
        [Fact]
        public void OverflowOneValue()
        {
            T est = Create();
            var hash = Hash.Create("Test");

            for (long n = 0; n < est.MaxCount; n++)
            {
                var result = est.TryIncrementAndEstimate(hash, out long estimate);
                Assert.Equal(IncrementResult.Success, result);
                Assert.Equal(n + 1, estimate);
            }

            {
                var result = est.TryIncrementAndEstimate(hash, out long estimate);
                Assert.Equal(IncrementResult.Overflow, result);
                Assert.Equal(est.MaxCount + 1, estimate);
            }
        }

        /// <summary>
        /// Add unique values until we reach capacity
        /// </summary>
        [Fact]
        public void InsertValuesToCapacity()
        {
            // If the estimator doesn't have capacity limits (such as CountMinSketch), skip this test
            if (MinValuesBeforeCapacity < 1 || MaxValuesBeforeCapacity < 1)
            {
                return;
            }

            T est = Create();

            for (int n = 0; ; n++)
            {
                var hash = Hash.Create(n);
                var result = est.TryIncrementAndEstimate(hash, out long _);

                // It is possible, particularly on a non-counting cuckoo filter, to get an overflow due to collisions.
                // Treat these the same as success.

                if (result == IncrementResult.NoCapacity)
                {
                    Assert.InRange(n, MinValuesBeforeCapacity, MaxValuesBeforeCapacity);
                    return;
                }
            }
        }

        /// <summary>
        /// Clears a value
        /// </summary>
        [Fact]
        public void Clear()
        {
            T est = Create();

            var hash = Hash.Create("Test");
            var result = est.TryIncrementAndEstimate(hash, out long estimate);
            Assert.Equal(IncrementResult.Success, result);
            Assert.Equal(1, estimate);
            Assert.True(est.BytesUsed > 0);

            est.Clear();
            Assert.Equal(0, est.Estimate(hash));
            Assert.Equal(0, est.BytesUsed);
        }

        /// <summary>
        /// Stress test the estimator using the EstimatorTest app
        /// </summary>
        [Fact]
        public void EstimatorTestStress()
        {
            T est = Create();
            var simEst = new SimEstimator("Estimator", est);
            var sim = new Simulation(5, 1000, 0.25, 0.3, false, true, simEst);
            sim.Run();
        }
    }
}
