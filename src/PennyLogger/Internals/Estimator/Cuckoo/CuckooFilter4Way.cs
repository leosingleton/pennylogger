// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger.Internals.Estimator.Cuckoo
{
    /// <inheritdoc/>
    public class CuckooFilter4Way<TBucket> : CuckooFilter<TBucket>
        where TBucket : ICuckooBucket, new()
    {
        /// <inheritdoc/>
        public CuckooFilter4Way(long size) : base(size)
        {
        }

        /// <inheritdoc/>
        protected override long[] CalculateBuckets(Hash hash, ulong fingerprint)
        {
            unchecked
            {
                long bucket1 = (long)((ulong)hash.Hash1 % (ulong)Buckets.LongLength);
                long[] alternates = CalculateAlternateBuckets(bucket1, fingerprint);
                return new long[] { bucket1, alternates[0], alternates[1], alternates[2] };
            }
        }

        /// <inheritdoc/>
        protected override long[] CalculateAlternateBuckets(long index, ulong fingerprint)
        {
            // Compute two 64-bit hashes of the fingerprint
            Hash hash = Hash.Create(fingerprint);
            return new long[3]
            {
                (long)((ulong)(index ^ hash.Hash1) % (ulong)Buckets.LongLength),
                (long)((ulong)(index ^ hash.Hash2) % (ulong)Buckets.LongLength),
                (long)((ulong)(index ^ hash.Hash1 ^ hash.Hash2) % (ulong)Buckets.LongLength)
            };
        }

        /// <inheritdoc/>
        protected override int MaxRecursions => BitTwiddle.FindFirstSet((ulong)Buckets.LongLength) - 1;
    }
}
