// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.Cuckoo;

namespace PennyLogger.Internals.Estimator.CascadingCuckoo
{
    /// <summary>
    /// Cuckoo filter that supports cascading (using larger filters as a value's count increases), scaling (adding
    /// filters as the load factor increases), and counting. This implementation uses 4-way cuckoo filters, meaning
    /// each value can map to one of 4 possible buckets. It is slower but offers improved memory consumption compared to
    /// <see cref="CascadingCuckooFilter2Way"/>.
    /// </summary>
    public class CascadingCuckooFilter4Way : CascadingEstimator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CascadingCuckooFilter4Way() : base(
            new ScalableEstimator(size => new CuckooFilter4Way<CuckooBucket8>(size)),
            new ScalableEstimator(size => new CuckooFilter4Way<CuckooBucket16Counting>(size)),
            new ScalableEstimator(size => new CuckooFilter4Way<CuckooBucket64Counting>(size)))
        {
        }
    }
}
