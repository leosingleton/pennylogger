// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator;

namespace PennyLogger.EstimatorTest
{
    /// <summary>
    /// Dummy implementation of a frequency estimator that always returns zero
    /// </summary>
    internal class ZeroEstimator : IFrequencyEstimator
    {
        /// <inheritdoc/>
        public long MaxBytes { get; set; }

        /// <inheritdoc/>
        public long MaxCount => 0;

        /// <inheritdoc/>
        public long TotalBytes => 0;

        /// <inheritdoc/>
        public long BytesUsed => 0;

        /// <inheritdoc/>
        public long Estimate(Hash hash) => 0;

        /// <inheritdoc/>
        public IncrementResult TryIncrementAndEstimate(Hash hash, out long estimate)
        {
            estimate = 1;
            return IncrementResult.Overflow;
        }

        /// <inheritdoc/>
        public void Clear()
        {
        }
    }
}
