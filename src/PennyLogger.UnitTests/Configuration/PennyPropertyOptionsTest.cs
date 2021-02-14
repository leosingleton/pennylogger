// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Xunit;

namespace PennyLogger.Configuration.UnitTests
{
    /// <summary>
    /// Test the <see cref="PennyPropertyOptions"/> class
    /// </summary>
    public class PennyPropertyOptionsTest
    {
        /// <summary>
        /// Compares two options instances with equal values
        /// </summary>
        [Fact]
        public void EqualOptions()
        {
            var prop1 = new PennyPropertyOptions
            {
                Name = "Prop",
                IgnoreNull = true,
                IgnoreEmpty = false,
                Enumerable = new PennyPropertyEnumerableOptions { Top = 10, Approximate = false }
            };

            var prop2 = new PennyPropertyOptions
            {
                Name = "Prop",
                IgnoreNull = true,
                IgnoreEmpty = false,
                Enumerable = new PennyPropertyEnumerableOptions { Top = 10, Approximate = false }
            };

            Assert.Equal(prop1, prop1);
            Assert.Equal(prop1, prop2);

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(prop1 == prop1);
            Assert.False(prop1 != prop2);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.True(prop1 == prop2);
            Assert.False(prop1 != prop2);

            Assert.Equal(prop1.GetHashCode(), prop2.GetHashCode());
        }

        /// <summary>
        /// Compares two options instances with not equal values
        /// </summary>
        [Fact]
        public void NotEqualOptions()
        {
            var prop1 = new PennyPropertyOptions
            {
                Name = "Prop",
                IgnoreNull = true,
                IgnoreEmpty = false,
                Enumerable = new PennyPropertyEnumerableOptions { Top = 10, Approximate = false }
            };

            var prop2 = new PennyPropertyOptions
            {
                Name = "Prop",
                IgnoreNull = true,
                IgnoreEmpty = false,
                Enumerable = new PennyPropertyEnumerableOptions { Top = 11, Approximate = false }
            };

            Assert.NotEqual(prop1, prop2);

            Assert.False(prop1 == prop2);
            Assert.True(prop1 != prop2);

            Assert.NotEqual(prop1.GetHashCode(), prop2.GetHashCode());
        }
    }
}
