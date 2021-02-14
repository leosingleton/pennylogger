// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.UnitTests;

namespace PennyLogger.Internals.Estimator.Cuckoo.UnitTests
{
    public class CuckooFilter2WayTest8 : FrequencyEstimatorTestBase<CuckooFilter2Way<CuckooBucket8>>
    {
        protected override CuckooFilter2Way<CuckooBucket8> Create() =>
            new CuckooFilter2Way<CuckooBucket8>(128);

        protected override int MinValuesBeforeCapacity => 64;
        protected override int MaxValuesBeforeCapacity => 129;
    }

    public class CuckooFilter2WayTest16Counting : FrequencyEstimatorTestBase<CuckooFilter2Way<CuckooBucket16Counting>>
    {
        protected override CuckooFilter2Way<CuckooBucket16Counting> Create() =>
            new CuckooFilter2Way<CuckooBucket16Counting>(128 * 2);

        protected override int MinValuesBeforeCapacity => 64;
        protected override int MaxValuesBeforeCapacity => 129;
    }

    public class CuckooFilter2WayTest64Counting : FrequencyEstimatorTestBase<CuckooFilter2Way<CuckooBucket64Counting>>
    {
        protected override CuckooFilter2Way<CuckooBucket64Counting> Create() =>
            new CuckooFilter2Way<CuckooBucket64Counting>(128 * 8);

        protected override int MinValuesBeforeCapacity => 64;
        protected override int MaxValuesBeforeCapacity => 129;
    }
}
