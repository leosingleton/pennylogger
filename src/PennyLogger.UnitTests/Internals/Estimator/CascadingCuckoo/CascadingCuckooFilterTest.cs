// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.UnitTests;

namespace PennyLogger.Internals.Estimator.CascadingCuckoo.UnitTests
{
    /// <summary>
    /// Test <see cref="CascadingCuckooFilter2Way"/>
    /// </summary>
    public class CascadingCuckooFilter2WayTest : FrequencyEstimatorTestBase<IFrequencyEstimator>
    {
        /// <inheritdoc/>
        protected override IFrequencyEstimator Create() => new CascadingCuckooFilter2Way { MaxBytes = 16384 };

        // The cascading and scaling support allows the memory to go somewhat over MaxBytes, so the actual capacity is
        // slightly over 16K values in this test case, instead of the usual 8-16K for a standard 8-bit Cuckoo filter.
        
        /// <inheritdoc/>
        protected override int MinValuesBeforeCapacity => 8192;

        /// <inheritdoc/>
        protected override int MaxValuesBeforeCapacity => 32768;
    }

    /// <summary>
    /// Test <see cref="CascadingCuckooFilter4Way"/>
    /// </summary>
    public class CascadingCuckooFilter4WayTest : FrequencyEstimatorTestBase<IFrequencyEstimator>
    {
        protected override IFrequencyEstimator Create() => new CascadingCuckooFilter4Way() { MaxBytes = 16384 };

        // The cascading and scaling support allows the memory to go somewhat over MaxBytes, so the actual capacity is
        // slightly over 16K values in this test case, instead of the usual 8-16K for a standard 8-bit Cuckoo filter.

        /// <inheritdoc/>
        protected override int MinValuesBeforeCapacity => 8192;

        /// <inheritdoc/>
        protected override int MaxValuesBeforeCapacity => 32768;
    }
}
