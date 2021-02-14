// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.UnitTests;

namespace PennyLogger.Internals.Estimator.CountMin.UnitTests
{
    /// <summary>
    /// Test <see cref="CountMeanMinSketch"/>
    /// </summary>
    public class CountMeanMinSketchTest : FrequencyEstimatorTestBase<CountMeanMinSketch>
    {
        /// <inheritdoc/>
        protected override CountMeanMinSketch Create() => new CountMeanMinSketch(0.001, 0.001);

        // CountMeanMinSketch never returns NoCapacity, as there is no limit to the number of collisions. Disable the
        // capacity test.

        /// <inheritdoc/>
        protected override int MinValuesBeforeCapacity => 0;

        /// <inheritdoc/>
        protected override int MaxValuesBeforeCapacity => 0;
    }
}
