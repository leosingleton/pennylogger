// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.Cuckoo;

namespace PennyLogger.Internals.Estimator.CascadingCuckoo
{
    /// <summary>
    /// Cuckoo filter that supports cascading (using larger filters as a value's count increases), scaling (adding
    /// filters as the load factor increases), and counting. This implementation uses 2-way cuckoo filters, meaning
    /// each value can map to one of 2 possible buckets. It is faster but offers poorer memory consumption compared to
    /// <see cref="CascadingCuckooFilter4Way"/>.
    /// </summary>
    public class CascadingCuckooFilter2Way : CascadingEstimator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CascadingCuckooFilter2Way() : base(
            new ScalableEstimator(size => new CuckooFilter2Way<CuckooBucket8>(size)),
            new ScalableEstimator(size => new CuckooFilter2Way<CuckooBucket16Counting>(size)),
            new ScalableEstimator(size => new CuckooFilter2Way<CuckooBucket64Counting>(size)))
        {
        }
    }
}
