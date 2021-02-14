// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Xunit;

namespace PennyLogger.Mocks.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="MockPennyLogger"/> class
    /// </summary>
    public class MockPennyLoggerTest
    {
        /// <summary>
        /// Tests an empty event with the mock PennyLogger
        /// </summary>
        [Fact]
        public void EmptyEvent()
        {
            var logger = new MockPennyLogger();
            logger.Event(new MockEmptyEvent());
        }
    }
}
