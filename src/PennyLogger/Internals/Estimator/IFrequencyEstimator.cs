// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator
{
    /// <summary>
    /// Generic interface for a probabilistic frequency estimator that maximizes space-efficiency at the expense of
    /// accuracy. PennyLogger has a pluggable model, as probabilistic data structures are an area of active research
    /// and there is no one optimal algorithm.
    /// </summary>
    public interface IFrequencyEstimator
    {
        /// <summary>
        /// Controls the maximum amount of memory used by the frequency estimator, in bytes. Note that this limit is an
        /// estimate and not strictly enforced. Also, note that reducing this setting after adding values may not free
        /// up memory that has already been allocated.
        /// </summary>
        public long MaxBytes { get; set; }

        /// <summary>
        /// Maximum count which may be stored for a value
        /// </summary>
        public long MaxCount { get; }

        /// <summary>
        /// Total memory (in bytes) consumed by the frequency estimator, including extra memory allocated but currently
        /// not holding a value.
        /// </summary>
        public long TotalBytes { get; }

        /// <summary>
        /// Total memory (in bytes) consumed by the frequency estimator. Dividing this value by <see cref="TotalBytes"/>
        /// can estimate the load factor of the frequency estimator.
        /// </summary>
        public long BytesUsed { get; }

        /// <summary>
        /// Estimates the count of a value
        /// </summary>
        /// <param name="hash">Value, specified as a 128-bit hash</param>
        /// <returns>Estimated count</returns>
        public long Estimate(Hash hash);

        /// <summary>
        /// Increments the count of a value and returns the post-incremented estimate
        /// </summary>
        /// <param name="hash">Value, specified as a 128-bit hash</param>
        /// <param name="estimate">Returns the estimated count, post-increment</param>
        /// <returns>See <see cref="IncrementResult"/></returns>
        public IncrementResult TryIncrementAndEstimate(Hash hash, out long estimate);

        /// <summary>
        /// Zeroes the count of all values
        /// </summary>
        /// <remarks>
        /// Calling <see cref="Clear"/> is preferrable to instantiating a new frequency estimator because it clears all
        /// values, without necessarily clearing out all state. Some frequency estimators may optimize themselves based
        /// on the input from past runs, so may perform better over time.
        /// </remarks>
        public void Clear();
    }
}
