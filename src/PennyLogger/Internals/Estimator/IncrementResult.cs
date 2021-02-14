// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator
{
    /// <summary>
    /// Return values for <see cref="IFrequencyEstimator.TryIncrementAndEstimate(Hash, out long)"/>
    /// </summary>
    public enum IncrementResult
    {
        /// <summary>
        /// The value was successfully incremented
        /// </summary>
        Success,

        /// <summary>
        /// The value is unchanged because incrementing it would cause an overflow
        /// </summary>
        Overflow,

        /// <summary>
        /// The value was not found, and was not able to be inserted because there is not enough capacity in the filter
        /// </summary>
        NoCapacity
    }
}
