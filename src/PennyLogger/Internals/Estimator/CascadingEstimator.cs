// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Linq;

namespace PennyLogger.Internals.Estimator
{
    /// <summary>
    /// Frequency estimation algorithms generally consume more memory per value, the larger the maximum count they hold.
    /// To optimize memory usage, the cascading estimator combines a collection of estimators, with increasing maximum
    /// count. As the count for a value exceeds the smaller estimator, it &quot;cascades&quot; into the larger
    /// estimator.
    /// </summary>
    public class CascadingEstimator : IFrequencyEstimator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="estimators">Array of estimator instances, ordered from smallest to largest</param>
        public CascadingEstimator(params IFrequencyEstimator[] estimators)
        {
            if (estimators.Length < 2)
            {
                throw new ArgumentException(
                    "CascadingEstimator requires a minimum of 2 estimators as a constructor parameter");
            }

            Estimators = estimators;

            RecomputeMaxBytes();
        }

        /// <summary>
        /// Array of estimator instances, ordered from smallest to largest
        /// </summary>
        private readonly IFrequencyEstimator[] Estimators;

        /// <inheritdoc/>
        public long MaxBytes
        {
            get => _MaxBytes;

            set
            {
                _MaxBytes = value;
                RecomputeMaxBytes();
            }
        }
        private long _MaxBytes = long.MaxValue;

        /// <inheritdoc/>
        public long MaxCount => Estimators.Sum(est => est.MaxCount);

        /// <inheritdoc/>
        public long TotalBytes => Estimators.Sum(est => est.TotalBytes);

        /// <inheritdoc/>
        public long BytesUsed => Estimators.Sum(est => est.BytesUsed);

        /// <inheritdoc/>
        public long Estimate(Hash hash) => Estimators.Sum(est => est.Estimate(hash));

        /// <inheritdoc/>
        public IncrementResult TryIncrementAndEstimate(Hash hash, out long estimate)
        {
            // There is an unintuitive off-by-one error here. On overflow, TryIncrementAndEstimate() returns
            // MaxCount + 1. If a value cascades across more than one estimator, this becomes +2, +3, etc.
            // So we subtract one from each estimate and add back a single +1 here.
            estimate = 1;

            foreach (var est in Estimators)
            {
                var result = est.TryIncrementAndEstimate(hash, out long value);
                estimate += value - 1;

                if (result != IncrementResult.Overflow)
                {
                    RecomputeMaxBytes();
                    return result;
                }
            }

            return IncrementResult.Overflow;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            foreach (var est in Estimators)
            {
                est.Clear();
            }

            RecomputeMaxBytes();
        }

        private void RecomputeMaxBytes()
        {
            long bytesRemaining = Math.Max(MaxBytes - TotalBytes, 0);

            foreach (var est in Estimators)
            {
                est.MaxBytes = est.TotalBytes + bytesRemaining;
            }
        }
    }
}
