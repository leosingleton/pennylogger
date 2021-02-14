// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace PennyLogger.Internals.Dictionary
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey, TValue}"/>
    /// </summary>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Gets a value from the dictionary, without throwing a <see cref="KeyNotFoundException"/>
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <param name="key">Key to get</param>
        /// <returns>Value of the key, or null if not found</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : class
        {
            bool found = dict.TryGetValue(key, out var value);
            return found ? value : null;
        }

        /// <summary>
        /// Calculates a hash code, taking into account the contents of the dictionary, unlike the standard
        /// <see cref="object.GetHashCode"/> implementation
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <returns>Hash code</returns>
        public static int GetDeepHashCode<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            unchecked
            {
                int hash = -1234567;

                foreach (var kvp in dict)
                {
                    hash ^= kvp.Key.GetHashCode() + kvp.Value.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Determines whether two dictionaries are equal, taking into account the contents, unlike the standard
        /// <see cref="object.Equals(object)"/> implementation
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dict1">Dictionary</param>
        /// <param name="dict2">Dictionary</param>
        /// <returns></returns>
        public static bool DeepEquals<TKey, TValue>(this IDictionary<TKey, TValue> dict1,
            IDictionary<TKey, TValue> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (var kvp in dict1)
            {
                if (!dict2.ContainsKey(kvp.Key) || !dict2[kvp.Key].Equals(kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns an object, which when compared, performs a deep comparison of the dictionary
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <returns>
        /// Object with <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> overridden to perform
        /// a deep comparison of the dictionary. This object also overrides the == and != operators.
        /// </returns>
        public static DictionaryValueType<TKey, TValue> ToValueType<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return new DictionaryValueType<TKey, TValue>(dict);
        }

        /// <summary>
        /// Value type returned by <see cref="ToValueType{TKey, TValue}(IDictionary{TKey, TValue})"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public class DictionaryValueType<TKey, TValue> : IEquatable<DictionaryValueType<TKey, TValue>>
        {
            internal DictionaryValueType(IDictionary<TKey, TValue> value)
            {
                Value = value;
            }

            /// <inheritdoc/>
            public bool Equals(DictionaryValueType<TKey, TValue> other) => Value.DeepEquals(other.Value);

            /// <inheritdoc/>
            public override bool Equals(object obj) =>
                obj is DictionaryValueType<TKey, TValue> other && Value.DeepEquals(other.Value);

            /// <inheritdoc/>
            public override int GetHashCode() => Value.GetDeepHashCode();

            private readonly IDictionary<TKey, TValue> Value;

            /// <inheritdoc/>
            public static bool operator ==(DictionaryValueType<TKey, TValue> dict1,
                DictionaryValueType<TKey, TValue> dict2) => ReferenceEquals(dict1, dict2) ||
                (dict1 is object && dict2 is object && dict1.Value.DeepEquals(dict2.Value));

            /// <inheritdoc/>
            public static bool operator !=(DictionaryValueType<TKey, TValue> dict1,
                DictionaryValueType<TKey, TValue> dict2) => !ReferenceEquals(dict1, dict2) &&
                (dict1 is null || dict2 is null || !dict1.Value.DeepEquals(dict2.Value));
        }
    }
}
