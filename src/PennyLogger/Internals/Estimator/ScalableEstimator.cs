// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace PennyLogger.Internals.Estimator
{
    /// <summary>
    /// Frequency estimation algorithms generally rely on fixed-size internal data structures. To handle an unknown
    /// amount of data, this wrapper scales out by adding a new, larger instance whenever
    /// <see cref="IncrementResult.NoCapacity"/> is returned.
    /// </summary>
    public class ScalableEstimator : IFrequencyEstimator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="createEstimator">
        /// Lambda function to create a new estimator, taking the metrics in <see cref="ScaleMetrics"/> as a parameter.
        /// The lambda must create a new estimator and return it, or return null.
        /// </param>
        public ScalableEstimator(Func<ScaleMetrics, IFrequencyEstimator> createEstimator)
        {
            CreateEstimator = createEstimator;
            Estimators = new List<IFrequencyEstimator>();
        }

        /// <summary>
        /// Constructor. This overload uses a default mapping of metrics to size. Use the
        /// <see cref="ScalableEstimator(Func{ScaleMetrics, IFrequencyEstimator})"/> constructor to create a custom
        /// mapping of metrics to size.
        /// </summary>
        /// <param name="createEstimator">
        /// Lambda function to create a new estimator, taking the size in bytes as a parameter
        /// </param>
        public ScalableEstimator(Func<long, IFrequencyEstimator> createEstimator)
        {
            CreateEstimator = metrics =>
            {
                long size = DefaultSizeCalculator(metrics);
                return (size > 0) ? createEstimator(size) : null;
            };
            Estimators = new List<IFrequencyEstimator>();
        }

        private readonly Func<ScaleMetrics, IFrequencyEstimator> CreateEstimator;

        private int LastClearEstimatorCount;
        private long LastClearTotalBytes;
        private long LastClearBytesUsed;

        /// <summary>
        /// Collection of estimator instances, ordered from largest (newest) to smallest (oldest)
        /// </summary>
        private readonly List<IFrequencyEstimator> Estimators;

        /// <inheritdoc/>
        public long MaxBytes { get; set; } = long.MaxValue;

        /// <inheritdoc/>
        public long MaxCount => (Estimators.FirstOrDefault() ?? CreateTemporaryEstimator()).MaxCount;

        /// <inheritdoc/>
        public long TotalBytes => Estimators.Sum(est => est.TotalBytes);

        /// <inheritdoc/>
        public long BytesUsed => Estimators.Sum(est => est.BytesUsed);

        /// <inheritdoc/>
        public long Estimate(Hash hash) => Estimators.Sum(est => est.Estimate(hash));

        /// <inheritdoc/>
        public IncrementResult TryIncrementAndEstimate(Hash hash, out long estimate)
        {
            estimate = Estimate(hash) + 1;

            // It is possible that due to scaling and being spread across multiple estimators, a single value reaches
            // MaxCount before getting an overflow error from any of the underlying estimators. Even though we could
            // choose to keep counting above MaxCount, we choose not to and instead return an overflow here.
            //
            // The rationale is that the caller may make assumptions about the count. For instance, the
            // CascadingEstimator moves a value to a more accurate estimator as the value increases above the MaxCount
            // threshold. Not returning overflow would delay this and lead to more inaccuracies.
            if (estimate > MaxCount)
            {
                return IncrementResult.Overflow;
            }

            var est = Estimators.FirstOrDefault();
            if (est != null)
            {
                var result = est.TryIncrementAndEstimate(hash, out _);
                if (result != IncrementResult.NoCapacity)
                {
                    return result;
                }
            }

            est = AddEstimator();
            if (est != null)
            {
                return est.TryIncrementAndEstimate(hash, out _);
            }

            estimate = 0;
            return IncrementResult.NoCapacity;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (Estimators.Count > 0)
            {
                // Save performance metrics for future calls to AddEstimator()
                LastClearEstimatorCount = Estimators.Count;
                LastClearTotalBytes = TotalBytes;
                LastClearBytesUsed = BytesUsed;

                Estimators.Clear();
            }
        }

        /// <summary>
        /// Attempts to create and add a new estimator
        /// </summary>
        /// <returns>
        /// New <see cref="IFrequencyEstimator"/> instance on success, or null if a new filter was unable to be added
        /// </returns>
        private IFrequencyEstimator AddEstimator()
        {
            // Determine the amount of memory remaining. If we have exceeded it, or do not have enough to create a new
            // estimator, abort.
            long bytesRemaining = MaxBytes - BytesUsed;
            var prevEstimator = Estimators.FirstOrDefault();
            if (bytesRemaining <= (prevEstimator?.TotalBytes ?? 0))
            {
                return null;
            }

            var estimator = CreateEstimator(new ScaleMetrics
            {
                EstimatorCount = Estimators.Count,
                BytesRemaining = bytesRemaining,
                PreviousEstimator = prevEstimator,
                LastClearEstimatorCount = LastClearEstimatorCount,
                LastClearTotalBytes = LastClearTotalBytes,
                LastClearBytesUsed = LastClearBytesUsed
            });
            if (estimator != null)
            {
                // Ensure the size of the estimators increases the more estimators we create
                if (estimator.TotalBytes <= prevEstimator?.TotalBytes)
                {
                    throw new InvalidOperationException(
                        $"CreateEstimator returned an estimator of size {estimator.TotalBytes} which is not larger " +
                        $"than the previous size of {prevEstimator.TotalBytes}");
                }

                // Ensure the MaxCount of all estimators is the same
                if (prevEstimator != null && estimator.MaxCount != prevEstimator.MaxCount)
                {
                    throw new InvalidOperationException(
                        $"CreateEstimator returned an estimator with MaxCount={estimator.MaxCount} when " +
                        $"{prevEstimator.MaxCount} was expected");
                }

                // Insert the new estimator at the head of the list
                Estimators.Insert(0, estimator);
            }

            return estimator;
        }

        /// <summary>
        /// Creates a temporary estimator to use in <see cref="MaxCount"/>
        /// </summary>
        private IFrequencyEstimator CreateTemporaryEstimator() => CreateEstimator(new ScaleMetrics
        {
            EstimatorCount = 0,
            BytesRemaining = 1024,
            PreviousEstimator = null,
            LastClearEstimatorCount = 0,
            LastClearTotalBytes = 0,
            LastClearBytesUsed = 0
        });

        /// <summary>
        /// Default method to process metrics and determine the size next estimator to create. Used by the
        /// <see cref="ScalableEstimator(Func{long, IFrequencyEstimator})"/> constructor.
        /// </summary>
        /// <param name="metrics">Scale metrics</param>
        /// <returns>
        /// Size of the next estimator, in bytes, or zero if there is not enough memory remaining to create another
        /// estimator
        /// </returns>
        private static long DefaultSizeCalculator(ScaleMetrics metrics)
        {
            // Minimum size is larger than the previous estimator, or 1024 bytes
            long minSize = Math.Max((metrics.PreviousEstimator?.TotalBytes ?? 0) + 1, 1024);
            if (minSize > metrics.BytesRemaining)
            {
                // It is not possible to create another estimator within the remaining memory
                return 0;
            }

            // Estimate the ideal size based on 80% load factor, or 4x the last estimator
            long idealSize = (long)(metrics.LastClearBytesUsed / 0.8);
            idealSize = Math.Max(idealSize, (metrics.PreviousEstimator?.TotalBytes ?? 0) * 4);

            // Calculate the actual size based on min, max and memory remaining
            return Math.Min(Math.Max(minSize, idealSize), metrics.BytesRemaining);
        }
    }
}
