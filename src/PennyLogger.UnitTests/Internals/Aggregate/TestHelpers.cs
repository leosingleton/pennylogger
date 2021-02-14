// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.UnitTests;
using PennyLogger.Internals.Reflection;
using System;
using System.Collections.Generic;
using Xunit;

namespace PennyLogger.Internals.Aggregate.UnitTests
{
    /// <summary>
    /// Helper functions for unit testing classes derived from <see cref="AggregateEnumerableProperty{T}"/>
    /// </summary>
    /// <remarks>
    /// Ideally, we would follow the same pattern as <see cref="FrequencyEstimatorTestBase{T}"/>, however it wouldn't
    /// work with internal classes, since Xunit test classes must be public. Instead, this helper class contains as
    /// much shared code as possible to avoid test duplication.
    /// </remarks>
    static class TestHelpers
    {
        public static void BuildConstructorParameters(out PropertyReflector<string> prop, out PennyPropertyConfig config)
        {
            var obj = new { Value = "" };
            var memberInfo = obj.GetType().GetProperty("Value");
            prop = new PropertyReflectorString(memberInfo);
            config = PennyPropertyConfig.Create(null, null, null, null, typeof(string));
        }

        public static void IncrementValue(AggregateEnumerableProperty<string> agg, string value, int count)
        {
            var obj = new { Value = value };
            for (int n = 0; n < count; n++)
            {
                agg.Add(obj, false);
            }
        }

        public static void IncrementRandomValues(AggregateEnumerableProperty<string> agg, int values)
        {
            for (int n = 0; n < values; n++)
            {
                var obj = new { Value = Guid.NewGuid().ToString() };
                agg.Add(obj, false);
            }
        }

        public static void AssertContainsTopValue(AggregateEnumerableProperty<string> agg, string value, int count)
        {
            var counts = agg.GetSortedCounts();
            var expected = new KeyValuePair<string, long>(value, count);
            Assert.Contains(expected, counts);
        }
    }
}
