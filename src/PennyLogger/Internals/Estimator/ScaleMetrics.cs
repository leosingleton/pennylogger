// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator
{
    /// <summary>
    /// Metrics provided by <see cref="ScalableEstimator"/> to calculate the size of the new estimator when
    /// increasing the number of estimator instances
    /// </summary>
    public struct ScaleMetrics
    {
        /// <summary>
        /// The number of current estimators (will be zero on the first call)
        /// </summary>
        public int EstimatorCount;

        /// <summary>
        /// Remaining memory, in bytes
        /// </summary>
        public long BytesRemaining;

        /// <summary>
        /// The previously-created estimator, or null if this is the first
        /// </summary>
        public IFrequencyEstimator PreviousEstimator;

        /// <summary>
        /// The number of estimators the last time <see cref="ScalableEstimator.Clear"/> was called (or zero if
        /// never called)
        /// </summary>
        public int LastClearEstimatorCount;

        /// <summary>
        /// The value of <see cref="ScalableEstimator.TotalBytes"/> the last time
        /// <see cref="ScalableEstimator.Clear"/> was called (or zero if never called)
        /// </summary>
        public long LastClearTotalBytes;

        /// <summary>
        /// The value of <see cref="ScalableEstimator.BytesUsed"/> the last time
        /// <see cref="ScalableEstimator.Clear"/> was called (or zero if never called)
        /// </summary>
        public long LastClearBytesUsed;
    }
}
