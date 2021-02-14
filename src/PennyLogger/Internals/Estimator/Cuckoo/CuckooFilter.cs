// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Linq;

namespace PennyLogger.Internals.Estimator.Cuckoo
{
    /// <summary>
    /// <para>
    /// Implementation of a cuckoo filter, a probablistic data structure for storing set membership in a very space-
    /// efficient manner. Cuckoo filters work by storing a fingerprint of the input values in the set within a hash
    /// table. However, rather than using chaining or linear probing to resolve hash table collisions, cuckoo filters
    /// use a fixed number of alternate locations, calculated using the fingerprint. Because the alternate locations can
    /// be calculated solely using the contents of the bucket, cuckoo filters are able to reorganize the existing
    /// elements to make room in the hash table or remove existing elements.
    /// </para>
    /// <para>
    /// For an introduction to cuckoo filters, see https://en.wikipedia.org/wiki/Cuckoo_filter
    /// </para>
    /// <para>
    /// This class supports both standard cuckoo filters as described in the article above, plus adds support for
    /// variations that include a counter along with the fingerprint.
    /// </para>
    /// <para>
    /// Although the parameters to the methods in this class use 64-bit fingerprints and 64-bit counters, these are
    /// purely maximum values, and the actual data stored is determined by the <typeparamref name="TBucket"/> type.
    /// </para>
    /// </summary>
    /// <typeparam name="TBucket">
    /// Struct implementing a single bucket in the cuckoo filter. This struct controls how much of the 64-bit
    /// fingerprints and counters are stored in memory. See <see cref="ICuckooBucket"/> for details.
    /// </typeparam>
    public abstract class CuckooFilter<TBucket> : IFrequencyEstimator
        where TBucket : ICuckooBucket, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">
        /// Size of the filter, in bytes. Note that the number of buckets in a cuckoo filter must be a power of 2, and
        /// a bucket may be more than one byte, so this value may be rounded up to satisfy the power-of-2 requirement.
        /// </param>
        protected CuckooFilter(long size)
        {
            long bucketSize = BucketSize;
            long bucketCount = Math.Max(BitTwiddle.RoundUpToPowerOf2(size / bucketSize), 4);
            MaxBytes = bucketCount * bucketSize;
            Buckets = new TBucket[bucketCount];
        }

        /// <summary>
        /// Array containing the actual bucket elements
        /// </summary>
        protected readonly TBucket[] Buckets;

        /// <summary>
        /// Size of one bucket, in bytes
        /// </summary>
        private static readonly long BucketSize = new TBucket().SizeOf;

        /// <summary>
        /// This property is ignored. The memory usage is completely determined at creation time based on the bucket
        /// count specified in the constructor and bucket size specified by <typeparamref name="TBucket"/>.
        /// </summary>
        public long MaxBytes { get; set; }

        /// <inheritdoc/>
        public long MaxCount => unchecked((long)new TBucket().MaxCount);

        /// <inheritdoc/>
        public long TotalBytes => Buckets.LongLength * BucketSize;

        /// <inheritdoc/>
        public long BytesUsed => Buckets.LongCount(entry => !entry.IsEmpty) * BucketSize;

        /// <inheritdoc/>
        public long Estimate(Hash hash)
        {
            ulong fingerprint = CalculateFingerprint(hash);
            long[] indices = CalculateBuckets(hash, fingerprint);

            // Check each of the possible buckets for the requested value
            foreach (long index in indices)
            {
                ref var bucket = ref Buckets[index];
                if (bucket.Fingerprint == fingerprint)
                {
                    // The value was found
                    return (long)bucket.Count;
                }
            }

            // The value was not found
            return 0;
        }

        /// <inheritdoc/>
        public IncrementResult TryIncrementAndEstimate(Hash hash, out long estimate)
        {
            ulong fingerprint = CalculateFingerprint(hash);
            long[] indices = CalculateBuckets(hash, fingerprint);

            // Check each of the possible buckets for the requested value
            foreach (long index in indices)
            {
                ref var bucket = ref Buckets[index];
                if (bucket.Fingerprint == fingerprint)
                {
                    // The value was found
                    if (bucket.Count == bucket.MaxCount)
                    {
                        estimate = (long)bucket.Count + 1;
                        return IncrementResult.Overflow;
                    }
                    else
                    {
                        estimate = (long)(++bucket.Count);
                        return IncrementResult.Success;
                    }
                }
            }

            // If we get here, the value was not found. Attempt to insert a new entry with count of 1.
            var newEntry = new TBucket { Fingerprint = fingerprint, Count = 1 };
            estimate = 1;

            // If we get here, the value was not found. Check whether any of the possible buckets are empty, and if so,
            // insert the value into an empty bucket.
            foreach (long index in indices)
            {
                ref var bucket = ref Buckets[index];
                if (bucket.IsEmpty)
                {
                    bucket = newEntry;
                    return IncrementResult.Success;
                }
            }

            // If we get here, all possible buckets are full. Attempt to make an empty bucket by swapping one of the
            // existing values to one of its alternate locations.
            foreach (long index in indices)
            {
                if (RecursiveSwap(index, MaxRecursions))
                {
                    Buckets[index] = newEntry;
                    return IncrementResult.Success;
                }
            }

            return IncrementResult.NoCapacity;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Array.Clear(Buckets, 0, Buckets.Length);
        }

        /// <summary>
        /// Calculates the fingerprint of an input value
        /// </summary>
        /// <param name="hash">128-bit hash of the input value</param>
        /// <returns>Fingerprint</returns>
        /// <remarks>
        /// The result is basically <see cref="Hash.Hash2"/>, except truncated to the size supported by
        /// <typeparamref name="TBucket"/>.
        /// </remarks>
        private ulong CalculateFingerprint(Hash hash)
        {
            unchecked
            {
                ulong maxFingerprint = new TBucket().MaxFingerprint;
                return (ulong)hash.Hash2 % maxFingerprint;
            }
        }

        /// <summary>
        /// Calculates the possible buckets for an input value
        /// </summary>
        /// <param name="hash">128-bit hash of the input value</param>
        /// <param name="fingerprint">
        /// Fingerprint of the bucket's value, calculated using <see cref="CalculateFingerprint(Hash)"/>
        /// </param>
        /// <returns>Array of possible bucket indices</returns>
        protected abstract long[] CalculateBuckets(Hash hash, ulong fingerprint);

        /// <summary>
        /// Calculates the alternate buckets for an existing value
        /// </summary>
        /// <param name="index">Index of the bucket</param>
        /// <param name="fingerprint">Fingerprint of the bucket's value</param>
        /// <returns>Array of possible alternate bucket indices</returns>
        protected abstract long[] CalculateAlternateBuckets(long index, ulong fingerprint);

        /// <summary>
        /// Calculates the maximum number of recursions when swapping values to make an empty bucket. As described in
        /// various papers on cuckoo filters, this value should be approximately log(bucketCount). Rather than wasting
        /// memory to store it, we quickly calculate it on demand using a FindFirstSet operation.
        /// </summary>
        protected abstract int MaxRecursions { get; }

        /// <summary>
        /// Attempt to make a bucket empty by swapping its current value to its alternate bucket(s). If all alternate
        /// buckets are full, this method recursively attempts to swap those elements.
        /// </summary>
        /// <param name="index">Index of the bucket to make empty</param>
        /// <param name="recursionsRemaining">
        /// Number of recursions remaining. On the initial method call, this parameter should be initialized to
        /// <see cref="MaxRecursions"/>.
        /// </param>
        /// <returns>
        /// True if the bucket is now empty. False if the maximum number of recursions were reached and no empty bucket
        /// was found.
        /// </returns>
        private bool RecursiveSwap(long index, int recursionsRemaining)
        {
            if (recursionsRemaining < 0)
            {
                return false;
            }

            long[] indices = CalculateAlternateBuckets(index, Buckets[index].Fingerprint);

            // Check if any of the alternate locations are empty
            foreach (long alternateIndex in indices)
            {
                if (Buckets[alternateIndex].IsEmpty)
                {
                    Swap(index, alternateIndex);
                    return true;
                }
            }

            // Try recursively swapping alternate locations
            foreach (long alternateIndex in indices)
            {
                if (RecursiveSwap(alternateIndex, recursionsRemaining - 1))
                {
                    Swap(index, alternateIndex);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Swaps two buckets
        /// </summary>
        /// <param name="index1">Index of the first bucket</param>
        /// <param name="index2">Index of the second bucket</param>
        private void Swap(long index1, long index2)
        {
            var temp = Buckets[index1];
            Buckets[index1] = Buckets[index2];
            Buckets[index2] = temp;
        }
    }
}
