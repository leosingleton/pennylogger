// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.UnitTests;

namespace PennyLogger.Internals.Estimator.Cuckoo.UnitTests
{
    public class CascadingEstimatorTest : FrequencyEstimatorTestBase<CascadingEstimator>
    {
        protected override CascadingEstimator Create() => new CascadingEstimator(new IFrequencyEstimator[]
        {
            new CuckooFilter2Way<CuckooBucket8>(128),
            new CuckooFilter2Way<CuckooBucket16Counting>(128)
        });

        protected override int MinValuesBeforeCapacity => 64;
        protected override int MaxValuesBeforeCapacity => 129;
    }
}
