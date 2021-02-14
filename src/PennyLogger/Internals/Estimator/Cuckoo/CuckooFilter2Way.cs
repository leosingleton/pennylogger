// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator.Cuckoo
{
    /// <inheritdoc/>
    public class CuckooFilter2Way<TBucket> : CuckooFilter<TBucket>
        where TBucket : ICuckooBucket, new()
    {
        /// <inheritdoc/>
        public CuckooFilter2Way(long size) : base(size)
        {
        }

        /// <inheritdoc/>
        protected override long[] CalculateBuckets(Hash hash, ulong fingerprint)
        {
            unchecked
            {
                long bucket1 = (long)((ulong)hash.Hash1 % (ulong)Buckets.LongLength);
                long bucket2 = CalculateAlternateBucket(bucket1, fingerprint);
                return new long[] { bucket1, bucket2 };
            }
        }

        /// <inheritdoc/>
        protected override long[] CalculateAlternateBuckets(long index, ulong fingerprint) =>
            new long[] { CalculateAlternateBucket(index, fingerprint) };

        private long CalculateAlternateBucket(long index, ulong fingerprint)
        {
            unchecked
            {
                // Compute a hash of the fingerprint. We want a 64-bit fingerprint, so use Hash instead of GetHashCode().
                Hash hash = Hash.Create(fingerprint);
                return (long)((ulong)(index ^ hash.Hash1) % (ulong)Buckets.LongLength);
            }
        }

        /// <inheritdoc/>
        protected override int MaxRecursions => BitTwiddle.FindFirstSet((ulong)Buckets.LongLength);
    }
}
