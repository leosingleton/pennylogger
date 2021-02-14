// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Dictionary;
using PennyLogger.Internals.Estimator;
using PennyLogger.Internals.Estimator.CascadingCuckoo;
using PennyLogger.Internals.Reflection;
using System.Collections.Generic;

namespace PennyLogger.Internals.Aggregate
{
    /// <summary>
    /// Object to track the Top-N values for an enumerable property. This implementation uses a lossy frequency
    /// estimation data structure, which guarantees bounded memory consumption, at the expense of accurracy as the
    /// number of unique input values grows.
    /// </summary>
    /// <typeparam name="T">
    /// Property type. Nearly all .NET types can be configured as &quot;enumerable&quot; in PennyLogger. For instance,
    /// an <see cref="int"/> representing an HTTP status code would be enumerable.
    /// </typeparam>
    internal class AggregateEnumerablePropertyLossy<T> : AggregateEnumerableProperty<T>
    {
        /// <inheritdoc/>
        public AggregateEnumerablePropertyLossy(PropertyReflector<T> property, PennyPropertyConfig config) :
            base(property, config)
        {
            // Keep the top 2*N values in a dictionary. We'll only ever show the top N, but storing a few more in a
            // lossless data structure improves the accuracy.
            Counts = new TopNDictionary<T>(config.Enumerable.Top * 2);

            // For now, the frequency estimator is hardcoded to a 4-way cascading scaling cuckoo filter. Although there
            // are other data structures in the Estimators/ directory, this one performed the best in testing. In the
            // future, it might make sense to pick the algorithm via config and implement a factory method here instead.
            Estimator = new CascadingCuckooFilter4Way
            {
                MaxBytes = config.Enumerable.MaxMemory
            };
        }

        /// <inheritdoc/>
        public override IEnumerable<KeyValuePair<T, long>> GetSortedCounts() => Counts;

        /// <inheritdoc/>
        protected override void AddValue(T value, bool first)
        {
            if (!Counts.TryIncrement(value))
            {
                // The first time the TopNDictionary overflows, copy its values into the frequency estimator.
                if (!UseEstimator)
                {
                    UseEstimator = true;

                    foreach (var kvp in Counts)
                    {
                        var hash1 = Property.GetHashValue(kvp.Key);
                        long count = kvp.Value;

                        // BUGBUG: IFrequencyEstimator doesn't have a Set() method, so we call increment in a for
                        //         loop. This is probably a good area for optimization.
                        for (int n = 0; n < count; n++)
                        {
                            Estimator.TryIncrementAndEstimate(hash1, out var _);
                        }
                    }
                }

                // The value is not (yet) in the top 2*N. Increment it in the frequency estimator, but check the
                // newly-estimated count to see if it belongs in the top 2*N post-increment.
                var hash2 = Property.GetHashValue(value);
                Estimator.TryIncrementAndEstimate(hash2, out long estimate);
                if (estimate > Counts.MinCount)
                {
                    Counts.Add(value, estimate);
                }
            }
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Counts.Clear();
            Estimator.Clear();
            UseEstimator = false;
        }

        /// <summary>
        /// Top 2*N values and their counts are tracked here
        /// </summary>
        private readonly TopNDictionary<T> Counts;

        /// <summary>
        /// Frequency estimator. Used to track all values, not just the top 2*N, however this uses a probabilistic data
        /// structure, so has a lower accuracy than <see cref="Counts"/>.
        /// </summary>
        private readonly IFrequencyEstimator Estimator;

        /// <summary>
        /// Initially, we do not use <see cref="Estimator"/> and put all values in the <see cref="Counts"/> collection.
        /// This value tracks when the frequency estimator is in use.
        /// </summary>
        private bool UseEstimator = false;
    }
}
