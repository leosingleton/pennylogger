// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Mocks.UnitTests;
using System;
using System.Collections.Generic;
using Xunit;

namespace PennyLogger.UnitTests
{
    public partial class PennyLoggerTest
    {
        /// <summary>
        /// Tests an empty event with ID derived from the class name
        /// </summary>
        [Fact]
        public void EmptyEvent()
        {
            Init(out var mock, out var logger);

            var ev = new MockEmptyEvent();
            logger.Event(ev);

            Assert.Single(mock.LogHistory);
            Assert.Equal(ev.GetExpectedJson(), mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests an empty event with ID overridden via attributes
        /// </summary>
        [Fact]
        public void IDAttributeEvent()
        {
            Init(out var mock, out var logger);

            var ev = new MockIDAttributeEvent();
            logger.Event(ev);

            Assert.Single(mock.LogHistory);
            Assert.Equal(ev.GetExpectedJson(), mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests an event with a dynamically-specified ID property
        /// </summary>
        [Fact]
        public void DynamicIDEvent()
        {
            Init(out var mock, out var logger);

            var ev = new MockDynamicIDEvent { Event = "MyDynamicIDEvent" };
            logger.Event(ev);

            Assert.Single(mock.LogHistory);
            Assert.Equal(ev.GetExpectedJson(), mock.LogHistory[0]);
        }

        /// <summary>
        /// An event with conflicting different names causes an exception
        /// </summary>
        [Fact]
        public void MultipleMismatchedNames()
        {
            Init(out var mock, out var logger);

            Assert.Throws<ArgumentException>(() => logger.Event(new { Event = "MyEvent" },
                new PennyEventOptions { Id = "MyEventMismatched" }));
        }

        /// <summary>
        /// A single event ID gets throttled according to attributes
        /// </summary>
        [Fact]
        public void ThrottleOneEvent()
        {
            Init(out var mock, out var logger);

            for (int i = 0; i < 100; i++)
            {
                logger.Event(new MockDynamicIDEvent { Event = "ThrottleOne" });
            }

            // The attribute rate limits to 5 per interval
            Assert.Equal(5, mock.LogHistory.Count);
        }

        /// <summary>
        /// Each unique event ID gets its own throttling allocation
        /// </summary>
        [Fact]
        public void ThrottleMultipleEvent()
        {
            Init(out var mock, out var logger);

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    logger.Event(new MockDynamicIDEvent { Event = $"Throttle{j}" });
                }
            }

            // Each of the 10 event IDs gets its own rate limit of 5 per interval
            Assert.Equal(10 * 5, mock.LogHistory.Count);
        }

        /// <summary>
        /// Tests aggregating an event containing an Int32 property
        /// </summary>
        [Fact]
        public void AggregateInt()
        {
            Init(out var mock, out var logger);

            logger.Event(new MockIntEvent { Value = 1 });
            logger.Event(new MockIntEvent { Value = 2 });
            logger.Event(new MockIntEvent { Value = 3 });
            logger.Flush();

            Assert.Single(mock.LogHistory);
            Assert.Equal(MockIntEvent.GetExpectedJson(3, 1, 3, 6), mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests aggregating an event containing an enum property
        /// </summary>
        [Fact]
        public void AggregateEnum()
        {
            Init(out var mock, out var logger);

            logger.Event(new MockEnumEvent { Value = MockEnumValue.Value1 });
            logger.Event(new MockEnumEvent { Value = MockEnumValue.Value2 });
            logger.Event(new MockEnumEvent { Value = MockEnumValue.Value2 });
            logger.Flush();

            var expected = new Dictionary<string, int>
            {
                { "Value2", 2 },
                { "Value1", 1 }
            };

            Assert.Single(mock.LogHistory);
            Assert.Equal(MockEnumEvent.GetExpectedJson(3, expected), mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests aggregating a string using Top-5
        /// </summary>
        [Fact]
        public void AggregateStringTop5()
        {
            Init(out var mock, out var logger);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    logger.Event(new MockStringTop5Event { Value = $"Mock{j}" });
                }
            }
            logger.Flush();

            var expected = new Dictionary<string, int>
            {
                { "Mock0", 10 },
                { "Mock1", 9 },
                { "Mock2", 8 },
                { "Mock3", 7 },
                { "Mock4", 6 },
            };

            Assert.Single(mock.LogHistory);
            Assert.Equal(MockStringTop5Event.GetExpectedJson(55, expected), mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests aggregating a string with null values
        /// </summary>
        [Fact]
        public void AggregateStringWithNulls()
        {
            Init(out var mock, out var logger);

            logger.Event(new MockStringTop5Event { Value = "MockStringWithNulls" });
            logger.Event(new MockStringTop5Event { Value = null });
            logger.Event(new MockStringTop5Event { Value = null });
            logger.Flush();

            var expected = new Dictionary<string, int>
            {
                { "(null)", 2 },
                { "MockStringWithNulls", 1 }
            };

            Assert.Single(mock.LogHistory);
            Assert.Equal(MockStringTop5Event.GetExpectedJson(3, expected), mock.LogHistory[0]);
        }

        /// <summary>
        /// Tests string truncation
        /// </summary>
        [Fact]
        public void TruncateString()
        {
            Init(out var mock, out var logger);

            logger.Event(new MockTruncateStringEvent { Value = "Short" });
            logger.Event(new MockTruncateStringEvent { Value = "ThisStringIsTooLong" });

            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal(MockTruncateStringEvent.GetExpectedJson("Short"), mock.LogHistory[0]);
            Assert.Equal(MockTruncateStringEvent.GetExpectedJson("ThisString..."), mock.LogHistory[1]);
        }

        /// <summary>
        /// Tests all supported data types
        /// </summary>
        [Fact]
        public void AllTypes()
        {
            Init(out var mock, out var logger);

            logger.Event(new MockAllTypesEvent
            {
                String = "Hello World!",
                Int32 = -42,
                Int64 = -1_000_000_000_000,
                UInt32 = 42,
                UInt64 = 1_000_000_000_000,
                Single = 0.1f,
                Double = 0.1,
                Decimal = 0.1m,
                Bool = true,
                Type = typeof(MockAllTypesEvent),
                Guid = Guid.NewGuid()
            });
            logger.Flush();

            Assert.Equal(2, mock.LogHistory.Count);
        }

        /// <summary>
        /// Tests an event disabled via attributes
        /// </summary>
        [Fact]
        public void DisableEvent()
        {
            Init(out var mock, out var logger);

            logger.Event(new MockDisableEvent { Value = "Hello World!" });
            logger.Flush();

            Assert.Empty(mock.LogHistory);
        }

        /// <summary>
        /// Tests an event with one property disabled via attributes
        /// </summary>
        [Fact]
        public void DisableProperty()
        {
            Init(out var mock, out var logger);

            logger.Event(new MockDisablePropertyEvent
            {
                Number = 42,
                String = "Hello World!"
            });
            logger.Flush();

            Assert.Equal(2, mock.LogHistory.Count);
            Assert.Equal("{\"Event\":\"MockDisableProperty\",\"Number\":42}", mock.LogHistory[0]);
            Assert.Equal(
                "{\"Event\":\"MockDisableProperty$\",\"Count\":1,\"Number\":{\"Min\":42,\"Max\":42,\"Sum\":42}}",
                mock.LogHistory[1]);
        }
    }
}
