// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Xunit;

namespace PennyLogger.Internals.Aggregate.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="AggregateEnumerablePropertyLossless{T}"/>
    /// </summary>
    public class AggregateEnumerablePropertyLosslessTest
    {
        private AggregateEnumerablePropertyLossless<string> Create()
        {
            TestHelpers.BuildConstructorParameters(out var prop, out var config);
            return new AggregateEnumerablePropertyLossless<string>(prop, config);
        }

        /// <summary>
        /// Inserts one value and ensures it is contained in the Top-N
        /// </summary>
        [Fact]
        public void InsertOneValue()
        {
            var agg = Create();
            TestHelpers.IncrementValue(agg, "Test", 1);
            TestHelpers.AssertContainsTopValue(agg, "Test", 1);
        }

        /// <summary>
        /// Inserts one large value and ensures it is contained in the Top-N even after many small values
        /// </summary>
        [Fact]
        public void LargeThenSmall()
        {
            var agg = Create();
            TestHelpers.IncrementValue(agg, "Test", 100);
            TestHelpers.IncrementRandomValues(agg, 100);
            TestHelpers.AssertContainsTopValue(agg, "Test", 100);
        }

        /// <summary>
        /// Inserts many small values, then ensures one large value is contained in the Top-N
        /// </summary>
        [Fact]
        public void SmallThenLarge()
        {
            var agg = Create();
            TestHelpers.IncrementRandomValues(agg, 100);
            TestHelpers.IncrementValue(agg, "Test", 100);
            TestHelpers.AssertContainsTopValue(agg, "Test", 100);
        }
    }
}
