// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator.Cuckoo
{
    /// <summary>
    /// Interface implemented by structs used for cuckoo filter buckets
    /// </summary>
    public interface ICuckooBucket
    {
        /// <summary>
        /// Gets or sets the fingerprint of the value being counted. This is the hash from <see cref="Hash.Hash2"/>.
        /// </summary>
        /// <remarks>
        /// Note that the data type is a 64-bit value, however most <see cref="ICuckooBucket"/> implementations will
        /// truncate it to a smaller value for memory efficiency, at the risk of a higher rate of collisions.
        /// </remarks>
        public ulong Fingerprint { get; set; }

        /// <summary>
        /// Gets or sets the count of the value being counted. This must be a positive, non-zero value.
        /// </summary>
        /// <remarks>
        /// Note that the data type is a 64-bit value, however most <see cref="ICuckooBucket"/> implementations will
        /// truncate it to a smaller value for memory efficiency. At the most extreme, a traditional, non-counting
        /// cuckoo filter may just return a hardcoded value of 1 and not store the count whatsoever.
        /// </remarks>
        public ulong Count { get; set; }

        /// <summary>
        /// Returns the maximum value that may be stored in <see cref="Fingerprint"/>
        /// </summary>
        public ulong MaxFingerprint { get; }

        /// <summary>
        /// Returns the maximum value that may be stored in <see cref="Count"/>
        /// </summary>
        public ulong MaxCount { get; }

        /// <summary>
        /// Returns the size of this struct, in bytes
        /// </summary>
        public int SizeOf { get; }

        /// <summary>
        /// Determines whether this struct is empty, i.e. it holds the default value
        /// </summary>
        public bool IsEmpty { get; }
    }
}
