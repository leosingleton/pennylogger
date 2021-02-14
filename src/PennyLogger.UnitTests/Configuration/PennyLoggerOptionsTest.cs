// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace PennyLogger.Configuration.UnitTests
{
    /// <summary>
    /// Test the <see cref="PennyLoggerOptions"/> class
    /// </summary>
    public class PennyLoggerOptionsTest
    {
        /// <summary>
        /// Compares two options instances with equal values
        /// </summary>
        [Fact]
        public void EqualOptions()
        {
            var options1 = new PennyLoggerOptions
            {
                Events = new Dictionary<string, PennyEventOptions>
                {
                    { "Event1", new PennyEventOptions { Enabled = false } },
                    { "Event2", new PennyEventOptions { Id = "Event" } }
                }
            };

            var options2 = new PennyLoggerOptions
            {
                Events = new Dictionary<string, PennyEventOptions>
                {
                    { "Event1", new PennyEventOptions { Enabled = false } },
                    { "Event2", new PennyEventOptions { Id = "Event" } }
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
            var options1 = new PennyLoggerOptions
            {
                Events = new Dictionary<string, PennyEventOptions>
                {
                    { "Event1", new PennyEventOptions { Enabled = false } },
                    { "Event2", new PennyEventOptions { Id = "Event" } }
                }
            };

            var options2 = new PennyLoggerOptions
            {
                Events = new Dictionary<string, PennyEventOptions>
                {
                    { "Event1", new PennyEventOptions { Enabled = false } },
                    { "Event2", new PennyEventOptions { } }
                }
            };

            Assert.NotEqual(options1, options2);

            Assert.False(options1 == options2);
            Assert.True(options1 != options2);

            Assert.NotEqual(options1.GetHashCode(), options2.GetHashCode());
        }
    }
}
