// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals;
using Xunit;

namespace PennyLogger.UnitTests
{
    public partial class PennyLoggerTest
    {
        /// <summary>
        /// Tests a sampler created via anonymous object and ID specified in the properties
        /// </summary>
        [Fact]
        public void SamplerWithAnonymousObject()
        {
            Init(out var mock, out var logger);

            var sampler = logger.Sample(new
            {
                Event = "SamplerWithAnonymousObject",
                Value = 12
            });

            var s = (SamplerState)sampler;
            s.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"SamplerWithAnonymousObject\",\"Value\":12}", mock.LogHistory[0]);

            sampler.Dispose();
        }

        /// <summary>
        /// Tests a sampler created via anonymous object and ID specified in the options
        /// </summary>
        [Fact]
        public void SamplerWithAnonymousObjectAndIdOptions()
        {
            Init(out var mock, out var logger);

            var sampler = logger.Sample(
                new { Value = 12 },
                new PennySamplerOptions { Id = "SamplerWithAnonymousObjectAndIdOptions" });

            var s = (SamplerState)sampler;
            s.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"SamplerWithAnonymousObjectAndIdOptions\",\"Value\":12}", mock.LogHistory[0]);

            sampler.Dispose();
        }

        /// <summary>
        /// Tests a sampler created via class
        /// </summary>
        [Fact]
        public void SamplerWithClass()
        {
            Init(out var mock, out var logger);

            var sampler = logger.Sample(new MockSampler
            {
                String = "Test",
                Value = 42
            });

            var s = (SamplerState)sampler;
            s.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"Mock\",\"String\":\"Test\",\"Value\":42}", mock.LogHistory[0]);

            sampler.Dispose();
        }

        private class MockSampler
        {
            public string String { get; set; }
            public int Value;
        }

        /// <summary>
        /// Tests a sampler created via class and ID specified in the attributes
        /// </summary>
        [Fact]
        public void SamplerWithClassAndIdAttribute()
        {
            Init(out var mock, out var logger);

            var sampler = logger.Sample(new MySampler
            {
                Value = 42
            });

            var s = (SamplerState)sampler;
            s.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"Mock\",\"Value\":42}", mock.LogHistory[0]);

            sampler.Dispose();
        }

        [PennySampler(Id = "Mock")]
        private class MySampler
        {
            public int Value { get; set; }
        }

        /// <summary>
        /// Tests a sampler created via lambda function
        /// </summary>
        [Fact]
        public void SamplerWithLambda()
        {
            Init(out var mock, out var logger);

            int count = 0;
            var sampler = logger.Sample(() => new
            {
                Event = "SamplerWithLambda",
                Value = ++count
            });

            var s = (SamplerState)sampler;
            s.Flush();
            s.Flush();

            // Note that the lambda actually gets evaluated once to discover the event name, so Value actually starts
            // at 2 here...
            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal("{\"Event\":\"SamplerWithLambda\",\"Value\":2}", mock.LogHistory[0]);
            Assert.Equal("{\"Event\":\"SamplerWithLambda\",\"Value\":3}", mock.LogHistory[1]);

            sampler.Dispose();
        }
    }
}
