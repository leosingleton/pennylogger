// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PennyLogger.Internals.Aggregate
{
    /// <summary>
    /// Base class for objects that track the aggregate values of a single property as an enumerable type. When
    /// aggregating enumerable types, PennyLogger tracks the Top-N values along with their counts.
    /// </summary>
    /// <typeparam name="T">
    /// Property type. Nearly all .NET types can be configured as &quot;enumerable&quot; in PennyLogger. For instance,
    /// an <see cref="int"/> representing an HTTP status code would be enumerable.
    /// </typeparam>
    internal abstract class AggregateEnumerableProperty<T> : AggregateProperty<T>
    {
        /// <inheritdoc/>
        protected AggregateEnumerableProperty(PropertyReflector<T> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <summary>
        /// Derived classes must implement this method to return the top values and their counts, sorted from highest to
        /// lowest count.
        /// </summary>
        /// <returns>Top values and counts</returns>
        public abstract IEnumerable<KeyValuePair<T, long>> GetSortedCounts();

        /// <inheritdoc/>
        public override void Serialize(Utf8JsonWriter writer)
        {
            var topN = GetSortedCounts()
                .Select(kvp => KeyValuePair.Create(kvp.Key?.ToString() ?? "(null)", kvp.Value))
                .Take(Config.Enumerable.Top)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            JsonSerializer.Serialize(writer, topN);
        }
    }
}
