// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;

namespace PennyLogger.Configuration.UnitTests
{
    /// <summary>
    /// Test the <see cref="PennyEventOptions"/> class
    /// </summary>
    public class PennyEventOptionsTest
    {
        /// <summary>
        /// Compares two options instances with equal values
        /// </summary>
        [Fact]
        public void EqualOptions()
        {
            var options1 = new PennyEventOptions
            {
                Id = "Event",
                AggregateLogging = new PennyEventAggregateLoggingOptions { Interval = 30, Level = LogLevel.Debug,
                    LogIfZero = true },
                RawLogging = new PennyEventRawLoggingOptions { Interval = 60, Level = LogLevel.Critical, Max = 100 },
                Properties = new Dictionary<string, PennyPropertyOptions>
                {
                    { "Prop1", new PennyPropertyOptions { Enabled = true, MaxLength = 1024} },
                    { "Prop2", new PennyPropertyOptions { Name = "Prop2A", IgnoreNull = false, IgnoreEmpty = false } }
                }
            };

            var options2 = new PennyEventOptions
            {
                Id = "Event",
                AggregateLogging = new PennyEventAggregateLoggingOptions { Interval = 30, Level = LogLevel.Debug,
                    LogIfZero = true },
                RawLogging = new PennyEventRawLoggingOptions { Interval = 60, Level = LogLevel.Critical, Max = 100 },
                Properties = new Dictionary<string, PennyPropertyOptions>
                {
                    { "Prop1", new PennyPropertyOptions { Enabled = true, MaxLength = 1024} },
                    { "Prop2", new PennyPropertyOptions { Name = "Prop2A", IgnoreNull = false, IgnoreEmpty = false } }
                }
            };

            Assert.Equal(options1, options1);
            Assert.Equal(options1, options2);

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(options1 == options1);
            Assert.False(options1 != options1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.True(options1 == options2);
            Assert.False(options1 != options2);

            Assert.Equal(options1.GetHashCode(), options2.GetHashCode());
        }

        /// <summary>
        /// Compares two options instances with not equal values
        /// </summary>
        [Fact]
        public void NotEqualOptions()
        {
            var options1 = new PennyEventOptions
            {
                Id = "Event",
                AggregateLogging = new PennyEventAggregateLoggingOptions { Interval = 30, Level = LogLevel.Debug,
                    LogIfZero = true },
                RawLogging = new PennyEventRawLoggingOptions { Interval = 60, Level = LogLevel.Critical, Max = 100 },
                Properties = new Dictionary<string, PennyPropertyOptions>
                {
                    { "Prop1", new PennyPropertyOptions { Enabled = true, MaxLength = 1024} },
                    { "Prop2", new PennyPropertyOptions { Name = "Prop2A", IgnoreNull = false, IgnoreEmpty = true } }
                }
            };

            var options2 = new PennyEventOptions
            {
                Id = "Event",
                AggregateLogging = new PennyEventAggregateLoggingOptions { Interval = 30, Level = LogLevel.Debug,
                    LogIfZero = true },
                RawLogging = new PennyEventRawLoggingOptions { Interval = 60, Level = LogLevel.Critical, Max = 100 },
                Properties = new Dictionary<string, PennyPropertyOptions>
                {
                    { "Prop1", new PennyPropertyOptions { Enabled = true, MaxLength = 1024} },
                    { "Prop2", new PennyPropertyOptions { Name = "Prop2A", IgnoreNull = false, IgnoreEmpty = false } }
                }
            };

            Assert.NotEqual(options1, options2);

            Assert.False(options1 == options2);
            Assert.True(options1 != options2);

            Assert.NotEqual(options1.GetHashCode(), options2.GetHashCode());
        }
    }
}
