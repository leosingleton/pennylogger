// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals;
using PennyLogger.Mocks.UnitTests;
using System.Collections.Generic;
using Xunit;

namespace PennyLogger.UnitTests
{
    public partial class PennyLoggerTest
    {
        /// <summary>
        /// Enables raw logging of an event via configuration
        /// </summary>
        [Fact]
        public void RawViaConfig()
        {
            Init(out var mock, out var logger, out var options);

            options.CurrentValue = new PennyLoggerOptions
            {
                Events = new Dictionary<string, PennyEventOptions>
                {
                    { "RawViaConfig", new PennyEventOptions { RawLogging = new PennyEventRawLoggingOptions() } }
                }
            };

            logger.Event(new
            {
                Event = "RawViaConfig"
            });

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"RawViaConfig\"}", mock.LogHistory[0]);
        }

        /// <summary>
        /// Enables raw logging of an event via dynamic configuration
        /// </summary>
        [Fact]
        public void RawViaDynamicConfig()
        {
            Init(out var mock, out var logger, out var options);

            // Log first with default options. This will log one aggregate event, which gets flushed automatically on
            // the configuration change below.
            logger.Event(new
            {
                Event = "RawViaDynamicConfig"
            });

            // Change the event to raw logging.
            options.CurrentValue = new PennyLoggerOptions
            {
                Events = new Dictionary<string, PennyEventOptions>
                {
                    { "RawViaDynamicConfig", new PennyEventOptions { RawLogging = new PennyEventRawLoggingOptions() } }
                }
            };
            logger.Event(new
            {
                Event = "RawViaDynamicConfig"
            });

            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal("{\"Event\":\"RawViaDynamicConfig$\",\"Count\":1}", mock.LogHistory[0]);
            Assert.Equal("{\"Event\":\"RawViaDynamicConfig\"}", mock.LogHistory[1]);
        }

        /// <summary>
        /// Disable an event via dynamic configuration
        /// </summary>
        [Fact]
        public void DisableEventViaDynamicConfig()
        {
            Init(out var mock, out var logger, out var options);

            // Log one event with default settings
            var ev = new MockEmptyEvent();
            logger.Event(ev);

            Assert.Single(mock.LogHistory);
            Assert.Equal(ev.GetExpectedJson(), mock.LogHistory[0]);

            // Disable the event
            options.CurrentValue = new PennyLoggerOptions
            {
                Events = new Dictionary<string, PennyEventOptions>
                {
                    { "MockEmpty", new PennyEventOptions { Enabled = false } }
                }
            };

            // Further calls result in no output
            logger.Event(new MockEmptyEvent());
            logger.Event(new MockEmptyEvent());

            Assert.Single(mock.LogHistory);
        }

        /// <summary>
        /// Disable a sampler via dynamic configuration
        /// </summary>
        [Fact]
        public void DisableSamplerViaDynamicConfig()
        {
            Init(out var mock, out var logger, out var options);

            var sampler = logger.Sample(new
            {
                Event = "Mock",
                Value = 42
            });

            var s = (SamplerState)sampler;
            s.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"Mock\",\"Value\":42}", mock.LogHistory[0]);

            // Disable the sampler
            options.CurrentValue = new PennyLoggerOptions
            {
                Samplers = new Dictionary<string, PennySamplerOptions>
                {
                    { "Mock", new PennySamplerOptions { Enabled = false } }
                }
            };

            // Further calls result in no output
            s.Flush();
            s.Flush();

            Assert.Single(mock.LogHistory);

            sampler.Dispose();
        }
    }
}
