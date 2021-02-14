// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Xunit;

namespace PennyLogger.UnitTests
{
    public partial class PennyLoggerTest
    {
        /// <summary>
        /// Tests an empty anonymous event with EventID specified via parameters
        /// </summary>
        [Fact]
        public void EmptyAnonymousEvent()
        {
            Init(out var mock, out var logger);

            var options = new PennyEventOptions
            {
                Id = "EmptyAnonymous",
                RawLogging = new PennyEventRawLoggingOptions()
            };
            logger.Event(new object(), options);

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"EmptyAnonymous\"}", mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests an anonymous event with EventID specified via properties
        /// </summary>
        [Fact]
        public void AnonymousEvent()
        {
            Init(out var mock, out var logger);

            var options = new PennyEventOptions
            {
                RawLogging = new PennyEventRawLoggingOptions()
            };
            logger.Event(new { Event = "Anonymous" }, options);

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"Anonymous\"}", mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests an empty anonymous event with EventID specified via parameters
        /// </summary>
        [Fact]
        public void EmptyAnonymousEvent2()
        {
            Init(out var mock, out var logger);

            var options = new PennyEventOptions
            {
                Id = "EmptyAnonymous",
                RawLogging = new PennyEventRawLoggingOptions()
            };
            logger.Event(new object(), options);
            logger.Event(new object(), options);

            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal("{\"Event\":\"EmptyAnonymous\"}", mock.LogHistory[0]);
            Assert.Equal("{\"Event\":\"EmptyAnonymous\"}", mock.LogHistory[1]);
        }

        /// <summary>
        /// Tests an empty anonymous event with different EventIDs specified via parameters
        /// </summary>
        [Fact]
        public void ParamsDifferentIds()
        {
            Init(out var mock, out var logger);

            var options1 = new PennyEventOptions
            {
                Id = "ParamsDifferentIds1",
                RawLogging = new PennyEventRawLoggingOptions()
            };
            logger.Event(new object(), options1);
            var options2 = new PennyEventOptions
            {
                Id = "ParamsDifferentIds2",
                RawLogging = new PennyEventRawLoggingOptions()
            };
            logger.Event(new object(), options2);

            // Note that the ID property is only evaluated on the first call, and the ID may not be changed on
            // subsequent calls to Log().
            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal("{\"Event\":\"ParamsDifferentIds1\"}", mock.LogHistory[0]);
            Assert.Equal("{\"Event\":\"ParamsDifferentIds1\"}", mock.LogHistory[1]);
        }

        /// <summary>
        /// Tests changing the logging type via parameters
        /// </summary>
        [Fact]
        public void ParamsDifferentLogging()
        {
            Init(out var mock, out var logger);

            var options1 = new PennyEventOptions
            {
                Id = "ParamsDifferentLogging",
                RawLogging = new PennyEventRawLoggingOptions()
            };
            logger.Event(new object(), options1);
            var options2 = new PennyEventOptions
            {
                Id = "ParamsDifferentLogging",
                AggregateLogging = new PennyEventAggregateLoggingOptions()
            };
            logger.Event(new object(), options2);
            logger.Flush();

            // We expect one raw log event, followed by one aggregate log event
            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal("{\"Event\":\"ParamsDifferentLogging\"}", mock.LogHistory[0]);
            Assert.Equal("{\"Event\":\"ParamsDifferentLogging$\",\"Count\":1}", mock.LogHistory[1]);
        }

        /// <summary>
        /// Tests an anonymous event with EventID specified via properties, logging aggregated output
        /// </summary>
        [Fact]
        public void AnonymousEventWithAggregation()
        {
            Init(out var mock, out var logger);

            logger.Event(new { Event = "AnonymousWithAgg" });
            logger.Event(new { Event = "AnonymousWithAgg" });
            logger.Event(new { Event = "AnonymousWithAgg" });
            logger.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal("{\"Event\":\"AnonymousWithAgg$\",\"Count\":3}", mock.LogHistory[0]);
        }
    }
}
