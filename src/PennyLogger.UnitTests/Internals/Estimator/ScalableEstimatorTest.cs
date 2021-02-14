// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.UnitTests;

namespace PennyLogger.Internals.Estimator.Cuckoo.UnitTests
{
    public class ScalableEstimatorTest : FrequencyEstimatorTestBase<ScalableEstimator>
    {
        // Start with 64 bytes, then double the previous size
        protected override ScalableEstimator Create() => new ScalableEstimator(
            metrics => new CuckooFilter2Way<CuckooBucket16Counting>((
                metrics.PreviousEstimator?.TotalBytes ?? 32) * 2)) { MaxBytes = 1024 };

        protected override int MinValuesBeforeCapacity => 384;
        protected override int MaxValuesBeforeCapacity => 513;
    }
}
