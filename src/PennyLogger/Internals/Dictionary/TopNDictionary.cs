// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PennyLogger.Internals.Dictionary
{
    /// <summary>
    /// Dictionary-like data structure for tracking the Top-N of a set of items
    /// </summary>
    /// <typeparam name="T">Type of item to track</typeparam>
    /// <remarks>
    /// Unlike the built-in .NET collections that do not accept null as a key, null is a valid value of
    /// <typeparamref name="T"/> in this class.
    /// </remarks>
    internal class TopNDictionary<T> : IEnumerable<KeyValuePair<T, long>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxValues">Maximum number of values to track (the N in Top-N)</param>
        public TopNDictionary(long maxValues)
        {
            MaxValues = maxValues;
            Values = new Dictionary<T, long>();
        }

        /// <summary>
        /// Maximum number of values to track. This value is set in the constructor and cannot be changed.
        /// </summary>
        public long MaxValues { get; private set; }

        /// <summary>
        /// Minimum count required to be in the Top-N. Attempting to insert a value with this count or lower will fail.
        /// </summary>
        public long MinCount { get; private set; }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<T, long>> GetEnumerator() => Values
            .Append(new KeyValuePair<T, long>(default, NullCount))
            .Where(kvp => kvp.Value > 0)
            .OrderByDescending(kvp => kvp.Value)
            .GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Number of items tracked. This value is always &lt;= <see cref="MaxValues"/>.
        /// </summary>
        public long Count => Values.Count + ((NullCount > 0) ? 1 : 0);

        /// <summary>
        /// Adds a new value to the collection. If the collection already has <see cref="MaxValues"/> items, then the
        /// lowest-valued item will be automatically removed to make room.
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="count">Value's count. This must be greater than <see cref="MinCount"/>.</param>
        /// <returns>True on success. False if <paramref name="count"/> &lt;= <see cref="MinCount"/>.</returns>
        public bool TryAdd(T value, long count)
        {
            if (count <= MinCount)
            {
                return false;
            }

            // Insert the new value/count pair. Normally this is tracked in the Values dictionary, but null is not a
            // valid key. Track it specially.
            if (value == null)
            {
                NullCount = count;
            }
            else
            {
                Values[value] = count;
            }

            // If the number of items in the Top-N collection now exceeds MaxValues, remove the lowest-valued item.
            if (Count >= MaxValues)
            {
                if (NullCount == MinCount)
                {
                    // The lowest-valued item is null. Remove null from the Top-N.
                    NullCount = 0;
                }
                else
                {
                    // The lowest-valued item is not null. Remove the lowest-valued item from the Values dictionary.
                    T toRemove = Values
                        .Where(kvp => kvp.Value == MinCount)
                        .First()
                        .Key;
                    Values.Remove(toRemove);
                }

                RecomputeMinCount();
            }

            return true;
        }

        /// <summary>
        /// Adds a new value to the collection. If the collection already has <see cref="MaxValues"/> items, then the
        /// lowest-valued item will be automatically removed to make room.
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="count">Value's count. This must be greater than <see cref="MinCount"/>.</param>
        /// <exception cref="ArgumentException">If <paramref name="count"/> &lt;= <see cref="MinCount"/></exception>
        public void Add(T value, long count)
        {
            if (!TryAdd(value, count))
            {
                throw new ArgumentException(
                    $"Cannot add value with count of {count}. Minimum is {MinCount}.",
                    nameof(count));
            }
        }

        /// <summary>
        /// Increments an existing value in the collection. If the value is not found and the collection is not full,
        /// inserts the value with a count of 1.
        /// </summary>
        /// <param name="value">Value to increment</param>
        /// <returns>True on success. False if the value was not found and the collection is full.</returns>
        public bool TryIncrement(T value)
        {
            // Special-case null values. The .NET Dictionary class doesn't allow them as keys
            if (value == null)
            {
                if (NullCount > MinCount)
                {
                    NullCount++;
                    return true;
                }
                else if (NullCount == MinCount)
                {
                    // Null is the lowest-count value in the collection. Increment it, then recompute the MinCount
                    // property, as it may have changed.
                    NullCount++;
                    RecomputeMinCount();
                    return true;
                }
                else
                {
                    // NullCount < MinCount. The collection is full, otherwise MinCount would be zero.
                    return false;
                }
            }

            // If the value already exists in the Top-N, increment it.
            if (Values.ContainsKey(value))
            {
                if (Values[value]++ == MinCount)
                {
                    // The value was previously the lowest-count value in the collection. Recompute the MinCount
                    // property, as it may have changed.
                    RecomputeMinCount();
                }
                return true;
            }

            long size = Count;
            if (size < MaxValues)
            {
                // The Top-N collection is not full. Insert the new value with a count of 1.
                Values[value] = 1;

                if (size + 1 == MaxValues)
                {
                    // Adding a new value caused the collection to be full. The MinCount can no longer be zero, and must
                    // now be 1, since we just inserted a count of 1.
                    MinCount = 1;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Increments an existing value in the collection. If the value is not found and the collection is not full,
        /// inserts the value with a count of 1.
        /// </summary>
        /// <param name="value">Value to increment</param>
        /// <exception cref="ArgumentException">If the value was not found and the collection is full.</exception>
        public void Increment(T value)
        {
            if (!TryIncrement(value))
            {
                throw new ArgumentException(
                    $"Cannot increment value {value} because it is not contained in the dictionary.",
                    nameof(value));
            }
        }

        /// <summary>
        /// Clears all values in the collection
        /// </summary>
        public void Clear()
        {
            Values.Clear();
            MinCount = 0;
            NullCount = 0;
        }

        private void RecomputeMinCount()
        {
            // MinCount is always 0 when the collection is not full.
            if (Count < MaxValues)
            {
                MinCount = 0;
                return;
            }

            // Special case: If MaxValue == 1 and the Values dictionary is empty, then only null values exist.
            if (Values.Count == 0)
            {
                MinCount = NullCount;
                return;
            }

            // Find the minimum count in the Values dictionary.
            MinCount = Values.Min(kvp => kvp.Value);

            // The minimum count could be null, which is tracked separately, as Dictionary doesn't support null as a
            // valid key.
            if (NullCount > 0 && NullCount < MinCount)
            {
                MinCount = NullCount;
            }
        }

        private readonly Dictionary<T, long> Values;
        private long NullCount;
    }
}
