// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Dictionary;
using PennyLogger.Internals.Reflection;
using System.Collections.Generic;

namespace PennyLogger.Internals.Aggregate
{
    /// <summary>
    /// Object to track the Top-N values for an enumerable property. This implementation uses a simple hash table data
    /// structure, which while accurate, may result in unbounded memory consumption if there are too many unique input
    /// values.
    /// </summary>
    /// <typeparam name="T">
    /// Property type. Nearly all .NET types can be configured as &quot;enumerable&quot; in PennyLogger. For instance,
    /// an <see cref="int"/> representing an HTTP status code would be enumerable.
    /// </typeparam>
    internal class AggregateEnumerablePropertyLossless<T> : AggregateEnumerableProperty<T>
    {
        /// <inheritdoc/>
        public AggregateEnumerablePropertyLossless(PropertyReflector<T> property, PennyPropertyConfig config) :
            base(property, config)
        {
            Counts = new TopNDictionary<T>(long.MaxValue);
        }

        /// <inheritdoc/>
        public override IEnumerable<KeyValuePair<T, long>> GetSortedCounts() => Counts;

        /// <inheritdoc/>
        protected override void AddValue(T value, bool first)
        {
            Counts.Increment(value);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Counts.Clear();
        }

        private readonly TopNDictionary<T> Counts;
    }
}
