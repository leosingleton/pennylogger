// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Xunit;

namespace PennyLogger.UnitTests
{
    public partial class PennyLoggerTest
    {
        /// <summary>
        /// Tests <see cref="PennyEventAggregateLoggingConfig.LogIfZero"/> false (default)
        /// </summary>
        [Fact]
        public void LogIfZeroDefault()
        {
            Init(out var mock, out var logger);

            // Log one event to register the aggregate event
            logger.Event(new { Event = "LogIfZeroDefault", String = "Test", Value = 20 });
            logger.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal(
                "{\"Event\":\"LogIfZeroDefault$\",\"Count\":1,\"String\":{\"Test\":1},\"Value\":" +
                "{\"Min\":20,\"Max\":20,\"Sum\":20}}",
                mock.LogHistory[0]);
            
            // Later calls to Flush() do not log any events, as there were zero new events
            logger.Flush();
            Assert.Single(mock.LogHistory);
        }

        /// <summary>
        /// Tests <see cref="PennyEventAggregateLoggingConfig.LogIfZero"/> true
        /// </summary>
        [Fact]
        public void LogIfZeroTrue()
        {
            Init(out var mock, out var logger);

            // Configuration the event ID to log every interval, even if zero events
            var options = new PennyEventOptions
            {
                AggregateLogging = new PennyEventAggregateLoggingOptions { LogIfZero = true }
            };

            // Log one event to register the aggregate event
            logger.Event(new { Event = "LogIfZeroTrue", String = "Test", Value = 20 }, options);
            logger.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal(
                "{\"Event\":\"LogIfZeroTrue$\",\"Count\":1,\"String\":{\"Test\":1},\"Value\":" +
                "{\"Min\":20,\"Max\":20,\"Sum\":20}}",
                mock.LogHistory[0]);

            // Later calls to Flush() do not log any events, as there were zero new events
            logger.Flush();
            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal(
                "{\"Event\":\"LogIfZeroTrue$\",\"Count\":0,\"String\":{},\"Value\":{\"Min\":0,\"Max\":0,\"Sum\":0}}",
                mock.LogHistory[1]);
        }
    }
}
